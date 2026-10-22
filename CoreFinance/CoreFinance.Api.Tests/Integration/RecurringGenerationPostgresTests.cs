using System.Data;
using System.Globalization;
using System.Net;
using System.Text.Json;
using CoreFinance.Api.BackgroundServices;
using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Application.Interfaces;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace CoreFinance.Api.Tests.Integration;

[Trait("Category", "Characterization")]
public sealed class RecurringGenerationPostgresTests
{
    private const string TemplateRoute = "/api/RecurringTransactionTemplate";
    private static readonly DateTimeOffset FebruaryNow = new(2026, 2, 1, 0, 0, 0, TimeSpan.FromHours(7));

    [Fact]
    public async Task Hosted_Start_And_Restart_Should_Catch_Up_All_Owners_And_Elapsed_End_Dates_Despite_Invalid_Template()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var ownerA = await fixture.LoginAsync("generation-host-a");
        using var ownerB = await fixture.LoginAsync("generation-host-b");
        var accountA = await ownerA.CreateAccountAsync("Hosted owner A");
        var accountB = await ownerB.CreateAccountAsync("Hosted owner B");
        var today = fixture.Today;
        var invalid = Template(ownerA.UserId, accountA, today.AddDays(-450), RecurrenceFrequency.Custom);
        invalid.Id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        invalid.CustomIntervalDays = 0; // Legacy stored corruption, deliberately bypassing HTTP validation.
        var backlog = Template(ownerA.UserId, accountA, today.AddDays(-450));
        backlog.Id = Guid.Parse("00000000-0000-0000-0000-000000000002");
        backlog.EndDate = today.AddDays(1);
        var expired = Template(ownerB.UserId, accountB, today.AddDays(-70), RecurrenceFrequency.Weekly);
        expired.EndDate = today.AddDays(-14);
        var inactive = Template(ownerA.UserId, accountA, today.AddDays(-10));
        inactive.IsActive = false;
        var manualOnly = Template(ownerB.UserId, accountB, today.AddDays(-10));
        manualOnly.AutoGenerate = false;
        var deleted = Template(ownerB.UserId, accountB, today.AddDays(-10));
        deleted.IsDeleted = "20260101000000";
        var downtime = Template(ownerB.UserId, accountB, today.AddDays(-30));
        downtime.EndDate = today.AddDays(-3);
        var historical = Period(backlog, backlog.StartDate, 81.25m);
        historical.Status = ExpectedTransactionStatus.Cancelled;
        historical.AdjustmentReason = "Historical period remains reserved";
        await SeedAsync(fixture, invalid, backlog, expired, inactive, manualOnly, deleted);
        await SeedPeriodsAsync(fixture, historical);
        var historicalBefore = await SnapshotsAsync(fixture, [historical.Id]);
        var invalidBefore = await TemplateSnapshotAsync(fixture, invalid.Id);
        await using var commits = await CommitNotifications.CreateAsync(fixture, [backlog.Id, expired.Id, downtime.Id]);

        // No request is made to this worker factory: its actual BackgroundService must perform startup catch-up.
        var firstWorker = await fixture.StartWorkerAsync();
        await commits.WaitForAsync([backlog.Id, expired.Id]);
        await firstWorker.DisposeAsync();

        var backlogRows = await PeriodsAsync(fixture, backlog.Id);
        backlogRows.Select(row => row.ExpectedDate).Should().Equal(
            Enumerable.Range(0, 452).Select(day => backlog.StartDate.AddDays(day)));
        backlogRows.Should().OnlyContain(row => row.UserId == ownerA.UserId && row.AccountId == accountA);
        backlogRows.Where(row => row.Id != historical.Id).Should().OnlyContain(row =>
            row.ExpectedAmount == backlog.Amount && row.Status == ExpectedTransactionStatus.Pending &&
            row.ActualTransactionId == null && row.IsDeleted == null);
        (await TemplateAsync(fixture, backlog.Id)).NextExecutionDate.Should().Be(today.AddDays(2));
        var expiredRows = await PeriodsAsync(fixture, expired.Id);
        expiredRows.Select(row => row.ExpectedDate).Should().Equal(
            Enumerable.Range(0, 9).Select(week => expired.StartDate.AddDays(week * 7)));
        expiredRows.Should().OnlyContain(row => row.UserId == ownerB.UserId && row.AccountId == accountB &&
            row.ExpectedAmount == expired.Amount && row.Status == ExpectedTransactionStatus.Pending);
        (await TemplateAsync(fixture, expired.Id)).NextExecutionDate.Should().Be(today.AddDays(-7));
        await AssertSnapshotsAsync(fixture, historicalBefore);
        (await TemplateSnapshotAsync(fixture, invalid.Id)).Should().Be(invalidBefore);
        foreach (var excluded in new[] { invalid, inactive, manualOnly, deleted })
            (await PeriodsAsync(fixture, excluded.Id)).Should().BeEmpty();
        (await ActualCountAsync(fixture)).Should().Be(0, "the hosted generator must never create payments");

        var firstPassSnapshots = await SnapshotsAsync(fixture, backlogRows.Concat(expiredRows).Select(row => row.Id).ToArray());
        // Seed a cursor left behind during downtime; its EndDate has already elapsed before the next startup.
        await SeedAsync(fixture, downtime);
        var secondWorker = await fixture.StartWorkerAsync();
        await commits.WaitForAsync([downtime.Id]);
        await secondWorker.DisposeAsync();

        var downtimeRows = await PeriodsAsync(fixture, downtime.Id);
        downtimeRows.Select(row => row.ExpectedDate).Should().Equal(
            Enumerable.Range(0, 28).Select(day => downtime.StartDate.AddDays(day)));
        downtimeRows.Should().OnlyContain(row => row.UserId == ownerB.UserId && row.ExpectedAmount == downtime.Amount &&
            row.Status == ExpectedTransactionStatus.Pending && row.ActualTransactionId == null);
        (await TemplateAsync(fixture, downtime.Id)).NextExecutionDate.Should().Be(today.AddDays(-2));
        await AssertSnapshotsAsync(fixture, firstPassSnapshots);
        (await TemplateSnapshotAsync(fixture, invalid.Id)).Should().Be(invalidBefore);
        (await ActualCountAsync(fixture)).Should().Be(0);
    }

    [Fact]
    public async Task RunOnce_Should_Dispose_Failed_Owner_Scope_And_Generate_Valid_Owner_Without_Duplicates()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync(FebruaryNow);
        var owner = Guid.NewGuid();
        var valid = Template(owner, Guid.Empty, fixture.Today.AddDays(-2));
        valid.AccountId = null;
        var legacy = Template(Guid.Empty, Guid.Empty, valid.StartDate);
        legacy.AccountId = null;
        await SeedAsync(fixture, valid, legacy);
        var legacyBefore = await TemplateSnapshotAsync(fixture, legacy.Id);

        List<NpgsqlConnection> scopedConnectionProbes = [];
        await using var factory = fixture.Factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ =>
                {
                    var connection = new NpgsqlConnection(fixture.ConnectionString);
                    try
                    {
                        connection.Open();
                        scopedConnectionProbes.Add(connection);
                        return connection;
                    }
                    catch
                    {
                        connection.Dispose();
                        throw;
                    }
                });
                services.RemoveAll<CurrentUserContext>();
                services.AddScoped(provider =>
                {
                    // The real scope owns this resource before the worker initializes its owner.
                    _ = provider.GetRequiredService<NpgsqlConnection>();
                    return new CurrentUserContext(provider.GetRequiredService<IHttpContextAccessor>());
                });
            }));

        try
        {
            var worker = factory.Services.GetServices<IHostedService>()
                .OfType<RecurringTransactionGenerationService>().Single();
            await worker.RunOnceAsync(CancellationToken.None);
            var activeAfterFirstPass = scopedConnectionProbes.Count(connection => connection.State != ConnectionState.Closed);

            var periods = await PeriodsAsync(fixture, valid.Id);
            periods.Select(row => row.ExpectedDate).Should().Equal(
                Enumerable.Range(0, 4).Select(day => valid.StartDate.AddDays(day)));
            periods.Should().OnlyContain(row => row.UserId == owner && row.AccountId == null &&
                row.ExpectedAmount == valid.Amount && row.Status == ExpectedTransactionStatus.Pending &&
                row.ActualTransactionId == null && row.IsDeleted == null);
            (await TemplateAsync(fixture, valid.Id)).NextExecutionDate.Should().Be(fixture.Today.AddDays(2));
            (await PeriodsAsync(fixture, legacy.Id)).Should().BeEmpty();
            (await ActualCountAsync(fixture)).Should().Be(0);
            var firstPass = await SnapshotsAsync(fixture, periods.Select(row => row.Id).ToArray());

            await worker.RunOnceAsync(CancellationToken.None);
            var activeAfterSecondPass = scopedConnectionProbes.Count(connection => connection.State != ConnectionState.Closed);

            (await PeriodsAsync(fixture, valid.Id)).Select(row => row.Id).Should().Equal(periods.Select(row => row.Id));
            await AssertSnapshotsAsync(fixture, firstPass);
            (await PeriodsAsync(fixture, legacy.Id)).Should().BeEmpty();
            (await TemplateSnapshotAsync(fixture, legacy.Id)).Should().Be(legacyBefore);
            (await ActualCountAsync(fixture)).Should().Be(0);
            scopedConnectionProbes.Should().NotBeEmpty();
            new[] { activeAfterFirstPass, activeAfterSecondPass }.Should().Equal(new[] { 0, 0 },
                "each completed pass must dispose scoped resources even when owner initialization fails");
        }
        finally
        {
            // Clean only this test's probes after observing their lifetime, including on a regression failure.
            foreach (var connection in scopedConnectionProbes)
                await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task One_Hundred_Repeated_And_Concurrent_Manual_Job_And_Update_Runs_Should_Reserve_Create_And_Terminal_Slots()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync(FebruaryNow);
        using var api = await fixture.LoginAsync("generation-hundred");
        var account = await api.CreateAccountAsync("Generation race account");
        var start = fixture.Today.AddDays(-3);
        var end = fixture.Today.AddDays(5);
        var templateId = await CreateTemplateAsync(api, account, start, RecurrenceFrequency.Daily, 3, end);
        var originalRows = await PeriodsAsync(fixture, templateId);
        originalRows.Select(row => row.ExpectedDate).Should().Equal(Enumerable.Range(0, 7).Select(day => start.AddDays(day)));
        await OkAsync(api.RecordPaymentAsync(originalRows[0].Id));
        await OkAsync(api.CancelExpectedAsync(originalRows[1].Id, "Do not regenerate cancelled slot"));
        await OkAsync(api.DeleteAsync($"/api/ExpectedTransaction/{originalRows[2].Id}"));
        var reserved = await SnapshotsAsync(fixture, originalRows.Select(row => row.Id).ToArray());
        var job = fixture.Factory.Services.GetServices<IHostedService>()
            .OfType<RecurringTransactionGenerationService>().Single();

        // Ten waves repeatedly reopen the historical cursor. Each wave is proved blocked in PostgreSQL before release.
        for (var wave = 0; wave < 10; wave++)
        {
            (await fixture.ExecuteSqlAsync(
                "UPDATE recurring_transaction_templates SET next_execution_date = @start WHERE id = @id",
                new NpgsqlParameter("start", start), new NpgsqlParameter("id", templateId))).Should().Be(1);
            await using var held = await HeldTemplateLock.AcquireAsync(fixture, api.UserId, templateId);
            var contenders = Enumerable.Range(wave * 10, 10).Select(index => (index % 3) switch
            {
                0 => OkAsync(api.PostJsonAsync($"{TemplateRoute}/{templateId}/generate-expected-transactions?daysInAdvance=5", new { })),
                1 => job.RunOnceAsync(CancellationToken.None),
                _ => UpdateTemplateAsync(api, templateId, new Dictionary<string, string>
                {
                    ["Amount"] = (200m + index).ToString(CultureInfo.InvariantCulture),
                    ["DaysInAdvance"] = "5",
                    ["EndDate"] = Date(end), ["Description"] = "Edited without rewriting periods"
                })
            }).ToArray();
            await held.WaitForWaitersAsync(10, Task.WhenAll(contenders));
            await held.ReleaseAsync();
            await Task.WhenAll(contenders).WaitAsync(TimeSpan.FromSeconds(60));
        }

        var persisted = await PeriodsAsync(fixture, templateId);
        persisted.Select(row => row.ExpectedDate).Should().Equal(Enumerable.Range(0, 9).Select(day => start.AddDays(day)));
        persisted.GroupBy(row => row.ExpectedDate.Date).Should().OnlyContain(group => group.Count() == 1);
        persisted.Where(row => reserved.ContainsKey(row.Id)).Select(row => row.Id).Should().Equal(originalRows.Select(row => row.Id));
        var added = persisted.Where(row => !reserved.ContainsKey(row.Id)).ToArray();
        added.Select(row => row.ExpectedDate).Should().Equal(new[] { fixture.Today.AddDays(4), fixture.Today.AddDays(5) });
        added.Should().OnlyContain(row => row.UserId == api.UserId && row.AccountId == account &&
            row.Status == ExpectedTransactionStatus.Pending && row.IsDeleted == null && row.ActualTransactionId == null);
        var validAmounts = Enumerable.Range(0, 100).Where(index => index % 3 == 2).Select(index => 200m + index).Append(100m);
        validAmounts.Should().Contain(added[0].ExpectedAmount);
        added[1].ExpectedAmount.Should().Be(added[0].ExpectedAmount, "one committed locked run owns both missing slots");
        added.Should().OnlyContain(row => row.Description ==
            (row.ExpectedAmount == 100m ? "Original description" : "Edited without rewriting periods"));
        persisted[0].Status.Should().Be(ExpectedTransactionStatus.Confirmed);
        persisted[0].ActualTransactionId.Should().NotBeNull();
        persisted[1].Status.Should().Be(ExpectedTransactionStatus.Cancelled);
        persisted[2].IsDeleted.Should().NotBeNullOrEmpty();
        await AssertSnapshotsAsync(fixture, reserved);
        (await TemplateAsync(fixture, templateId)).NextExecutionDate.Should().Be(end.AddDays(1));
        (await ActualCountAsync(fixture)).Should().Be(1, "only the explicit payment, not the 100 generation runs, creates an actual");
    }

    /// <summary>
    ///     (EN) A cross-month PUT preserves the complete period and xmin; a same-month PUT keeps the next anchored slot available.<br />
    ///     (VI) PUT sang tháng khác giữ nguyên toàn bộ kỳ và xmin; PUT cùng tháng vẫn cho phép sinh slot kế tiếp đúng neo.
    /// </summary>
    [Fact]
    public async Task Monthly_Period_Put_Should_Reject_Cross_Month_Without_Durable_Changes_And_Generate_Next_Anchor_After_Same_Month_Move()
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync(FebruaryNow);
        using var api = await fixture.LoginAsync("generation-slot-edit");
        var account = await api.CreateAccountAsync("Monthly slot edit account");
        var templateId = await CreateTemplateAsync(api, account, new DateTime(2026, 1, 31),
            RecurrenceFrequency.Monthly, 1, new DateTime(2026, 2, 28));
        var january = (await PeriodsAsync(fixture, templateId)).Should().ContainSingle().Which;
        january.ExpectedDate.Should().Be(new DateTime(2026, 1, 31));
        var before = await SnapshotsAsync(fixture, [january.Id]);
        var originalVersion = await XminAsync(fixture, january.Id);

        // Act
        using var rejected = await api.PutExpectedAsync(january.Id, new Dictionary<string, string>
        {
            ["Id"] = january.Id.ToString(), ["ExpectedDate"] = "2026-02-02",
            ["ExpectedAmount"] = "150", ["AdjustmentReason"] = "Replacement reading",
            ["Description"] = "Replacement description", ["Category"] = "Replacement category",
            ["Notes"] = "Replacement notes"
        });
        var content = await rejected.Content.ReadAsStringAsync();

        // Assert
        rejected.StatusCode.Should().Be(HttpStatusCode.BadRequest, content);
        using (var body = JsonDocument.Parse(content))
        {
            body.RootElement.EnumerateObject().Select(property => property.Name).Should()
                .BeEquivalentTo(new[] { "code", "message" });
            body.RootElement.GetProperty("code").GetString().Should().Be(CoreFinanceErrorCodes.InvalidRequest);
            body.RootElement.GetProperty("message").GetString().Should().Be(CoreFinanceMessages.ExpectedDateMustStayInSlot);
        }
        await AssertSnapshotsAsync(fixture, before);
        (await XminAsync(fixture, january.Id)).Should().Be(originalVersion);

        await OkAsync(api.PutExpectedAsync(january.Id, new Dictionary<string, string>
        {
            ["Id"] = january.Id.ToString(), ["ExpectedDate"] = "2026-01-25"
        }));
        var moved = (await PeriodsAsync(fixture, templateId)).Should().ContainSingle().Which;
        moved.Id.Should().Be(january.Id);
        moved.ExpectedDate.Should().Be(new DateTime(2026, 1, 25));
        moved.ExpectedAmount.Should().Be(january.ExpectedAmount);
        moved.OriginalAmount.Should().BeNull();
        moved.IsAdjusted.Should().BeFalse();
        moved.Description.Should().Be(january.Description);
        moved.Category.Should().Be(january.Category);
        moved.Notes.Should().Be(january.Notes);
        var afterMove = await SnapshotsAsync(fixture, [january.Id]);

        await OkAsync(api.PostJsonAsync(
            $"{TemplateRoute}/{templateId}/generate-expected-transactions?daysInAdvance=27", new { }));
        var generated = await PeriodsAsync(fixture, templateId);
        generated.Select(row => row.ExpectedDate).Should().Equal(
            new[] { new DateTime(2026, 1, 25), new DateTime(2026, 2, 28) });
        generated.Should().OnlyContain(row => row.UserId == api.UserId && row.AccountId == account &&
            row.ExpectedAmount == 100m && row.Status == ExpectedTransactionStatus.Pending && row.ActualTransactionId == null);
        var february = generated.Single(row => row.ExpectedDate == new DateTime(2026, 2, 28));
        var generatedBeforeRepeat = await SnapshotsAsync(fixture, generated.Select(row => row.Id).ToArray());

        await OkAsync(api.PostJsonAsync(
            $"{TemplateRoute}/{templateId}/generate-expected-transactions?daysInAdvance=27", new { }));
        var repeated = await PeriodsAsync(fixture, templateId);
        repeated.Should().HaveCount(2);
        repeated.Count(row => row.ExpectedDate == new DateTime(2026, 2, 28)).Should().Be(1);
        repeated.Single(row => row.ExpectedDate == new DateTime(2026, 2, 28)).Id.Should().Be(february.Id);
        await AssertSnapshotsAsync(fixture, afterMove);
        await AssertSnapshotsAsync(fixture, generatedBeforeRepeat);
        (await TemplateAsync(fixture, templateId)).NextExecutionDate.Should().Be(new DateTime(2026, 3, 31));
        (await ActualCountAsync(fixture)).Should().Be(0);
    }

    /// <summary>
    ///     (EN) A period without a recurring template accepts a cross-month PUT through the authenticated real API.<br />
    ///     (VI) Kỳ không gắn mẫu định kỳ chấp nhận PUT sang tháng khác qua API thật đã xác thực.
    /// </summary>
    [Fact]
    public async Task Unbound_Period_Put_Should_Allow_Cross_Month_Move_Without_Changing_Amount_Or_Metadata()
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync(FebruaryNow);
        using var api = await fixture.LoginAsync("generation-unbound-edit");
        var account = await api.CreateAccountAsync("Unbound date edit account");
        var id = await api.CreateExpectedAsync(new DateTime(2026, 1, 31), 125m, accountId: account,
            description: "Unbound invoice", category: "Utilities");

        // Act
        await OkAsync(api.PutExpectedAsync(id, new Dictionary<string, string>
        {
            ["Id"] = id.ToString(), ["ExpectedDate"] = "2026-02-02"
        }));

        // Assert
        var stored = await fixture.ReadAsync(database => database.ExpectedTransactions.AsNoTracking()
            .SingleAsync(row => row.Id == id));
        stored.ExpectedDate.Should().Be(new DateTime(2026, 2, 2));
        stored.RecurringTransactionTemplateId.Should().BeNull();
        stored.UserId.Should().Be(api.UserId);
        stored.AccountId.Should().Be(account);
        stored.ExpectedAmount.Should().Be(125m);
        stored.Description.Should().Be("Unbound invoice");
        stored.Category.Should().Be("Utilities");
        stored.IsAdjusted.Should().BeFalse();
        stored.OriginalAmount.Should().BeNull();
        stored.Status.Should().Be(ExpectedTransactionStatus.Pending);
        (await ActualCountAsync(fixture)).Should().Be(0);
    }

    /// <summary>
    ///     (EN) Resupplying EndDate on an amount edit keeps its inclusive bound during immediate generation.<br />
    ///     (VI) Gửi lại EndDate khi sửa số tiền giữ biên bao gồm ngày kết thúc trong lần sinh ngay sau cập nhật.
    /// </summary>
    [Fact]
    public async Task Template_Amount_Put_Should_Preserve_Explicit_EndDate_And_Stop_Immediate_Generation_At_That_Bound()
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync(FebruaryNow);
        using var api = await fixture.LoginAsync("generation-explicit-end");
        var account = await api.CreateAccountAsync("Explicit end date account");
        var template = Template(api.UserId, account, fixture.Today.AddDays(-2));
        template.EndDate = fixture.Today.AddDays(1);
        template.DaysInAdvance = 10;
        await SeedAsync(fixture, template);
        (await PeriodsAsync(fixture, template.Id)).Should().BeEmpty();

        // Act
        await UpdateTemplateAsync(api, template.Id, new Dictionary<string, string>
        {
            ["Amount"] = "222.22", ["EndDate"] = Date(template.EndDate.Value)
        });

        // Assert
        var stored = await TemplateAsync(fixture, template.Id);
        stored.Amount.Should().Be(222.22m);
        stored.EndDate.Should().Be(template.EndDate);
        stored.DaysInAdvance.Should().Be(10);
        stored.NextExecutionDate.Should().Be(fixture.Today.AddDays(2));
        var rows = await PeriodsAsync(fixture, template.Id);
        rows.Select(row => row.ExpectedDate).Should().Equal(
            Enumerable.Range(0, 4).Select(day => template.StartDate.AddDays(day)));
        rows.Should().OnlyContain(row => row.ExpectedDate <= template.EndDate.Value &&
            row.ExpectedAmount == 222.22m && row.UserId == api.UserId && row.AccountId == account &&
            row.Status == ExpectedTransactionStatus.Pending && row.ActualTransactionId == null);
        (await ActualCountAsync(fixture)).Should().Be(0);
    }

    /// <summary>
    ///     (EN) Amount-only template PUT clears all omitted optional fields, including EndDate, before immediate generation.<br />
    ///     (VI) PUT mẫu chỉ gửi số tiền xóa mọi trường tùy chọn bị bỏ trống, gồm EndDate, trước khi sinh ngay.
    /// </summary>
    [Fact]
    public async Task Template_Amount_Only_Put_Should_Clear_Omitted_Optional_Fields_And_Generate_Beyond_Previous_EndDate()
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync(FebruaryNow);
        using var api = await fixture.LoginAsync("generation-omitted-end");
        var account = await api.CreateAccountAsync("Omitted end date account");
        var template = Template(api.UserId, account, fixture.Today.AddDays(-2));
        template.EndDate = fixture.Today.AddDays(1);
        template.CronExpression = "0 0 * * *";
        template.DaysInAdvance = 10;
        await SeedAsync(fixture, template);
        var before = await TemplateAsync(fixture, template.Id);
        before.EndDate.Should().Be(template.EndDate);
        before.Description.Should().NotBeNullOrEmpty();
        before.Category.Should().NotBeNullOrEmpty();
        before.Notes.Should().NotBeNullOrEmpty();
        before.CronExpression.Should().NotBeNullOrEmpty();

        // Act
        await UpdateTemplateAsync(api, template.Id, new Dictionary<string, string> { ["Amount"] = "333.33" });

        // Assert
        var stored = await TemplateAsync(fixture, template.Id);
        stored.Amount.Should().Be(333.33m);
        stored.EndDate.Should().BeNull();
        stored.Description.Should().BeNull();
        stored.Category.Should().BeNull();
        stored.Notes.Should().BeNull();
        stored.CronExpression.Should().BeNull();
        stored.Name.Should().Be(before.Name);
        stored.StartDate.Should().Be(before.StartDate);
        stored.Frequency.Should().Be(before.Frequency);
        stored.DaysInAdvance.Should().Be(before.DaysInAdvance);
        stored.IsActive.Should().BeTrue();
        stored.AutoGenerate.Should().BeTrue();
        stored.NextExecutionDate.Should().Be(fixture.Today.AddDays(11));
        var rows = await PeriodsAsync(fixture, template.Id);
        rows.Select(row => row.ExpectedDate).Should().Equal(
            Enumerable.Range(0, 13).Select(day => template.StartDate.AddDays(day)));
        rows.Should().Contain(row => row.ExpectedDate > before.EndDate!.Value);
        rows.Should().OnlyContain(row => row.ExpectedAmount == 333.33m && row.Description == null &&
            row.Category == null && row.UserId == api.UserId && row.AccountId == account &&
            row.Status == ExpectedTransactionStatus.Pending && row.ActualTransactionId == null);
        (await ActualCountAsync(fixture)).Should().Be(0);
    }

    [Fact]
    public async Task Monthly_31st_Should_Persist_Twelve_Anchored_Periods_Without_Drift()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync(
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(7)));
        using var api = await fixture.LoginAsync("generation-month31");
        var account = await api.CreateAccountAsync("Month-end account");
        var templateId = await CreateTemplateAsync(api, account, new DateTime(2026, 1, 31),
            RecurrenceFrequency.Monthly, 365, new DateTime(2026, 12, 31), 123.45m);

        var rows = await PeriodsAsync(fixture, templateId);
        rows.Select(row => row.ExpectedDate).Should().Equal(new[]
        {
            new DateTime(2026, 1, 31), new DateTime(2026, 2, 28), new DateTime(2026, 3, 31),
            new DateTime(2026, 4, 30), new DateTime(2026, 5, 31), new DateTime(2026, 6, 30),
            new DateTime(2026, 7, 31), new DateTime(2026, 8, 31), new DateTime(2026, 9, 30),
            new DateTime(2026, 10, 31), new DateTime(2026, 11, 30), new DateTime(2026, 12, 31)
        });
        rows.Should().OnlyContain(row => row.UserId == api.UserId && row.AccountId == account &&
            row.ExpectedAmount == 123.45m && row.Status == ExpectedTransactionStatus.Pending && row.ActualTransactionId == null);
        (await TemplateAsync(fixture, templateId)).NextExecutionDate.Should().Be(new DateTime(2027, 1, 31));
        (await ActualCountAsync(fixture)).Should().Be(0);
    }

    [Fact]
    public async Task Annual_Leap_Anchor_Should_Recover_February_29_After_Non_Leap_Years()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync(
            new DateTimeOffset(2028, 3, 1, 0, 0, 0, TimeSpan.FromHours(7)));
        using var api = await fixture.LoginAsync("generation-leap");
        var account = await api.CreateAccountAsync("Leap-year account");
        var templateId = await CreateTemplateAsync(api, account, new DateTime(2024, 2, 29),
            RecurrenceFrequency.Annually, 1, new DateTime(2028, 2, 29), 555.55m);

        var rows = await PeriodsAsync(fixture, templateId);
        rows.Select(row => row.ExpectedDate).Should().Equal(new[]
        {
            new DateTime(2024, 2, 29), new DateTime(2025, 2, 28), new DateTime(2026, 2, 28),
            new DateTime(2027, 2, 28), new DateTime(2028, 2, 29)
        });
        rows.Should().OnlyContain(row => row.UserId == api.UserId && row.ExpectedAmount == 555.55m &&
            row.Status == ExpectedTransactionStatus.Pending && row.ActualTransactionId == null);
        (await TemplateAsync(fixture, templateId)).NextExecutionDate.Should().Be(new DateTime(2029, 2, 28));
        (await ActualCountAsync(fixture)).Should().Be(0);
    }

    [Fact]
    public async Task Legacy_Drifted_Cursor_Should_Realign_While_Keeping_Existing_Month_Slots_Byte_For_Byte()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync(
            new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.FromHours(7)));
        using var api = await fixture.LoginAsync("generation-drift");
        var account = await api.CreateAccountAsync("Legacy cursor account");
        var template = Template(api.UserId, account, new DateTime(2026, 1, 31), RecurrenceFrequency.Monthly);
        template.NextExecutionDate = new DateTime(2026, 3, 28);
        var january = Period(template, new DateTime(2026, 1, 31), 11m);
        var february = Period(template, new DateTime(2026, 2, 28), 22m);
        var driftedMarch = Period(template, new DateTime(2026, 3, 28), 33m);
        driftedMarch.Status = ExpectedTransactionStatus.Cancelled;
        driftedMarch.AdjustmentReason = "Keep the legacy March slot";
        await SeedAsync(fixture, template);
        await SeedPeriodsAsync(fixture, january, february, driftedMarch);
        var before = await SnapshotsAsync(fixture, [january.Id, february.Id, driftedMarch.Id]);

        await OkAsync(api.PostJsonAsync($"{TemplateRoute}/{template.Id}/generate-expected-transactions", new { }));
        (await PeriodsAsync(fixture, template.Id)).Select(row => row.ExpectedDate).Should().Equal(new[]
        {
            new DateTime(2026, 1, 31), new DateTime(2026, 2, 28), new DateTime(2026, 3, 28),
            new DateTime(2026, 4, 30), new DateTime(2026, 5, 31)
        });
        (await TemplateAsync(fixture, template.Id)).NextExecutionDate.Should().Be(new DateTime(2026, 6, 30));
        await OkAsync(api.PostJsonAsync(
            $"{TemplateRoute}/{template.Id}/generate-expected-transactions?daysInAdvance=60", new { }));
        var persisted = await PeriodsAsync(fixture, template.Id);
        persisted.Where(row => !before.ContainsKey(row.Id)).Select(row => row.ExpectedDate).Should().Equal(new[]
        {
            new DateTime(2026, 4, 30), new DateTime(2026, 5, 31), new DateTime(2026, 6, 30), new DateTime(2026, 7, 31)
        });
        persisted.Where(row => !before.ContainsKey(row.Id)).Should().OnlyContain(row =>
            row.ExpectedAmount == template.Amount && row.UserId == api.UserId && row.Status == ExpectedTransactionStatus.Pending);
        (await TemplateAsync(fixture, template.Id)).NextExecutionDate.Should().Be(new DateTime(2026, 8, 31));
        await AssertSnapshotsAsync(fixture, before);
        (await ActualCountAsync(fixture)).Should().Be(0);
    }

    [Fact]
    public async Task Quarterly_To_Monthly_Should_Fill_February_And_March_Without_Rewriting_Old_Future_Or_Tombstone_Rows()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync(FebruaryNow);
        using var api = await fixture.LoginAsync("generation-frequency-edit");
        var account = await api.CreateAccountAsync("Frequency edit account");
        var templateId = await CreateTemplateAsync(api, account, new DateTime(2026, 1, 31),
            RecurrenceFrequency.Quarterly, 365, new DateTime(2026, 12, 31));
        var old = await PeriodsAsync(fixture, templateId);
        old.Select(row => row.ExpectedDate).Should().Equal(new[]
        {
            new DateTime(2026, 1, 31), new DateTime(2026, 4, 30), new DateTime(2026, 7, 31), new DateTime(2026, 10, 31)
        });
        await OkAsync(api.CancelExpectedAsync(old[0].Id, "Closed historical January"));
        await OkAsync(api.AdjustExpectedAsync(old[1].Id, 77.77m, "Keep adjusted future period"));
        (await fixture.ExecuteSqlAsync(
            "UPDATE expected_transactions SET expected_date = @date, notes = @notes WHERE id = @id",
            new NpgsqlParameter("date", new DateTime(2026, 4, 28)),
            new NpgsqlParameter("notes", "Legacy future date is not a schedule-edit target"),
            new NpgsqlParameter("id", old[1].Id))).Should().Be(1);
        await OkAsync(api.DeleteAsync($"/api/ExpectedTransaction/{old[2].Id}"));
        await OkAsync(api.RecordPaymentAsync(old[3].Id));
        var before = await SnapshotsAsync(fixture, old.Select(row => row.Id).ToArray());

        await UpdateTemplateAsync(api, templateId, new Dictionary<string, string>
        {
            ["Frequency"] = "Monthly", ["Amount"] = "200", ["DaysInAdvance"] = "90",
            ["EndDate"] = "2026-12-31", ["Description"] = "New schedule description"
        });

        var rows = await PeriodsAsync(fixture, templateId);
        rows.Should().HaveCount(6);
        var added = rows.Where(row => !before.ContainsKey(row.Id)).ToArray();
        added.Select(row => row.ExpectedDate).Should().Equal(new[] { new DateTime(2026, 2, 28), new DateTime(2026, 3, 31) });
        added.Should().OnlyContain(row => row.ExpectedAmount == 200m && row.UserId == api.UserId &&
            row.Status == ExpectedTransactionStatus.Pending && row.Description == "New schedule description");
        rows.Count(row => row.ExpectedDate.Month == 4).Should().Be(1, "an existing drifted future period reserves its month");
        rows.Single(row => row.Id == old[2].Id).IsDeleted.Should().NotBeNullOrEmpty();
        (await TemplateAsync(fixture, templateId)).NextExecutionDate.Should().Be(new DateTime(2026, 5, 31));
        await AssertSnapshotsAsync(fixture, before);
        (await ActualCountAsync(fixture)).Should().Be(1);
    }

    [Fact]
    public async Task Amount_Only_Edit_Should_Not_Reset_A_Progressed_Cursor_Or_Reopen_Processed_History()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync(FebruaryNow);
        using var api = await fixture.LoginAsync("generation-amount-only");
        var account = await api.CreateAccountAsync("Amount-only account");
        var template = Template(api.UserId, account, new DateTime(2026, 1, 31), RecurrenceFrequency.Monthly);
        template.NextExecutionDate = new DateTime(2026, 7, 31);
        var historical = Period(template, template.StartDate, 81m);
        await SeedAsync(fixture, template);
        await SeedPeriodsAsync(fixture, historical);
        var before = await SnapshotsAsync(fixture, [historical.Id]);

        await UpdateTemplateAsync(api, template.Id, new Dictionary<string, string> { ["Amount"] = "222.22" });
        var after = await TemplateAsync(fixture, template.Id);
        after.NextExecutionDate.Should().Be(new DateTime(2026, 7, 31));
        after.StartDate.Should().Be(template.StartDate);
        after.Frequency.Should().Be(RecurrenceFrequency.Monthly);
        after.Amount.Should().Be(222.22m);
        (await PeriodsAsync(fixture, template.Id)).Should().ContainSingle();
        await AssertSnapshotsAsync(fixture, before);

        await OkAsync(api.PostJsonAsync(
            $"{TemplateRoute}/{template.Id}/generate-expected-transactions?daysInAdvance=220", new { }));
        var added = (await PeriodsAsync(fixture, template.Id)).Where(row => row.Id != historical.Id).ToArray();
        added.Select(row => row.ExpectedDate).Should().Equal(new[] { new DateTime(2026, 7, 31), new DateTime(2026, 8, 31) });
        added.Should().OnlyContain(row => row.ExpectedAmount == 222.22m && row.UserId == api.UserId);
        (await TemplateAsync(fixture, template.Id)).NextExecutionDate.Should().Be(new DateTime(2026, 9, 30));
        await AssertSnapshotsAsync(fixture, before);
        (await ActualCountAsync(fixture)).Should().Be(0);
    }

    [Theory]
    [InlineData("edit", true)]
    [InlineData("edit", false)]
    [InlineData("toggle", true)]
    [InlineData("toggle", false)]
    [InlineData("delete", true)]
    [InlineData("delete", false)]
    public async Task Template_Mutation_And_Generation_Should_Serialize_Both_Orders_Without_Stale_Metadata_Or_Resurrection(
        string mutation, bool generationFirst)
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync(FebruaryNow);
        using var api = await fixture.LoginAsync($"generation-{mutation}-race");
        var account = await api.CreateAccountAsync("Template mutation race account");
        var template = Template(api.UserId, account, fixture.Today.AddDays(-2));
        template.EndDate = fixture.Today.AddDays(1);
        var historical = Period(template, fixture.Today.AddDays(-1), 91m);
        historical.IsAdjusted = true;
        historical.OriginalAmount = 100m;
        historical.AdjustmentReason = "Existing period must not change";
        await SeedAsync(fixture, template);
        await SeedPeriodsAsync(fixture, historical);
        var before = await SnapshotsAsync(fixture, [historical.Id]);
        await using var held = await HeldTemplateLock.AcquireAsync(fixture, api.UserId, template.Id);
        Task Generate() => fixture.WithOwnerAsync(api.UserId, services =>
            services.GetRequiredService<IRecurringTransactionTemplateService>().GenerateExpectedTransactionsAsync(template.Id));
        async Task Mutate()
        {
            await fixture.WithOwnerAsync(api.UserId, async services =>
            {
                var service = services.GetRequiredService<IRecurringTransactionTemplateService>();
                switch (mutation)
                {
                    case "edit":
                        var result = await service.UpdateAsync(template.Id, new RecurringTransactionTemplateUpdateRequest
                        {
                            Id = template.Id, Name = "Edited name", Amount = 321m,
                            Description = "Edited description", Category = "Edited category", Notes = "Edited notes",
                            EndDate = template.EndDate
                        });
                        result.Should().NotBeNull();
                        break;
                    case "toggle":
                        (await service.ToggleActiveStatusAsync(template.Id, false)).Should().BeTrue();
                        break;
                    case "delete":
                        (await service.DeleteTemplateAsync(template.Id)).Should().BeTrue();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mutation));
                }
            });
        }

        var first = generationFirst ? Generate() : Mutate();
        await held.WaitForWaitersAsync(1, first);
        var second = generationFirst ? Mutate() : Generate();
        await held.WaitForWaitersAsync(2, Task.WhenAll(first, second));
        await held.ReleaseAsync();
        await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(30));

        var persisted = await TemplateAsync(fixture, template.Id);
        persisted.Name.Should().Be(mutation == "edit" ? "Edited name" : template.Name);
        persisted.Amount.Should().Be(mutation == "edit" ? 321m : template.Amount);
        persisted.Description.Should().Be(mutation == "edit" ? "Edited description" : template.Description);
        persisted.Category.Should().Be(mutation == "edit" ? "Edited category" : template.Category);
        persisted.Notes.Should().Be(mutation == "edit" ? "Edited notes" : template.Notes);
        persisted.StartDate.Should().Be(template.StartDate);
        persisted.EndDate.Should().Be(template.EndDate);
        persisted.Frequency.Should().Be(template.Frequency);
        persisted.IsActive.Should().Be(mutation != "toggle");
        if (mutation == "delete") persisted.IsDeleted.Should().NotBeNullOrEmpty();
        else persisted.IsDeleted.Should().BeNull();
        var rows = await PeriodsAsync(fixture, template.Id);
        var generationAllowed = generationFirst || mutation == "edit";
        rows.Should().HaveCount(generationAllowed ? 4 : 1);
        if (generationAllowed)
        {
            rows.Select(row => row.ExpectedDate).Should().Equal(Enumerable.Range(0, 4).Select(day => template.StartDate.AddDays(day)));
            rows.Where(row => row.Id != historical.Id).Should().OnlyContain(row =>
                row.ExpectedAmount == (mutation == "edit" && !generationFirst ? 321m : template.Amount) &&
                row.Description == (mutation == "edit" && !generationFirst ? "Edited description" : template.Description) &&
                row.UserId == api.UserId && row.Status == ExpectedTransactionStatus.Pending && row.IsDeleted == null);
            persisted.NextExecutionDate.Should().Be(fixture.Today.AddDays(2));
        }
        else persisted.NextExecutionDate.Should().Be(template.NextExecutionDate);
        await AssertSnapshotsAsync(fixture, before);
        (await ActualCountAsync(fixture)).Should().Be(0);
    }

    [Fact]
    public async Task Cancellation_While_Advisory_Lock_Is_Held_Should_Propagate_Without_Inserts_And_Allow_A_Fresh_Run()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync(FebruaryNow);
        using var api = await fixture.LoginAsync("generation-cancel-lock");
        var account = await api.CreateAccountAsync("Cancellation account");
        var template = Template(api.UserId, account, new DateTime(2026, 1, 1));
        await SeedAsync(fixture, template);
        var before = await TemplateSnapshotAsync(fixture, template.Id);
        await using var held = await HeldTemplateLock.AcquireAsync(fixture, api.UserId, template.Id);
        using var cancellation = new CancellationTokenSource();
        var generation = fixture.WithOwnerAsync(api.UserId, services =>
            services.GetRequiredService<IRecurringTransactionTemplateService>()
                .GenerateExpectedTransactionsAsync(template.Id, null, cancellation.Token));
        await held.WaitForWaitersAsync(1, generation);

        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => generation.WaitAsync(TimeSpan.FromSeconds(10)));
        (await PeriodsAsync(fixture, template.Id)).Should().BeEmpty();
        (await TemplateSnapshotAsync(fixture, template.Id)).Should().Be(before);
        await held.ReleaseAsync();
        await fixture.WithOwnerAsync(api.UserId, services =>
            services.GetRequiredService<IRecurringTransactionTemplateService>().GenerateExpectedTransactionsAsync(template.Id));

        var rows = await PeriodsAsync(fixture, template.Id);
        rows.Select(row => row.ExpectedDate).Should().Equal(
            Enumerable.Range(0, 33).Select(day => new DateTime(2026, 1, 1).AddDays(day)));
        rows.Should().OnlyContain(row => row.ExpectedAmount == template.Amount && row.UserId == api.UserId &&
            row.Status == ExpectedTransactionStatus.Pending && row.ActualTransactionId == null);
        (await TemplateAsync(fixture, template.Id)).NextExecutionDate.Should().Be(new DateTime(2026, 2, 3));
        (await ActualCountAsync(fixture)).Should().Be(0);
    }

    [Theory]
    [InlineData("confirmed")]
    [InlineData("cancelled")]
    [InlineData("deleted")]
    public async Task PostgreSql_Template_Day_Unique_Constraint_Should_Reserve_Terminal_And_Deleted_Rows_At_Any_Time_Of_Day(string terminal)
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync(FebruaryNow);
        using var api = await fixture.LoginAsync($"generation-unique-{terminal}");
        var account = await api.CreateAccountAsync("Unique slot account");
        var templateId = await CreateTemplateAsync(api, account, fixture.Today, RecurrenceFrequency.Daily, 1, fixture.Today);
        var row = (await PeriodsAsync(fixture, templateId)).Single();
        switch (terminal)
        {
            case "confirmed": await OkAsync(api.RecordPaymentAsync(row.Id)); break;
            case "cancelled": await OkAsync(api.CancelExpectedAsync(row.Id, "Reserved cancelled day")); break;
            case "deleted": await OkAsync(api.DeleteAsync($"/api/ExpectedTransaction/{row.Id}")); break;
        }
        var before = await SnapshotsAsync(fixture, [row.Id]);
        var duplicateId = Guid.NewGuid();

        var violation = await Assert.ThrowsAsync<PostgresException>(() => InsertDuplicateDayAsync(fixture, row.Id, duplicateId));
        violation.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
        violation.ConstraintName.Should().Be("ux_expected_template_day");
        (await PeriodsAsync(fixture, templateId)).Should().ContainSingle().Which.Id.Should().Be(row.Id);
        (await fixture.ReadAsync(database => database.ExpectedTransactions.IgnoreQueryFilters()
            .AnyAsync(period => period.Id == duplicateId))).Should().BeFalse();
        await AssertSnapshotsAsync(fixture, before);
        (await ActualCountAsync(fixture)).Should().Be(terminal == "confirmed" ? 1 : 0);
    }

    [Fact]
    public async Task PostgreSql_Actual_Link_Unique_Constraint_Should_Reserve_A_Legacy_Tombstone_Link()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync(FebruaryNow);
        using var api = await fixture.LoginAsync("generation-unique-actual");
        var account = await api.CreateAccountAsync("Unique actual account");
        var paidId = await api.CreateExpectedAsync(fixture.Today, 51m, accountId: account);
        var otherId = await api.CreateExpectedAsync(fixture.Today.AddDays(1), 61m, accountId: account);
        await OkAsync(api.RecordPaymentAsync(paidId));
        var paid = await fixture.ReadAsync(database => database.ExpectedTransactions.AsNoTracking().SingleAsync(row => row.Id == paidId));
        paid.ActualTransactionId.Should().NotBeNull();
        (await fixture.ExecuteSqlAsync("UPDATE expected_transactions SET is_deleted = @deleted WHERE id = @id",
            new NpgsqlParameter("deleted", "20260201000000"), new NpgsqlParameter("id", paidId))).Should().Be(1);
        var before = await SnapshotsAsync(fixture, [paidId, otherId]);

        var violation = await Assert.ThrowsAsync<PostgresException>(() => fixture.ExecuteSqlAsync(
            "UPDATE expected_transactions SET actual_transaction_id = @actual, status = @status WHERE id = @id",
            new NpgsqlParameter("actual", paid.ActualTransactionId!.Value),
            new NpgsqlParameter("status", (int)ExpectedTransactionStatus.Confirmed), new NpgsqlParameter("id", otherId)));
        violation.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
        violation.ConstraintName.Should().Be("ux_expected_actual_transaction");
        await AssertSnapshotsAsync(fixture, before);
        var links = await fixture.ReadAsync(database => database.ExpectedTransactions.IgnoreQueryFilters().AsNoTracking()
            .Where(row => row.ActualTransactionId == paid.ActualTransactionId).ToListAsync());
        links.Should().ContainSingle().Which.Id.Should().Be(paidId);
        links[0].IsDeleted.Should().NotBeNullOrEmpty();
        (await ActualCountAsync(fixture)).Should().Be(1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Real_Xmin_Should_Reject_A_Stale_Update_Or_Soft_Delete_And_Never_Be_A_Physical_Migration_Column(bool deleteFirst)
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync(FebruaryNow);
        using var api = await fixture.LoginAsync("generation-xmin");
        var id = await api.CreateExpectedAsync(fixture.Today, 75m, description: "Original metadata");
        var originalVersion = await XminAsync(fixture, id);
        var updateLoaded = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var deleteLoaded = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseUpdate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseDelete = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<int> Write(bool deleting, TaskCompletionSource<bool> loaded, TaskCompletionSource<bool> release) =>
            fixture.WithOwnerAsync(api.UserId, async services =>
            {
                var repository = services.GetRequiredService<IBaseRepository<ExpectedTransaction, Guid>>();
                var tracked = await repository.GetByIdAsync(id);
                tracked.Should().NotBeNull();
                if (deleting) tracked!.IsDeleted = "20260201010101";
                else
                {
                    tracked!.ExpectedAmount = 95m;
                    tracked.Description = "Committed metadata";
                    tracked.Notes = "Committed notes";
                }
                loaded.SetResult(true);
                await release.Task.WaitAsync(TimeSpan.FromSeconds(30));
                return await repository.UpdateAsync(tracked!);
            });
        var updating = Write(false, updateLoaded, releaseUpdate);
        var deleting = Write(true, deleteLoaded, releaseDelete);
        await Task.WhenAll(updateLoaded.Task, deleteLoaded.Task).WaitAsync(TimeSpan.FromSeconds(30));

        if (deleteFirst) releaseDelete.SetResult(true);
        else releaseUpdate.SetResult(true);
        (await (deleteFirst ? deleting : updating)).Should().Be(1);
        if (deleteFirst) releaseUpdate.SetResult(true);
        else releaseDelete.SetResult(true);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => deleteFirst ? updating : deleting);

        var persisted = await fixture.ReadAsync(database => database.ExpectedTransactions.IgnoreQueryFilters().AsNoTracking()
            .SingleAsync(row => row.Id == id));
        persisted.UserId.Should().Be(api.UserId);
        persisted.ExpectedDate.Should().Be(fixture.Today);
        persisted.ExpectedAmount.Should().Be(deleteFirst ? 75m : 95m);
        persisted.Description.Should().Be(deleteFirst ? "Original metadata" : "Committed metadata");
        persisted.Notes.Should().Be(deleteFirst ? null : "Committed notes");
        persisted.Status.Should().Be(ExpectedTransactionStatus.Pending);
        if (deleteFirst) persisted.IsDeleted.Should().Be("20260201010101");
        else persisted.IsDeleted.Should().BeNull();
        var committedVersion = await XminAsync(fixture, id);
        committedVersion.Should().NotBe(originalVersion);
        (await fixture.ExecuteSqlAsync(
            "UPDATE expected_transactions SET expected_amount = 999, is_deleted = NULL " +
            "WHERE id = @id AND xmin::text::bigint = @version",
            new NpgsqlParameter("id", id), new NpgsqlParameter("version", originalVersion))).Should().Be(0);
        (await XminAsync(fixture, id)).Should().Be(committedVersion);
        if (!deleteFirst)
        {
            await OkAsync(api.DeleteAsync($"/api/ExpectedTransaction/{id}"));
            (await XminAsync(fixture, id)).Should().NotBe(committedVersion, "a real soft delete advances the system row version");
            var tombstone = await fixture.ReadAsync(database => database.ExpectedTransactions.IgnoreQueryFilters().AsNoTracking()
                .SingleAsync(row => row.Id == id));
            tombstone.IsDeleted.Should().NotBeNullOrEmpty();
            tombstone.ExpectedAmount.Should().Be(95m);
            tombstone.Description.Should().Be("Committed metadata");
        }

        await using var connection = await fixture.OpenConnectionAsync();
        await using var column = new NpgsqlCommand(
            "SELECT attnum FROM pg_attribute WHERE attrelid = 'expected_transactions'::regclass AND attname = 'xmin'", connection);
        Convert.ToInt32(await column.ExecuteScalarAsync(), CultureInfo.InvariantCulture).Should().BeLessThan(0,
            "the applied migrations must leave xmin as a PostgreSQL system attribute, never a user column");
        (await ActualCountAsync(fixture)).Should().Be(0);
    }

    [Fact]
    public async Task Own_Fixture_Rollout_And_Worker_Stop_Should_Keep_Authenticated_Hardened_Api_Additive_Constraints_And_Data()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var ownerA = await fixture.LoginAsync("generation-rollout-a");
        using var ownerB = await fixture.LoginAsync("generation-rollout-b");
        var accountA = await ownerA.CreateAccountAsync("Rollout account A");
        var accountB = await ownerB.CreateAccountAsync("Rollout account B");
        var templateA = Template(ownerA.UserId, accountA, fixture.Today.AddDays(-2));
        var templateB = Template(ownerB.UserId, accountB, fixture.Today.AddDays(-1));
        templateA.EndDate = fixture.Today.AddDays(1);
        templateB.EndDate = fixture.Today.AddDays(1);
        await SeedAsync(fixture, templateA, templateB);
        using (var before = await ownerA.GetAsync("/api/ExpectedTransaction/reminders"))
        {
            before.StatusCode.Should().Be(HttpStatusCode.OK);
            using var body = JsonDocument.Parse(await before.Content.ReadAsStringAsync());
            body.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
            body.RootElement.GetProperty("totalRow").GetInt32().Should().Be(0);
        }
        (await ActualCountAsync(fixture)).Should().Be(0);
        await using var commits = await CommitNotifications.CreateAsync(fixture, [templateA.Id, templateB.Id]);

        var enabledWorker = await fixture.StartWorkerAsync();
        await commits.WaitForAsync([templateA.Id, templateB.Id]);
        var rowsA = await PeriodsAsync(fixture, templateA.Id);
        var rowsB = await PeriodsAsync(fixture, templateB.Id);
        rowsA.Select(row => row.ExpectedDate).Should().Equal(Enumerable.Range(0, 4).Select(day => templateA.StartDate.AddDays(day)));
        rowsB.Select(row => row.ExpectedDate).Should().Equal(Enumerable.Range(0, 3).Select(day => templateB.StartDate.AddDays(day)));
        var persistedBeforeStop = await SnapshotsAsync(fixture, rowsA.Concat(rowsB).Select(row => row.Id).ToArray());
        await enabledWorker.DisposeAsync(); // Worker rollback only; the same hardened API and additive schema stay deployed.

        using (var after = await ownerA.GetAsync("/api/ExpectedTransaction/reminders"))
        {
            after.StatusCode.Should().Be(HttpStatusCode.OK);
            using var body = JsonDocument.Parse(await after.Content.ReadAsStringAsync());
            body.RootElement.GetProperty("data").EnumerateArray().Select(row => row.GetProperty("id").GetGuid())
                .Should().BeEquivalentTo(rowsA.Select(row => row.Id));
            body.RootElement.GetProperty("totalRow").GetInt32().Should().Be(rowsA.Count);
            body.RootElement.GetProperty("data").EnumerateArray().Should()
                .OnlyContain(row => row.GetProperty("userId").GetGuid() == ownerA.UserId);
        }
        using (var foreign = await ownerB.GetAsync($"/api/ExpectedTransaction/{rowsA[0].Id}"))
            foreign.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var duplicateId = Guid.NewGuid();
        var violation = await Assert.ThrowsAsync<PostgresException>(() => InsertDuplicateDayAsync(fixture, rowsA[0].Id, duplicateId));
        violation.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
        violation.ConstraintName.Should().Be("ux_expected_template_day");
        await AssertSnapshotsAsync(fixture, persistedBeforeStop);
        (await ActualCountAsync(fixture)).Should().Be(0);
    }

    private static RecurringTransactionTemplate Template(Guid owner, Guid account, DateTime start,
        RecurrenceFrequency frequency = RecurrenceFrequency.Daily) => new()
    {
        Id = Guid.NewGuid(), UserId = owner, AccountId = account, Name = "Persisted generation template",
        Description = "Original description", Category = "Original category", Notes = "Original notes", Amount = 100m,
        TransactionType = RecurringTransactionType.Expense, Frequency = frequency,
        StartDate = start, NextExecutionDate = start, IsActive = true, AutoGenerate = true, DaysInAdvance = 1,
        CreatedAt = new DateTime(2026, 1, 1), UpdatedAt = new DateTime(2026, 1, 1), CreateBy = owner.ToString()
    };

    private static ExpectedTransaction Period(RecurringTransactionTemplate template, DateTime date, decimal amount) => new()
    {
        Id = Guid.NewGuid(), UserId = template.UserId, AccountId = template.AccountId,
        RecurringTransactionTemplateId = template.Id, ExpectedDate = date, ExpectedAmount = amount,
        Description = "Historical period description", Category = "Historical category", Notes = "Historical notes",
        TransactionType = RecurringTransactionType.Expense, Status = ExpectedTransactionStatus.Pending,
        GeneratedAt = new DateTime(2026, 1, 1), CreatedAt = new DateTime(2026, 1, 1), UpdatedAt = new DateTime(2026, 1, 1),
        CreateBy = template.UserId!.Value.ToString()
    };

    private static Task SeedAsync(Thm3PostgresFixture fixture, params RecurringTransactionTemplate[] templates) =>
        fixture.WithOwnerAsync(templates[0].UserId!.Value, async services =>
        {
            // Persist historical state directly, without accidentally invoking create-time generation.
            var database = services.GetRequiredService<CoreFinanceDbContext>();
            database.RecurringTransactionTemplates.AddRange(templates);
            await database.SaveChangesAsync();
        });

    private static Task SeedPeriodsAsync(Thm3PostgresFixture fixture, params ExpectedTransaction[] periods) =>
        fixture.WithOwnerAsync(periods[0].UserId!.Value, async services =>
        {
            var database = services.GetRequiredService<CoreFinanceDbContext>();
            database.ExpectedTransactions.AddRange(periods);
            await database.SaveChangesAsync();
        });

    private static Task<List<ExpectedTransaction>> PeriodsAsync(Thm3PostgresFixture fixture, Guid templateId) =>
        fixture.ReadAsync(database => database.ExpectedTransactions.IgnoreQueryFilters().AsNoTracking()
            .Where(row => row.RecurringTransactionTemplateId == templateId).OrderBy(row => row.ExpectedDate).ThenBy(row => row.Id)
            .ToListAsync());

    private static Task<RecurringTransactionTemplate> TemplateAsync(Thm3PostgresFixture fixture, Guid id) =>
        fixture.ReadAsync(database => database.RecurringTransactionTemplates.IgnoreQueryFilters().AsNoTracking().SingleAsync(row => row.Id == id));

    private static Task<int> ActualCountAsync(Thm3PostgresFixture fixture) =>
        fixture.ReadAsync(database => database.Transactions.IgnoreQueryFilters().CountAsync());

    private static async Task<Guid> CreateTemplateAsync(BackendFinanceClient api, Guid account, DateTime start,
        RecurrenceFrequency frequency, int days, DateTime? end = null, decimal amount = 100m)
    {
        var fields = new Dictionary<string, string>
        {
            ["AccountId"] = account.ToString(), ["Name"] = "API generated template", ["Description"] = "Original description",
            ["Amount"] = amount.ToString(CultureInfo.InvariantCulture), ["TransactionType"] = "Expense",
            ["Frequency"] = frequency.ToString(), ["StartDate"] = Date(start), ["NextExecutionDate"] = Date(start),
            ["DaysInAdvance"] = days.ToString(CultureInfo.InvariantCulture), ["IsActive"] = "true", ["AutoGenerate"] = "true"
        };
        if (end.HasValue) fields["EndDate"] = Date(end.Value);
        using var response = await api.PostFormAsync(TemplateRoute, fields);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await BackendFinanceClient.IdOf(response);
    }

    private static Task UpdateTemplateAsync(BackendFinanceClient api, Guid id, Dictionary<string, string> fields)
    {
        fields["Id"] = id.ToString();
        return OkAsync(api.PutFormAsync($"{TemplateRoute}/{id}", fields));
    }

    private static async Task OkAsync(Task<HttpResponseMessage> request)
    {
        using var response = await request;
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    private static string Date(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static async Task<Dictionary<Guid, string>> SnapshotsAsync(Thm3PostgresFixture fixture, Guid[] ids)
    {
        await using var connection = await fixture.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id, to_jsonb(period)::text || ':' || period.xmin::text AS snapshot " +
            "FROM expected_transactions AS period WHERE id = ANY(@ids)", connection);
        command.Parameters.AddWithValue("ids", ids);
        await using var reader = await command.ExecuteReaderAsync();
        Dictionary<Guid, string> snapshots = [];
        while (await reader.ReadAsync()) snapshots.Add(reader.GetGuid(0), reader.GetString(1));
        snapshots.Should().HaveCount(ids.Length);
        return snapshots;
    }

    private static async Task AssertSnapshotsAsync(Thm3PostgresFixture fixture, Dictionary<Guid, string> before)
    {
        var after = await SnapshotsAsync(fixture, before.Keys.ToArray());
        after.Should().BeEquivalentTo(before, "generation must not rewrite dates, amounts, metadata, statuses or tombstones");
    }

    private static async Task<string> TemplateSnapshotAsync(Thm3PostgresFixture fixture, Guid id)
    {
        await using var connection = await fixture.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT to_jsonb(template)::text || ':' || template.xmin::text FROM recurring_transaction_templates AS template WHERE id = @id",
            connection);
        command.Parameters.AddWithValue("id", id);
        return (string)(await command.ExecuteScalarAsync())!;
    }

    private static async Task<long> XminAsync(Thm3PostgresFixture fixture, Guid id)
    {
        await using var connection = await fixture.OpenConnectionAsync();
        await using var command = new NpgsqlCommand("SELECT xmin::text::bigint FROM expected_transactions WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", id);
        return (long)(await command.ExecuteScalarAsync())!;
    }

    private static Task<int> InsertDuplicateDayAsync(Thm3PostgresFixture fixture, Guid existingId, Guid duplicateId) =>
        fixture.ExecuteSqlAsync(
            "INSERT INTO expected_transactions (id, user_id, recurring_transaction_template_id, account_id, " +
            "expected_date, expected_amount, transaction_type, status, is_adjusted, generated_at, created_at, updated_at) " +
            "SELECT @duplicate, user_id, recurring_transaction_template_id, account_id, " +
            "expected_date::date + interval '18 hours 23 minutes', expected_amount, transaction_type, @status, false, " +
            "generated_at, created_at, updated_at FROM expected_transactions WHERE id = @existing",
            new NpgsqlParameter("duplicate", duplicateId), new NpgsqlParameter("existing", existingId),
            new NpgsqlParameter<int>("status", (int)ExpectedTransactionStatus.Pending));

    private sealed class HeldTemplateLock(Thm3PostgresFixture fixture, NpgsqlConnection connection, string key) : IAsyncDisposable
    {
        private bool _released;

        public static async Task<HeldTemplateLock> AcquireAsync(Thm3PostgresFixture fixture, Guid owner, Guid templateId)
        {
            var connection = await fixture.OpenConnectionAsync();
            var key = $"recurring:{owner:N}:{templateId:N}";
            try
            {
                await using var command = new NpgsqlCommand("SELECT pg_advisory_lock(hashtextextended(@key, 0))", connection);
                command.Parameters.AddWithValue("key", key);
                await command.ExecuteNonQueryAsync();
                return new HeldTemplateLock(fixture, connection, key);
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }
        }

        public async Task WaitForWaitersAsync(int expected, Task contender)
        {
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await using var observer = await fixture.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(
                "SELECT count(*) FROM pg_stat_activity WHERE datname = current_database() " +
                "AND wait_event_type = 'Lock' AND @holder = ANY(pg_blocking_pids(pid))", observer);
            command.Parameters.AddWithValue("holder", connection.ProcessID);
            while (true)
            {
                var waiting = (long)(await command.ExecuteScalarAsync(deadline.Token))!;
                if (waiting >= expected) return;
                if (contender.IsCompleted)
                {
                    await contender;
                    throw new InvalidOperationException("The contender completed before reaching the held PostgreSQL lock.");
                }
                // Polling is not evidence of success; only the observed server-side lock wait releases the barrier.
                await Task.Delay(TimeSpan.FromMilliseconds(20), deadline.Token);
            }
        }

        public async Task ReleaseAsync()
        {
            if (_released) return;
            await using var command = new NpgsqlCommand("SELECT pg_advisory_unlock(hashtextextended(@key, 0))", connection);
            command.Parameters.AddWithValue("key", key);
            ((bool)(await command.ExecuteScalarAsync())!).Should().BeTrue();
            _released = true;
        }

        public async ValueTask DisposeAsync()
        {
            try { await ReleaseAsync(); }
            finally { await connection.DisposeAsync(); }
        }
    }

    private sealed class CommitNotifications(NpgsqlConnection connection) : IAsyncDisposable
    {
        private readonly HashSet<Guid> _received = [];

        public static async Task<CommitNotifications> CreateAsync(Thm3PostgresFixture fixture, Guid[] templateIds)
        {
            var connection = await fixture.OpenConnectionAsync();
            try
            {
                var channel = $"thm3_{Guid.NewGuid():N}";
                await using (var listen = new NpgsqlCommand($"LISTEN {channel}", connection))
                    await listen.ExecuteNonQueryAsync();
                var ids = string.Join(",", templateIds.Select(id => $"'{id:D}'::uuid"));
                await fixture.ExecuteSqlAsync($"""
                    CREATE FUNCTION thm3_generation_commit() RETURNS trigger LANGUAGE plpgsql AS $body$
                    BEGIN
                        IF NEW.recurring_transaction_template_id = ANY(ARRAY[{ids}]) THEN
                            PERFORM pg_notify('{channel}', NEW.recurring_transaction_template_id::text);
                        END IF;
                        RETURN NEW;
                    END;
                    $body$;
                    CREATE TRIGGER thm3_generation_commit AFTER INSERT ON expected_transactions
                    FOR EACH ROW EXECUTE FUNCTION thm3_generation_commit();
                    """);
                var notifications = new CommitNotifications(connection);
                connection.Notification += (_, notification) => notifications._received.Add(Guid.Parse(notification.Payload));
                return notifications;
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }
        }

        public async Task WaitForAsync(Guid[] templateIds)
        {
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(90));
            while (templateIds.Any(id => !_received.Contains(id)))
                await connection.WaitAsync(deadline.Token);
        }

        public ValueTask DisposeAsync() => connection.DisposeAsync();
    }
}
