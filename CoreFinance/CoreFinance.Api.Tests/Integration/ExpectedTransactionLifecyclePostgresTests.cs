using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Tests.Shared.Helpers;

namespace CoreFinance.Api.Tests.Integration;

[Trait("Category", "Characterization")]
public sealed class ExpectedTransactionLifecyclePostgresTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Theory]
    [InlineData(RecurringTransactionType.Income, false)]
    [InlineData(RecurringTransactionType.Expense, false)]
    [InlineData(RecurringTransactionType.Income, true)]
    [InlineData(RecurringTransactionType.Expense, true)]
    public async Task Payment_Persists_Owned_Actual_Without_Rebinding_Null_References_Or_Changing_Balance(
        RecurringTransactionType type, bool fallback)
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("expected-owned-payment");
        var accountId = await api.CreateAccountAsync("Payment account");
        var templateId = fallback ? (Guid?)null : await CreateTemplateAsync(api, accountId, fixture.Today, "Owned schedule");
        var date = fixture.Today.AddDays(-2).AddHours(21).AddMinutes(15);
        var id = await api.CreateExpectedAsync(date, 125.75m, type,
            fallback ? null : accountId, templateId, "Invoice payment", "Utilities");
        var balanceBefore = await ReadBalanceAsync(fixture, accountId);
        if (fallback) await AssertSqlNullReferencesAsync(fixture, id);

        var json = await JsonAsync(api.RecordPaymentAsync(id, fallback ? accountId : null));
        json.GetProperty("success").GetBoolean().Should().BeTrue();
        json.GetProperty("expectedTransactionId").GetGuid().Should().Be(id);
        var actualId = json.GetProperty("actualTransactionId").GetGuid();
        var expected = await ReadExpectedAsync(fixture, id);
        expected.UserId.Should().Be(api.UserId);
        expected.Status.Should().Be(ExpectedTransactionStatus.Confirmed);
        expected.ActualTransactionId.Should().Be(actualId);
        expected.ProcessedAt.Should().NotBeNull();
        expected.ExpectedDate.Should().Be(date);
        expected.ExpectedAmount.Should().Be(125.75m);
        expected.AccountId.Should().Be(fallback ? null : accountId);
        expected.RecurringTransactionTemplateId.Should().Be(templateId);
        if (fallback) await AssertSqlNullReferencesAsync(fixture, id);

        var actual = await BackendFinanceClient.ReadTransactionRowAsync(actualId, fixture.ConnectionString);
        actual.Should().NotBeNull("the payment must commit an independently readable transaction");
        actual!["user_id"].Should().Be(api.UserId);
        actual["account_id"].Should().Be(accountId);
        actual["transaction_date"].Should().Be(date);
        actual["revenue_amount"].Should().Be(type == RecurringTransactionType.Income ? 125.75m : 0m);
        actual["spent_amount"].Should().Be(type == RecurringTransactionType.Expense ? 125.75m : 0m);
        actual["category_type"].Should().Be((int)(type == RecurringTransactionType.Income ? CategoryType.Income : CategoryType.Expense));
        actual["category_summary"].Should().Be("Utilities");
        actual["description"].Should().Be("Invoice payment");
        actual["balance"].Should().Be(0m);
        actual["is_deleted"].Should().BeNull();
        (await ReadBalanceAsync(fixture, accountId)).Should().Be(balanceBefore);
        (await CountActualsAsync(fixture)).Should().Be(1);
        var detail = DeserializeExpected(await JsonAsync(api.GetAsync($"/api/ExpectedTransaction/{id}")));
        detail.Status.Should().Be(ExpectedTransactionStatus.Confirmed);
        detail.HasActualTransaction.Should().BeTrue();
        detail.ActualTransactionId.Should().Be(actualId);
    }

    /// <summary>
    ///     (EN) Rejects a malformed payment account with the local error envelope and no durable changes.<br />
    ///     (VI) Từ chối tài khoản thanh toán sai định dạng bằng cấu trúc lỗi cục bộ, không thay đổi dữ liệu bền vững.
    /// </summary>
    [Fact]
    public async Task Malformed_Payment_Account_Returns_Local_Invalid_Request_Without_Durable_Changes()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("expected-malformed-payment-account");
        var accountId = await api.CreateAccountAsync("Malformed payment account");
        var id = await api.CreateExpectedAsync(fixture.Today, 125.75m, accountId: accountId);
        var before = await ReadExpectedAsync(fixture, id);
        before.UserId.Should().Be(api.UserId);
        before.Status.Should().Be(ExpectedTransactionStatus.Pending);

        using var response = await api.PostJsonAsync($"/api/ExpectedTransaction/{id}/record-payment",
            new { accountId = "not-a-guid" });
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, content);
        using var json = JsonDocument.Parse(content);
        json.RootElement.EnumerateObject().Select(property => property.Name).Should()
            .BeEquivalentTo(new[] { "code", "message" }, "model binding must retain the local envelope rather than ProblemDetails");
        json.RootElement.GetProperty("code").GetString().Should().Be("invalid_request");
        json.RootElement.GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();
        (await ReadExpectedAsync(fixture, id)).Should().BeEquivalentTo(before);
        (await CountActualsAsync(fixture)).Should().Be(0);
    }

    [Fact]
    public async Task Foreign_Owner_Commands_And_Fallback_Account_Choices_Leave_Durable_Data_Untouched()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var owner = await fixture.LoginAsync("expected-owner");
        using var foreign = await fixture.LoginAsync("expected-foreign");
        var ownAccount = await owner.CreateAccountAsync("Owner account");
        var otherOwnAccount = await owner.CreateAccountAsync("Another owner account");
        var foreignAccount = await foreign.CreateAccountAsync("Foreign account");
        var boundId = await owner.CreateExpectedAsync(fixture.Today, 400m, accountId: ownAccount, description: "Owned period");
        var unboundId = await owner.CreateExpectedAsync(fixture.Today.AddDays(1), 500m, description: "Unbound period");
        var foreignActual = await CreateManualActualAsync(foreign, foreignAccount, fixture.Today, 400m);
        var boundBefore = await ReadExpectedAsync(fixture, boundId);
        var unboundBefore = await ReadExpectedAsync(fixture, unboundId);
        var balances = await fixture.ReadAsync(db => db.Accounts.AsNoTracking().OrderBy(a => a.Id)
            .Select(a => new { a.Id, a.CurrentBalance }).ToListAsync());

        await ExpectAsync(foreign.GetAsync($"/api/ExpectedTransaction/{boundId}"), HttpStatusCode.NotFound);
        await ExpectAsync(foreign.RecordPaymentAsync(boundId), HttpStatusCode.NotFound);
        await ExpectAsync(foreign.CancelExpectedAsync(boundId, "Not my bill"), HttpStatusCode.NotFound);
        await ExpectAsync(foreign.AdjustExpectedAsync(boundId, 900m, "Not my bill"), HttpStatusCode.NotFound);
        await ExpectAsync(foreign.DeleteAsync($"/api/ExpectedTransaction/{boundId}"), HttpStatusCode.NotFound);
        await ExpectAsync(foreign.PutExpectedAsync(boundId, new Dictionary<string, string> { ["Description"] = "Foreign edit" }), HttpStatusCode.NotFound);
        await ExpectAsync(owner.ConfirmExpectedAsync(boundId, foreignActual), HttpStatusCode.NotFound);
        await ExpectAsync(owner.RecordPaymentAsync(boundId, otherOwnAccount), HttpStatusCode.BadRequest);
        await ExpectAsync(owner.RecordPaymentAsync(unboundId), HttpStatusCode.BadRequest);
        await ExpectAsync(owner.RecordPaymentAsync(unboundId, Guid.Empty), HttpStatusCode.BadRequest);
        await ExpectAsync(owner.RecordPaymentAsync(unboundId, foreignAccount), HttpStatusCode.NotFound);
        await ExpectAsync(owner.RecordPaymentAsync(unboundId, Guid.NewGuid()), HttpStatusCode.NotFound);

        (await ReadExpectedAsync(fixture, boundId)).Should().BeEquivalentTo(boundBefore);
        (await ReadExpectedAsync(fixture, unboundId)).Should().BeEquivalentTo(unboundBefore);
        await AssertSqlNullReferencesAsync(fixture, unboundId);
        (await CountActualsAsync(fixture)).Should().Be(1, "only the pre-existing foreign actual may remain");
        (await fixture.ReadAsync(db => db.Accounts.AsNoTracking().OrderBy(a => a.Id)
            .Select(a => new { a.Id, a.CurrentBalance }).ToListAsync())).Should().BeEquivalentTo(balances);
        var foreignList = await JsonAsync(foreign.PostJsonAsync("/api/ExpectedTransaction/filter", PageRequest()));
        foreignList.GetProperty("pagination").GetProperty("totalRow").GetInt32().Should().Be(0);
        foreignList.GetProperty("data").GetArrayLength().Should().Be(0);
        (await JsonAsync(owner.GetAsync($"/api/ExpectedTransaction/pending/{foreign.UserId}"))).GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Transfer_Payment_Is_Rejected_Without_An_Actual_Or_An_Account_Balance_Change()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("expected-transfer");
        var accountId = await api.CreateAccountAsync("Transfer account");
        var id = await api.CreateExpectedAsync(fixture.Today, 750m, RecurringTransactionType.Transfer, accountId);
        var before = await ReadExpectedAsync(fixture, id);
        var balance = await ReadBalanceAsync(fixture, accountId);

        await ExpectAsync(api.RecordPaymentAsync(id), HttpStatusCode.BadRequest);

        (await ReadExpectedAsync(fixture, id)).Should().BeEquivalentTo(before);
        (await CountActualsAsync(fixture)).Should().Be(0);
        (await ReadBalanceAsync(fixture, accountId)).Should().Be(balance);
    }

    [Fact]
    public async Task Fifty_Payment_Requests_That_Load_The_Same_Pending_Row_Commit_Exactly_One_Actual()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        var token = await fixture.LoginTokenAsync("expected-fifty-payments");
        var clients = Enumerable.Range(0, 50)
            .Select(_ => BackendFinanceClient.Authenticate(fixture.Factory.CreateClient(), token)).ToArray();
        try
        {
            var accountId = await clients[0].CreateAccountAsync("Concurrent payment account");
            var id = await clients[0].CreateExpectedAsync(fixture.Today.AddHours(18), 215.25m, accountId: accountId);
            var balanceBefore = await ReadBalanceAsync(fixture, accountId);
            await using var barrier = await RowWriteBarrier.HoldAsync(fixture, id);
            var requests = clients.Select(client => client.RecordPaymentAsync(id)).ToArray();
            var responses = await ReleaseAndCollectAsync(fixture, barrier, requests, 50);
            try
            {
                responses.Count(r => r.StatusCode == HttpStatusCode.OK).Should().Be(1);
                responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(49);
                var winner = responses.Single(r => r.StatusCode == HttpStatusCode.OK);
                using var json = JsonDocument.Parse(await winner.Content.ReadAsStringAsync());
                var actualId = json.RootElement.GetProperty("actualTransactionId").GetGuid();
                var expected = await ReadExpectedAsync(fixture, id);
                expected.Status.Should().Be(ExpectedTransactionStatus.Confirmed);
                expected.ActualTransactionId.Should().Be(actualId);
                expected.IsDeleted.Should().BeNull();
                (await CountActualsAsync(fixture)).Should().Be(1, "every losing insert must be rolled back");
                var actual = await fixture.ReadAsync(db => db.Transactions.IgnoreQueryFilters().AsNoTracking().SingleAsync());
                actual.Id.Should().Be(actualId);
                actual.UserId.Should().Be(clients[0].UserId);
                actual.SpentAmount.Should().Be(215.25m);
                actual.TransactionDate.Should().Be(expected.ExpectedDate);
                (await ReadBalanceAsync(fixture, accountId)).Should().Be(balanceBefore);
            }
            finally
            {
                foreach (var response in responses) response.Dispose();
            }
        }
        finally
        {
            foreach (var client in clients) client.Dispose();
        }
    }

    [Fact]
    public async Task Concurrent_Confirmation_Reserves_One_Actual_Link_Including_A_Legacy_Tombstone()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        var token = await fixture.LoginTokenAsync("expected-link-race");
        using var firstApi = BackendFinanceClient.Authenticate(fixture.Factory.CreateClient(), token);
        using var secondApi = BackendFinanceClient.Authenticate(fixture.Factory.CreateClient(), token);
        var accountId = await firstApi.CreateAccountAsync("Link account");
        var actualId = await CreateManualActualAsync(firstApi, accountId, fixture.Today, 100m);
        var firstId = await firstApi.CreateExpectedAsync(fixture.Today, 100m, accountId: accountId);
        var secondId = await firstApi.CreateExpectedAsync(fixture.Today.AddDays(1), 100m, accountId: accountId);
        await using var barrier = await RowWriteBarrier.HoldAsync(fixture, firstId, secondId);
        var requests = new[] { firstApi.ConfirmExpectedAsync(firstId, actualId), secondApi.ConfirmExpectedAsync(secondId, actualId) };
        var responses = await ReleaseAndCollectAsync(fixture, barrier, requests, 2);
        try
        {
            responses.Select(r => r.StatusCode).Should().BeEquivalentTo(new[] { HttpStatusCode.OK, HttpStatusCode.Conflict });
        }
        finally
        {
            foreach (var response in responses) response.Dispose();
        }

        var rows = await fixture.ReadAsync(db => db.ExpectedTransactions.IgnoreQueryFilters().AsNoTracking().ToListAsync());
        rows.Count(e => e.ActualTransactionId == actualId).Should().Be(1);
        rows.Count(e => e.Status == ExpectedTransactionStatus.Confirmed).Should().Be(1);
        var winnerId = rows.Single(e => e.ActualTransactionId == actualId).Id;
        var loserId = rows.Single(e => e.ActualTransactionId == null).Id;
        await fixture.ExecuteSqlAsync("UPDATE expected_transactions SET is_deleted = 'fixture-tombstone' WHERE id = @id",
            new NpgsqlParameter("id", winnerId));
        await ExpectAsync(firstApi.ConfirmExpectedAsync(loserId, actualId), HttpStatusCode.Conflict);
        Func<Task> duplicateLink = async () =>
        {
            await fixture.ExecuteSqlAsync("UPDATE expected_transactions SET actual_transaction_id = @actual WHERE id = @id",
                new NpgsqlParameter("actual", actualId), new NpgsqlParameter("id", loserId));
        };
        var failure = await duplicateLink.Should().ThrowAsync<PostgresException>();
        failure.Which.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
        failure.Which.ConstraintName.Should().Be("ux_expected_actual_transaction");
        (await ReadExpectedAsync(fixture, winnerId)).IsDeleted.Should().NotBeNull();
        (await ReadExpectedAsync(fixture, loserId)).ActualTransactionId.Should().BeNull();
        (await CountActualsAsync(fixture)).Should().Be(1);
    }

    [Theory]
    [InlineData("cancel", true)]
    [InlineData("cancel", false)]
    [InlineData("adjust", true)]
    [InlineData("adjust", false)]
    [InlineData("delete", true)]
    [InlineData("delete", false)]
    [InlineData("put-date", true)]
    [InlineData("put-date", false)]
    [InlineData("same-amount-adjust", true)]
    [InlineData("same-amount-adjust", false)]
    public async Task Payment_And_Pending_Mutation_Respect_Both_Commit_Orders_Without_Orphans_Or_Stale_State(
        string mutation, bool paymentFirst)
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        var token = await fixture.LoginTokenAsync("expected-ordered-race");
        using var paymentApi = BackendFinanceClient.Authenticate(fixture.Factory.CreateClient(), token);
        using var mutationApi = BackendFinanceClient.Authenticate(fixture.Factory.CreateClient(), token);
        var accountId = await paymentApi.CreateAccountAsync("Ordered race account");
        var initialDate = fixture.Today.AddDays(3).AddHours(19).AddMinutes(30);
        var editedDate = initialDate.AddDays(4).AddHours(1);
        var id = await paymentApi.CreateExpectedAsync(initialDate, 100m, accountId: accountId,
            description: "Original invoice", category: "Original category");
        await ExpectAsync(paymentApi.AdjustExpectedAsync(id, 120m, "Previous revision"), HttpStatusCode.OK);
        var before = await ReadExpectedAsync(fixture, id);
        var versionBefore = await ReadVersionAsync(fixture, id);
        var balanceBefore = await ReadBalanceAsync(fixture, accountId);
        await using var barrier = await RowWriteBarrier.HoldAsync(fixture, id);
        Task<HttpResponseMessage>? first = null;
        Task<HttpResponseMessage>? second = null;
        HttpResponseMessage[] responses;
        try
        {
            first = paymentFirst ? paymentApi.RecordPaymentAsync(id) : MutateAsync(mutationApi, id, mutation, editedDate);
            var firstWaiters = await WaitForWaitersAsync(fixture,
                waiters => waiters.Count == 1 && waiters[0].Blockers.Contains(barrier.ProcessId), [first]);
            var firstPid = firstWaiters.Single().ProcessId;
            second = paymentFirst ? MutateAsync(mutationApi, id, mutation, editedDate) : paymentApi.RecordPaymentAsync(id);
            // The second writer is behind the first writer's tuple lock, not merely scheduled later by a delay.
            await WaitForWaitersAsync(fixture,
                waiters => waiters.Count == 2 && waiters.Any(w => w.ProcessId != firstPid && w.Blockers.Contains(firstPid)),
                [first, second]);
            await barrier.ReleaseAsync();
            responses = await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(30));
        }
        catch
        {
            await barrier.ReleaseAsync();
            await DrainAsync(first, second);
            throw;
        }
        try
        {
            responses[0].StatusCode.Should().Be(HttpStatusCode.OK, "the observed first lock waiter owns the first commit");
            responses[1].StatusCode.Should().Be(HttpStatusCode.Conflict, "both writers loaded the old Pending xmin before the first commit");
        }
        finally
        {
            foreach (var response in responses) response.Dispose();
        }

        var after = await ReadExpectedAsync(fixture, id);
        (await ReadVersionAsync(fixture, id)).Should().NotBe(versionBefore);
        after.OriginalAmount.Should().Be(100m, "a later adjustment must not replace the first original amount");
        (await ReadBalanceAsync(fixture, accountId)).Should().Be(balanceBefore);
        if (paymentFirst)
        {
            after.Status.Should().Be(ExpectedTransactionStatus.Confirmed);
            after.ActualTransactionId.Should().NotBeNull();
            after.IsDeleted.Should().BeNull("a losing DELETE may not turn a committed payment into a tombstone");
            after.ExpectedAmount.Should().Be(before.ExpectedAmount);
            after.ExpectedDate.Should().Be(before.ExpectedDate);
            after.Description.Should().Be(before.Description);
            after.Category.Should().Be(before.Category);
            after.Notes.Should().Be(before.Notes);
            after.AdjustmentReason.Should().Be(before.AdjustmentReason);
            await AssertSingleActualMatchesAsync(fixture, after, accountId);
            return;
        }

        after.ActualTransactionId.Should().BeNull();
        (await CountActualsAsync(fixture)).Should().Be(0, "the stale payment's insert must roll back together with its lost confirmation");
        if (mutation is "cancel" or "delete")
        {
            after.ExpectedAmount.Should().Be(before.ExpectedAmount);
            after.ExpectedDate.Should().Be(before.ExpectedDate);
            after.Description.Should().Be(before.Description);
            after.Category.Should().Be(before.Category);
            after.Notes.Should().Be(before.Notes);
        }
        if (mutation == "cancel")
        {
            after.Status.Should().Be(ExpectedTransactionStatus.Cancelled);
            after.AdjustmentReason.Should().Be("Concurrent cancellation");
            after.ProcessedAt.Should().NotBeNull();
            after.IsDeleted.Should().BeNull();
        }
        else if (mutation == "delete")
        {
            after.Status.Should().Be(ExpectedTransactionStatus.Pending);
            after.IsDeleted.Should().NotBeNull("a stale payment must not resurrect the winning tombstone");
            after.AdjustmentReason.Should().Be(before.AdjustmentReason);
            await ExpectAsync(paymentApi.GetAsync($"/api/ExpectedTransaction/{id}"), HttpStatusCode.NotFound);
            await ExpectAsync(paymentApi.RecordPaymentAsync(id), HttpStatusCode.NotFound);
            (await ReadExpectedAsync(fixture, id)).Should().BeEquivalentTo(after);
        }
        else
        {
            after.Status.Should().Be(ExpectedTransactionStatus.Pending);
            after.IsDeleted.Should().BeNull();
            after.ExpectedAmount.Should().Be(mutation == "adjust" ? 160m : 120m);
            after.ExpectedDate.Should().Be(mutation == "put-date" ? editedDate : initialDate);
            after.AdjustmentReason.Should().Be(mutation == "put-date" ? "Previous revision" : "Concurrent revision");
            after.Description.Should().Be(mutation == "put-date" ? "Revised invoice" : "Original invoice");
            after.Category.Should().Be(mutation == "put-date" ? "Revised category" : "Original category");
            after.Notes.Should().Be(mutation == "put-date" ? "Revised notes" : before.Notes);
            await ExpectAsync(paymentApi.RecordPaymentAsync(id), HttpStatusCode.OK);
            var confirmed = await ReadExpectedAsync(fixture, id);
            confirmed.Status.Should().Be(ExpectedTransactionStatus.Confirmed);
            confirmed.AdjustmentReason.Should().Be(after.AdjustmentReason);
            confirmed.OriginalAmount.Should().Be(100m);
            await AssertSingleActualMatchesAsync(fixture, confirmed, accountId);
        }
        (await ReadBalanceAsync(fixture, accountId)).Should().Be(balanceBefore);
    }

    [Fact]
    public async Task Fault_After_Actual_Insert_Rolls_Back_Both_Writes_And_Does_Not_Affect_Other_Periods()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("expected-payment-fault");
        var accountId = await api.CreateAccountAsync("Fault account");
        var id = await api.CreateExpectedAsync(fixture.Today, 300m, accountId: accountId);
        var before = await ReadExpectedAsync(fixture, id);
        var balanceBefore = await ReadBalanceAsync(fixture, accountId);
        await fixture.ExecuteSqlAsync($"""
            CREATE FUNCTION thm3_reject_payment_update() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
                IF NEW.id = '{id:D}'::uuid AND NEW.actual_transaction_id IS NOT NULL
                   AND EXISTS (SELECT 1 FROM transactions WHERE id = NEW.actual_transaction_id) THEN
                    RAISE EXCEPTION 'Fixture-owned failure after actual insert' USING ERRCODE = '23514';
                END IF;
                RETURN NEW;
            END;
            $$;
            CREATE TRIGGER thm3_reject_payment_update BEFORE UPDATE ON expected_transactions
                FOR EACH ROW EXECUTE FUNCTION thm3_reject_payment_update();
            """);

        await ExpectAsync(api.RecordPaymentAsync(id), HttpStatusCode.InternalServerError);
        await using (var connection = await fixture.OpenConnectionAsync())
        {
            await using var command = new NpgsqlCommand("""
                SELECT status, actual_transaction_id, processed_at,
                       (SELECT count(*) FROM transactions)
                FROM expected_transactions WHERE id = @id
                """, connection);
            command.Parameters.AddWithValue("id", id);
            await using var reader = await command.ExecuteReaderAsync();
            (await reader.ReadAsync()).Should().BeTrue();
            reader.GetInt32(0).Should().Be((int)ExpectedTransactionStatus.Pending);
            reader.IsDBNull(1).Should().BeTrue();
            reader.IsDBNull(2).Should().BeTrue();
            reader.GetInt64(3).Should().Be(0, "an independent connection must see neither confirmation nor orphan insert");
        }
        (await ReadExpectedAsync(fixture, id)).Should().BeEquivalentTo(before);
        (await ReadBalanceAsync(fixture, accountId)).Should().Be(balanceBefore);

        var healthyId = await api.CreateExpectedAsync(fixture.Today.AddDays(1), 450m, accountId: accountId);
        await ExpectAsync(api.RecordPaymentAsync(healthyId), HttpStatusCode.OK);
        (await ReadExpectedAsync(fixture, healthyId)).Status.Should().Be(ExpectedTransactionStatus.Confirmed);
        (await CountActualsAsync(fixture)).Should().Be(1, "the trigger is scoped to only the failed fixture period");
        await fixture.ExecuteSqlAsync("DROP TRIGGER thm3_reject_payment_update ON expected_transactions; DROP FUNCTION thm3_reject_payment_update();");
        await ExpectAsync(api.RecordPaymentAsync(id), HttpStatusCode.OK);
        (await ReadExpectedAsync(fixture, id)).Status.Should().Be(ExpectedTransactionStatus.Confirmed);
        (await CountActualsAsync(fixture)).Should().Be(2, "retry after rollback creates exactly one actual for the failed period");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Nonpositive_Amounts_Cannot_Be_Created_Adjusted_Edited_Or_Paid_Even_For_Legacy_Rows(int invalidAmount)
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("expected-positive-amount");
        var accountId = await api.CreateAccountAsync("Amount account");
        await ExpectAsync(api.PostFormAsync("/api/ExpectedTransaction", ExpectedForm(fixture.Today, invalidAmount, accountId)), HttpStatusCode.BadRequest);
        (await fixture.ReadAsync(db => db.ExpectedTransactions.IgnoreQueryFilters().CountAsync())).Should().Be(0);
        var id = await api.CreateExpectedAsync(fixture.Today, 100m, accountId: accountId);
        var before = await ReadExpectedAsync(fixture, id);
        await ExpectAsync(api.AdjustExpectedAsync(id, invalidAmount, "Invalid amount"), HttpStatusCode.BadRequest);
        await ExpectAsync(api.PutExpectedAsync(id, new Dictionary<string, string>
        {
            ["ExpectedAmount"] = invalidAmount.ToString(CultureInfo.InvariantCulture), ["AdjustmentReason"] = "Invalid amount"
        }), HttpStatusCode.BadRequest);
        (await ReadExpectedAsync(fixture, id)).Should().BeEquivalentTo(before);
        await fixture.ExecuteSqlAsync("UPDATE expected_transactions SET expected_amount = @amount WHERE id = @id",
            new NpgsqlParameter("amount", (decimal)invalidAmount), new NpgsqlParameter("id", id));
        var legacyBefore = await ReadExpectedAsync(fixture, id);

        await ExpectAsync(api.RecordPaymentAsync(id), HttpStatusCode.BadRequest);

        (await ReadExpectedAsync(fixture, id)).Should().BeEquivalentTo(legacyBefore);
        (await CountActualsAsync(fixture)).Should().Be(0);
    }

    [Fact]
    public async Task Adjustment_Reasons_Are_Required_And_Two_Adjustments_Preserve_The_First_Original_Amount()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("expected-adjustment-reasons");
        var accountId = await api.CreateAccountAsync("Adjustment account");
        var id = await api.CreateExpectedAsync(fixture.Today, 100m, accountId: accountId);
        var before = await ReadExpectedAsync(fixture, id);
        foreach (var reason in new[] { "", " \t ", new string('x', 501) })
        {
            await ExpectAsync(api.AdjustExpectedAsync(id, 120m, reason), HttpStatusCode.BadRequest);
            await ExpectAsync(api.CancelExpectedAsync(id, reason), HttpStatusCode.BadRequest);
        }
        await ExpectAsync(api.PutExpectedAsync(id, new Dictionary<string, string> { ["ExpectedAmount"] = "120" }), HttpStatusCode.BadRequest);
        (await ReadExpectedAsync(fixture, id)).Should().BeEquivalentTo(before);

        var firstReason = new string('a', 500);
        await ExpectAsync(api.AdjustExpectedAsync(id, 120m, firstReason), HttpStatusCode.OK);
        var first = await ReadExpectedAsync(fixture, id);
        first.ExpectedAmount.Should().Be(120m);
        first.IsAdjusted.Should().BeTrue();
        first.OriginalAmount.Should().Be(100m);
        first.AdjustmentReason.Should().Be(firstReason);
        await ExpectAsync(api.AdjustExpectedAsync(id, 140.50m, "Second revision"), HttpStatusCode.OK);
        var second = await ReadExpectedAsync(fixture, id);
        second.ExpectedAmount.Should().Be(140.50m);
        second.IsAdjusted.Should().BeTrue();
        second.OriginalAmount.Should().Be(100m);
        second.AdjustmentReason.Should().Be("Second revision");
        await ExpectAsync(api.RecordPaymentAsync(id), HttpStatusCode.OK);
        var confirmed = await ReadExpectedAsync(fixture, id);
        confirmed.OriginalAmount.Should().Be(100m);
        confirmed.AdjustmentReason.Should().Be("Second revision");
        await AssertSingleActualMatchesAsync(fixture, confirmed, accountId);

        var cancellationId = await api.CreateExpectedAsync(fixture.Today.AddDays(1), 50m, accountId: accountId);
        var cancellationReason = new string('c', 500);
        await ExpectAsync(api.CancelExpectedAsync(cancellationId, cancellationReason), HttpStatusCode.OK);
        var cancelled = await ReadExpectedAsync(fixture, cancellationId);
        cancelled.Status.Should().Be(ExpectedTransactionStatus.Cancelled);
        cancelled.AdjustmentReason.Should().Be(cancellationReason);
        cancelled.ProcessedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(ExpectedTransactionStatus.Confirmed)]
    [InlineData(ExpectedTransactionStatus.Cancelled)]
    [InlineData(ExpectedTransactionStatus.Completed)]
    public async Task Terminal_Periods_Reject_Put_Delete_And_Lifecycle_Commands_Without_Changing_Stored_State(
        ExpectedTransactionStatus status)
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("expected-terminal");
        var accountId = await api.CreateAccountAsync("Terminal account");
        var id = await api.CreateExpectedAsync(fixture.Today, 80m, accountId: accountId, description: "Terminal invoice");
        if (status == ExpectedTransactionStatus.Confirmed)
            await ExpectAsync(api.RecordPaymentAsync(id), HttpStatusCode.OK);
        else if (status == ExpectedTransactionStatus.Cancelled)
            await ExpectAsync(api.CancelExpectedAsync(id, "No longer required"), HttpStatusCode.OK);
        else
            await fixture.ExecuteSqlAsync("UPDATE expected_transactions SET status = @status, processed_at = @processed WHERE id = @id",
                new NpgsqlParameter("status", (int)status), new NpgsqlParameter("processed", fixture.Today), new NpgsqlParameter("id", id));
        var before = await ReadExpectedAsync(fixture, id);
        var actualCount = await CountActualsAsync(fixture);

        await ExpectAsync(api.PutExpectedAsync(id, new Dictionary<string, string>
        {
            ["ExpectedDate"] = fixture.Today.AddDays(2).ToString("O", CultureInfo.InvariantCulture), ["Description"] = "Forbidden edit"
        }), HttpStatusCode.Conflict);
        await ExpectAsync(api.DeleteAsync($"/api/ExpectedTransaction/{id}"), HttpStatusCode.Conflict);
        await ExpectAsync(api.RecordPaymentAsync(id), HttpStatusCode.Conflict);
        await ExpectAsync(api.CancelExpectedAsync(id, "Forbidden cancellation"), HttpStatusCode.Conflict);
        await ExpectAsync(api.AdjustExpectedAsync(id, 90m, "Forbidden adjustment"), HttpStatusCode.Conflict);

        (await ReadExpectedAsync(fixture, id)).Should().BeEquivalentTo(before);
        (await CountActualsAsync(fixture)).Should().Be(actualCount);
        (await JsonAsync(api.GetAsync($"/api/ExpectedTransaction/{id}"))).GetProperty("id").GetGuid().Should().Be(id);
    }

    [Fact]
    public async Task PostgreSql_Projection_Filters_Search_Count_Ties_And_Clamp_Before_Paging()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync(new DateTimeOffset(2026, 10, 9, 17, 0, 0, TimeSpan.Zero));
        using var api = await fixture.LoginAsync("expected-paging");
        using var foreign = await fixture.LoginAsync("expected-paging-foreign");
        var accountId = await api.CreateAccountAsync("Paging account");
        var templateId = await CreateTemplateAsync(api, accountId, fixture.Today, "NEEDLE schedule");
        var otherTemplateId = await CreateTemplateAsync(api, accountId, fixture.Today, "Unrelated schedule");
        var tiedDate = fixture.Today.AddDays(-2).AddHours(22);
        var wanted = new List<Guid>();
        for (var i = 0; i < 3; i++)
            wanted.Add(await api.CreateExpectedAsync(tiedDate, 10m + i, accountId: accountId,
                description: i == 0 ? "NEEDLE description" : "Unrelated description", category: i == 1 ? "needle category" : "Other"));
        wanted.Add(await api.CreateExpectedAsync(tiedDate.AddDays(1), 20m, accountId: accountId,
            templateId: templateId, description: "Search matches only the template"));
        await api.CreateExpectedAsync(tiedDate, 30m, accountId: accountId, description: "No search match");
        await api.CreateExpectedAsync(fixture.Today, 40m, accountId: accountId, description: "NEEDLE due today");
        var cancelledId = await api.CreateExpectedAsync(tiedDate, 50m, accountId: accountId, description: "NEEDLE cancelled");
        await ExpectAsync(api.CancelExpectedAsync(cancelledId, "Excluded terminal period"), HttpStatusCode.OK);
        await api.CreateExpectedAsync(tiedDate, 60m, accountId: accountId, templateId: otherTemplateId, description: "Another template");
        var foreignAccount = await foreign.CreateAccountAsync("Foreign paging account");
        await foreign.CreateExpectedAsync(tiedDate, 70m, accountId: foreignAccount, description: "NEEDLE foreign");
        // The third tied row does not match search, leaving two tied rows plus a template-name-only match.
        wanted.RemoveAt(2);
        var durableOrder = await fixture.ReadAsync(db => db.ExpectedTransactions.AsNoTracking()
            .Where(e => wanted.Contains(e.Id)).OrderBy(e => e.ExpectedDate).ThenBy(e => e.Id).Select(e => e.Id).ToListAsync());
        durableOrder.Count.Should().Be(3);
        var filter = new
        {
            logicalOperator = "And",
            details = new[] { new { attributeName = "DisplayStatus", value = "4", filterType = "Equal" } }
        };
        var first = await JsonAsync(api.PostJsonAsync("/api/ExpectedTransaction/filter", PageRequest(1, 2, "NeEdLe", filter)));
        var second = await JsonAsync(api.PostJsonAsync("/api/ExpectedTransaction/filter", PageRequest(2, 2, "NeEdLe", filter)));
        var beyond = await JsonAsync(api.PostJsonAsync("/api/ExpectedTransaction/filter", PageRequest(999, 2, "NeEdLe", filter)));
        foreach (var page in new[] { first, second, beyond })
        {
            page.GetProperty("pagination").GetProperty("totalRow").GetInt32().Should().Be(3);
            page.GetProperty("pagination").GetProperty("pageCount").GetInt32().Should().Be(2);
            DeserializeRows(page.GetProperty("data")).Should().OnlyContain(e => e.DisplayStatus == 4 && e.Status == ExpectedTransactionStatus.Pending);
        }
        Ids(first.GetProperty("data")).Should().Equal(durableOrder.Take(2));
        Ids(second.GetProperty("data")).Should().Equal(durableOrder.Skip(2));
        Ids(beyond.GetProperty("data")).Should().Equal(Ids(second.GetProperty("data")));
        beyond.GetProperty("pagination").GetProperty("pageIndex").GetInt32().Should().Be(2);
        var repeat = await JsonAsync(api.PostJsonAsync("/api/ExpectedTransaction/filter", PageRequest(1, 2, "needle", filter,
            new[] { new { field = "ExpectedDate", direction = "Asc" } })));
        Ids(repeat.GetProperty("data")).Should().Equal(durableOrder.Take(2));

        var narrowedFilter = new
        {
            logicalOperator = "And",
            details = new[]
            {
                new { attributeName = "DisplayStatus", value = "4", filterType = "Equal" },
                new { attributeName = "RecurringTransactionTemplateId", value = templateId.ToString(), filterType = "Equal" },
                new { attributeName = "ExpectedDate", value = tiedDate.ToString("O", CultureInfo.InvariantCulture), filterType = "GreaterThan" }
            }
        };
        var narrowed = await JsonAsync(api.PostJsonAsync("/api/ExpectedTransaction/filter", PageRequest(999, 2, "needle", narrowedFilter)));
        narrowed.GetProperty("pagination").GetProperty("totalRow").GetInt32().Should().Be(1);
        narrowed.GetProperty("pagination").GetProperty("pageIndex").GetInt32().Should().Be(1);
        Ids(narrowed.GetProperty("data")).Should().Equal(wanted.Last());
        var empty = await JsonAsync(api.PostJsonAsync("/api/ExpectedTransaction/filter", PageRequest(999, 2, "absent search", filter)));
        empty.GetProperty("pagination").GetProperty("totalRow").GetInt32().Should().Be(0);
        empty.GetProperty("pagination").GetProperty("pageIndex").GetInt32().Should().Be(1);
        empty.GetProperty("data").GetArrayLength().Should().Be(0);
        var clamped = await JsonAsync(api.PostJsonAsync("/api/ExpectedTransaction/filter", PageRequest(-10, 0, "needle", filter)));
        clamped.GetProperty("pagination").GetProperty("pageIndex").GetInt32().Should().Be(1);
        clamped.GetProperty("pagination").GetProperty("pageSize").GetInt32().Should().Be(1);
        Ids(clamped.GetProperty("data")).Should().Equal(durableOrder.Take(1));
        (await fixture.ReadAsync(db => db.ExpectedTransactions.AsNoTracking().Where(e => wanted.Contains(e.Id))
            .Select(e => e.Status).ToListAsync())).Should().OnlyContain(s => s == ExpectedTransactionStatus.Pending);
    }

    [Theory]
    [InlineData("2026-10-09T16:59:59+00:00", "2026-10-09")]
    [InlineData("2026-10-09T17:00:00+00:00", "2026-10-10")]
    public async Task Reminders_And_Display_Status_Use_UtcPlusSeven_Midnight_And_Inclusive_Seven_Day_Boundaries(
        string businessNow, string expectedToday)
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync(DateTimeOffset.Parse(businessNow, CultureInfo.InvariantCulture));
        using var api = await fixture.LoginAsync("expected-midnight");
        using var foreign = await fixture.LoginAsync("expected-midnight-foreign");
        var today = DateTime.ParseExact(expectedToday, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        fixture.Today.Should().Be(today);
        var accountId = await api.CreateAccountAsync("Reminder account");
        var wanted = new[]
        {
            await api.CreateExpectedAsync(today.AddDays(-365), 10m, accountId: accountId),
            await api.CreateExpectedAsync(today.AddDays(-1).AddHours(23).AddMinutes(59), 20m, accountId: accountId),
            await api.CreateExpectedAsync(today, 30m, accountId: accountId),
            await api.CreateExpectedAsync(today.AddHours(23).AddMinutes(59), 40m, accountId: accountId),
            await api.CreateExpectedAsync(today.AddDays(7).AddHours(23).AddMinutes(59), 50m, accountId: accountId)
        };
        await api.CreateExpectedAsync(today.AddDays(8), 60m, accountId: accountId);
        var confirmedId = await api.CreateExpectedAsync(today.AddDays(-10), 70m, accountId: accountId);
        await ExpectAsync(api.RecordPaymentAsync(confirmedId), HttpStatusCode.OK);
        var cancelledId = await api.CreateExpectedAsync(today.AddDays(-9), 80m, accountId: accountId);
        await ExpectAsync(api.CancelExpectedAsync(cancelledId, "No reminder required"), HttpStatusCode.OK);
        var completedId = await api.CreateExpectedAsync(today.AddDays(-8), 90m, accountId: accountId);
        await fixture.ExecuteSqlAsync("UPDATE expected_transactions SET status = @status WHERE id = @id",
            new NpgsqlParameter("status", (int)ExpectedTransactionStatus.Completed), new NpgsqlParameter("id", completedId));
        var deletedId = await api.CreateExpectedAsync(today.AddDays(-7), 100m, accountId: accountId);
        await ExpectAsync(api.DeleteAsync($"/api/ExpectedTransaction/{deletedId}"), HttpStatusCode.OK);
        await foreign.CreateExpectedAsync(today, 110m);

        var json = await JsonAsync(api.GetAsync("/api/ExpectedTransaction/reminders"));
        json.GetProperty("totalRow").GetInt32().Should().Be(5);
        Ids(json.GetProperty("data")).Should().Equal(wanted);
        var reminders = DeserializeRows(json.GetProperty("data"));
        reminders.Select(e => e.DaysUntilDue).Should().Equal(-365, -1, 0, 0, 7);
        reminders.Select(e => e.DisplayStatus).Should().Equal(4, 4, 0, 0, 0);
        reminders.Should().OnlyContain(e => e.Status == ExpectedTransactionStatus.Pending && e.UserId == api.UserId);
        var yesterday = DeserializeExpected(await JsonAsync(api.GetAsync($"/api/ExpectedTransaction/{wanted[1]}")));
        yesterday.DaysUntilDue.Should().Be(-1);
        yesterday.DisplayStatus.Should().Be(4);
        var tomorrowWindow = await JsonAsync(api.GetAsync($"/api/ExpectedTransaction/upcoming/{api.UserId}?days=7"));
        Ids(tomorrowWindow).Should().Equal(wanted.Skip(2));
        (await ReadExpectedAsync(fixture, deletedId)).IsDeleted.Should().NotBeNull();
        (await fixture.ReadAsync(db => db.ExpectedTransactions.AsNoTracking().Where(e => wanted.Contains(e.Id))
            .Select(e => e.Status).ToListAsync())).Should().OnlyContain(s => s == ExpectedTransactionStatus.Pending,
                "Overdue is a read projection, not a stored state transition");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task System_Valid_Signed_Token_With_Malformed_Owner_Is_Forbidden_While_Business_Tables_Are_Locked(
        string? ownerClaim)
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var owner = await fixture.LoginAsync("expected-malformed-owner");
        var accountId = await owner.CreateAccountAsync("Authorization account");
        var templateId = await CreateTemplateAsync(owner, accountId, fixture.Today, "Authorization template");
        var id = await owner.CreateExpectedAsync(fixture.Today, 100m, accountId: accountId, templateId: templateId);
        var before = await ReadExpectedAsync(fixture, id);
        using var malformed = fixture.Factory.CreateClient();
        malformed.DefaultRequestHeaders.Authorization = ExternalBackend.Bearer(SignOwnerToken(ownerClaim));
        await using var lockConnection = await fixture.OpenConnectionAsync();
        await using var transaction = await lockConnection.BeginTransactionAsync();
        await using (var command = new NpgsqlCommand("LOCK TABLE expected_transactions, recurring_transaction_templates IN ACCESS EXCLUSIVE MODE", lockConnection, transaction))
            await command.ExecuteNonQueryAsync();
        var ordinaryRead = owner.GetAsync($"/api/ExpectedTransaction/{id}");
        var authorizationAssertionsCompleted = false;
        try
        {
            await WaitForWaitersAsync(fixture, waiters => waiters.Any(w => w.Blockers.Contains(lockConnection.ProcessID)), [ordinaryRead], updatesOnly: false);
            // A genuine owner is blocked by the tables; malformed authenticated owners must finish before any table access.
            // Invalid amount and mismatched PUT ID would produce 400 if request validation ran before owner authorization.
            await ExpectAsync(malformed.GetAsync($"/api/ExpectedTransaction/{id}").WaitAsync(TimeSpan.FromSeconds(5)), HttpStatusCode.Forbidden);
            await ExpectAsync(malformed.GetAsync("/api/ExpectedTransaction/reminders").WaitAsync(TimeSpan.FromSeconds(5)), HttpStatusCode.Forbidden);
            await ExpectAsync(malformed.PostAsJsonAsync("/api/ExpectedTransaction/filter", PageRequest()).WaitAsync(TimeSpan.FromSeconds(5)), HttpStatusCode.Forbidden);
            await ExpectAsync(malformed.PostAsJsonAsync($"/api/ExpectedTransaction/{id}/record-payment", new { }).WaitAsync(TimeSpan.FromSeconds(5)), HttpStatusCode.Forbidden);
            await ExpectAsync(malformed.PostAsync("/api/ExpectedTransaction", new FormUrlEncodedContent(ExpectedForm(fixture.Today, 0m, accountId))).WaitAsync(TimeSpan.FromSeconds(5)), HttpStatusCode.Forbidden);
            await ExpectAsync(malformed.PutAsync($"/api/ExpectedTransaction/{id}", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Id"] = Guid.NewGuid().ToString(), ["Description"] = "Forbidden edit"
            })).WaitAsync(TimeSpan.FromSeconds(5)), HttpStatusCode.Forbidden);
            await ExpectAsync(malformed.DeleteAsync($"/api/ExpectedTransaction/{id}").WaitAsync(TimeSpan.FromSeconds(5)), HttpStatusCode.Forbidden);
            await ExpectAsync(malformed.GetAsync($"/api/RecurringTransactionTemplate/{templateId}").WaitAsync(TimeSpan.FromSeconds(5)), HttpStatusCode.Forbidden);
            ordinaryRead.IsCompleted.Should().BeFalse();
            authorizationAssertionsCompleted = true;
        }
        finally
        {
            await transaction.RollbackAsync();
            if (!authorizationAssertionsCompleted) await DrainAsync(ordinaryRead);
        }
        await ExpectAsync(ordinaryRead, HttpStatusCode.OK);
        (await ReadExpectedAsync(fixture, id)).Should().BeEquivalentTo(before);
        (await CountActualsAsync(fixture)).Should().Be(0);
        (await fixture.ReadAsync(db => db.ExpectedTransactions.IgnoreQueryFilters().CountAsync())).Should().Be(1);
    }

    [Fact]
    public async Task Legacy_Foreign_Navigation_Is_Masked_On_Every_Read_Path_And_New_Foreign_References_Are_Rejected()
    {
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var owner = await fixture.LoginAsync("expected-legacy-owner");
        using var foreign = await fixture.LoginAsync("expected-legacy-foreign");
        var ownAccount = await owner.CreateAccountAsync("Visible owner account");
        var foreignAccount = await foreign.CreateAccountAsync("PRIVATE FOREIGN ACCOUNT");
        var foreignTemplate = await CreateTemplateAsync(foreign, foreignAccount, fixture.Today, "PRIVATE FOREIGN TEMPLATE");
        var id = await owner.CreateExpectedAsync(fixture.Today, 100m, accountId: ownAccount, description: "Owned legacy period");
        await fixture.ExecuteSqlAsync("""
            UPDATE expected_transactions SET account_id = @account, recurring_transaction_template_id = @template
            WHERE id = @id
            """, new NpgsqlParameter("account", foreignAccount), new NpgsqlParameter("template", foreignTemplate), new NpgsqlParameter("id", id));
        var legacyBefore = await ReadExpectedAsync(fixture, id);
        legacyBefore.UserId.Should().Be(owner.UserId);
        legacyBefore.AccountId.Should().Be(foreignAccount);
        legacyBefore.RecurringTransactionTemplateId.Should().Be(foreignTemplate);
        var detail = DeserializeExpected(await JsonAsync(owner.GetAsync($"/api/ExpectedTransaction/{id}")));
        AssertMaskedNavigation(detail, id, owner.UserId);
        var page = await JsonAsync(owner.PostJsonAsync("/api/ExpectedTransaction/filter", PageRequest()));
        page.GetProperty("pagination").GetProperty("totalRow").GetInt32().Should().Be(1);
        AssertMaskedNavigation(DeserializeRows(page.GetProperty("data")).Single(), id, owner.UserId);
        var reminders = await JsonAsync(owner.GetAsync("/api/ExpectedTransaction/reminders"));
        reminders.GetProperty("totalRow").GetInt32().Should().Be(1);
        AssertMaskedNavigation(DeserializeRows(reminders.GetProperty("data")).Single(), id, owner.UserId);
        foreach (var route in new[]
        {
            $"/api/ExpectedTransaction/template/{foreignTemplate}",
            $"/api/ExpectedTransaction/account/{foreignAccount}",
            $"/api/ExpectedTransaction/pending/{owner.UserId}",
            $"/api/ExpectedTransaction/upcoming/{owner.UserId}?days=7",
            $"/api/ExpectedTransaction/date-range/{owner.UserId}?startDate={fixture.Today:yyyy-MM-dd}&endDate={fixture.Today:yyyy-MM-dd}"
        })
        {
            var row = DeserializeRows(await JsonAsync(owner.GetAsync(route))).Single();
            AssertMaskedNavigation(row, id, owner.UserId);
        }
        var all = await fixture.WithOwnerAsync(owner.UserId, async services =>
            (await services.GetRequiredService<IExpectedTransactionService>().GetAllDtoAsync())!.ToList());
        AssertMaskedNavigation(all.Single(), id, owner.UserId);
        var privateSearch = await JsonAsync(owner.PostJsonAsync("/api/ExpectedTransaction/filter", PageRequest(search: "PRIVATE FOREIGN")));
        privateSearch.GetProperty("pagination").GetProperty("totalRow").GetInt32().Should().Be(0,
            "hidden navigation text cannot be used as a cross-owner search oracle");
        await ExpectAsync(foreign.GetAsync($"/api/ExpectedTransaction/{id}"), HttpStatusCode.NotFound);
        var foreignRead = await JsonAsync(foreign.GetAsync($"/api/ExpectedTransaction/template/{foreignTemplate}"));
        foreignRead.GetArrayLength().Should().Be(0);
        await ExpectAsync(owner.RecordPaymentAsync(id), HttpStatusCode.NotFound);

        var foreignAccountForm = ExpectedForm(fixture.Today.AddDays(1), 200m, foreignAccount);
        await ExpectAsync(owner.PostFormAsync("/api/ExpectedTransaction", foreignAccountForm), HttpStatusCode.NotFound);
        var foreignTemplateForm = ExpectedForm(fixture.Today.AddDays(2), 300m, ownAccount);
        foreignTemplateForm["RecurringTransactionTemplateId"] = foreignTemplate.ToString();
        await ExpectAsync(owner.PostFormAsync("/api/ExpectedTransaction", foreignTemplateForm), HttpStatusCode.NotFound);
        (await fixture.ReadAsync(db => db.ExpectedTransactions.IgnoreQueryFilters().CountAsync())).Should().Be(1);
        (await ReadExpectedAsync(fixture, id)).Should().BeEquivalentTo(legacyBefore);
        (await CountActualsAsync(fixture)).Should().Be(0);
    }

    private static Task<HttpResponseMessage> MutateAsync(BackendFinanceClient api, Guid id, string mutation, DateTime editedDate) => mutation switch
    {
        "cancel" => api.CancelExpectedAsync(id, "Concurrent cancellation"),
        "adjust" => api.AdjustExpectedAsync(id, 160m, "Concurrent revision"),
        "same-amount-adjust" => api.AdjustExpectedAsync(id, 120m, "Concurrent revision"),
        "delete" => api.DeleteAsync($"/api/ExpectedTransaction/{id}"),
        "put-date" => api.PutExpectedAsync(id, new Dictionary<string, string>
        {
            ["ExpectedDate"] = editedDate.ToString("O", CultureInfo.InvariantCulture),
            ["Description"] = "Revised invoice", ["Category"] = "Revised category", ["Notes"] = "Revised notes"
        }),
        _ => throw new ArgumentOutOfRangeException(nameof(mutation))
    };

    private static async Task<Guid> CreateTemplateAsync(BackendFinanceClient api, Guid accountId, DateTime date, string name)
    {
        var json = await JsonAsync(api.PostFormAsync("/api/RecurringTransactionTemplate", new Dictionary<string, string>
        {
            ["AccountId"] = accountId.ToString(), ["Name"] = name, ["Amount"] = "100",
            ["TransactionType"] = "Expense", ["Frequency"] = "Monthly",
            ["StartDate"] = date.ToString("O", CultureInfo.InvariantCulture),
            ["NextExecutionDate"] = date.ToString("O", CultureInfo.InvariantCulture),
            ["IsActive"] = "true", ["AutoGenerate"] = "false", ["DaysInAdvance"] = "7"
        }));
        return json.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateManualActualAsync(BackendFinanceClient api, Guid accountId, DateTime date, decimal amount)
    {
        var json = await JsonAsync(api.PostFormAsync("/api/Transaction", new Dictionary<string, string>
        {
            ["AccountId"] = accountId.ToString(), ["TransactionDate"] = date.ToString("O", CultureInfo.InvariantCulture),
            ["RevenueAmount"] = "0", ["SpentAmount"] = amount.ToString(CultureInfo.InvariantCulture),
            ["Balance"] = "0", ["Description"] = "Existing actual transaction"
        }));
        return json.GetProperty("id").GetGuid();
    }

    private static Dictionary<string, string> ExpectedForm(DateTime date, decimal amount, Guid? accountId = null)
    {
        var form = new Dictionary<string, string>
        {
            ["ExpectedDate"] = date.ToString("O", CultureInfo.InvariantCulture),
            ["ExpectedAmount"] = amount.ToString(CultureInfo.InvariantCulture), ["TransactionType"] = "Expense", ["Status"] = "Pending"
        };
        if (accountId.HasValue) form["AccountId"] = accountId.Value.ToString();
        return form;
    }

    private static object PageRequest(int pageIndex = 1, int pageSize = 20, string? search = null, object? filter = null, object? orders = null) =>
        new { pagination = new { pageIndex, pageSize }, searchValue = search, filter, orders };

    private static async Task ExpectAsync(Task<HttpResponseMessage> request, HttpStatusCode status)
    {
        using var response = await request;
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(status, content);
        if ((int)status >= 400)
        {
            using var json = JsonDocument.Parse(content);
            json.RootElement.GetProperty("code").GetString().Should().NotBeNullOrWhiteSpace();
        }
    }

    private static async Task<JsonElement> JsonAsync(Task<HttpResponseMessage> request)
    {
        using var response = await request;
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        using var json = JsonDocument.Parse(content);
        return json.RootElement.Clone();
    }

    private static ExpectedTransactionViewModel DeserializeExpected(JsonElement json) =>
        json.Deserialize<ExpectedTransactionViewModel>(JsonOptions)!;

    private static List<ExpectedTransactionViewModel> DeserializeRows(JsonElement json) =>
        json.Deserialize<List<ExpectedTransactionViewModel>>(JsonOptions)!;

    private static IEnumerable<Guid> Ids(JsonElement json) => json.EnumerateArray().Select(row => row.GetProperty("id").GetGuid());

    private static Task<ExpectedTransaction> ReadExpectedAsync(Thm3PostgresFixture fixture, Guid id) =>
        fixture.ReadAsync(db => db.ExpectedTransactions.IgnoreQueryFilters().AsNoTracking().SingleAsync(e => e.Id == id));

    private static Task<int> CountActualsAsync(Thm3PostgresFixture fixture) =>
        fixture.ReadAsync(db => db.Transactions.IgnoreQueryFilters().CountAsync());

    private static Task<decimal> ReadBalanceAsync(Thm3PostgresFixture fixture, Guid accountId) =>
        fixture.ReadAsync(db => db.Accounts.IgnoreQueryFilters().Where(a => a.Id == accountId).Select(a => a.CurrentBalance).SingleAsync());

    private static async Task<long> ReadVersionAsync(Thm3PostgresFixture fixture, Guid id)
    {
        await using var connection = await fixture.OpenConnectionAsync();
        await using var command = new NpgsqlCommand("SELECT xmin::text::bigint FROM expected_transactions WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", id);
        return (long)(await command.ExecuteScalarAsync())!;
    }

    private static async Task AssertSqlNullReferencesAsync(Thm3PostgresFixture fixture, Guid id)
    {
        await using var connection = await fixture.OpenConnectionAsync();
        await using var command = new NpgsqlCommand("SELECT account_id IS NULL, recurring_transaction_template_id IS NULL FROM expected_transactions WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync();
        (await reader.ReadAsync()).Should().BeTrue();
        reader.GetBoolean(0).Should().BeTrue("an omitted optional account must be SQL NULL, not Guid.Empty");
        reader.GetBoolean(1).Should().BeTrue("an omitted optional template must be SQL NULL, not Guid.Empty");
    }

    private static async Task AssertSingleActualMatchesAsync(Thm3PostgresFixture fixture, ExpectedTransaction expected, Guid accountId)
    {
        var actual = await fixture.ReadAsync(db => db.Transactions.IgnoreQueryFilters().AsNoTracking().SingleAsync());
        expected.ActualTransactionId.Should().Be(actual.Id);
        actual.UserId.Should().Be(expected.UserId);
        actual.AccountId.Should().Be(accountId);
        actual.TransactionDate.Should().Be(expected.ExpectedDate);
        actual.SpentAmount.Should().Be(expected.ExpectedAmount);
        actual.RevenueAmount.Should().Be(0m);
        actual.Description.Should().Be(expected.Description);
        actual.CategorySummary.Should().Be(expected.Category);
        actual.IsDeleted.Should().BeNull();
    }

    private static void AssertMaskedNavigation(ExpectedTransactionViewModel row, Guid id, Guid ownerId)
    {
        row.Id.Should().Be(id);
        row.UserId.Should().Be(ownerId);
        row.AccountName.Should().BeNullOrEmpty();
        row.AccountType.Should().BeNull();
        row.TemplateName.Should().BeNullOrEmpty();
        row.Description.Should().Be("Owned legacy period");
    }

    private static string SignOwnerToken(string? ownerClaim)
    {
        var claims = new List<Claim> { new("name", "Malformed owner characterization") };
        if (ownerClaim != null) claims.Add(new Claim("nameid", ownerClaim));
        var now = TimeProvider.System.GetUtcNow().UtcDateTime;
        var token = new JwtSecurityToken(ExternalBackend.Require("TIHOMO_TEST_JWT_ISSUER"),
            ExternalBackend.Require("TIHOMO_TEST_JWT_AUDIENCE"), claims, now.AddMinutes(-1), now.AddMinutes(10),
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ExternalBackend.Require("TIHOMO_TEST_JWT_SECRET"))), SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record LockWaiter(int ProcessId, int[] Blockers);

    private static async Task<List<LockWaiter>> WaitForWaitersAsync(Thm3PostgresFixture fixture,
        Func<List<LockWaiter>, bool> ready, IReadOnlyCollection<Task<HttpResponseMessage>> requests, bool updatesOnly = true)
    {
        await using var observer = await fixture.OpenConnectionAsync();
        var watch = Stopwatch.StartNew();
        while (watch.Elapsed < TimeSpan.FromSeconds(20))
        {
            var waiters = new List<LockWaiter>();
            await using (var command = new NpgsqlCommand("""
                SELECT pid, pg_blocking_pids(pid)
                FROM pg_stat_activity
                WHERE datname = current_database() AND pid <> pg_backend_pid()
                  AND state = 'active' AND wait_event_type = 'Lock'
                  AND (NOT @updates_only OR query ILIKE '%UPDATE expected_transactions%'
                       OR query ILIKE '%UPDATE "expected_transactions"%')
                """, observer))
            {
                command.CommandTimeout = 5;
                command.Parameters.AddWithValue("updates_only", updatesOnly);
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync()) waiters.Add(new LockWaiter(reader.GetInt32(0), reader.GetFieldValue<int[]>(1)));
            }
            if (ready(waiters)) return waiters;
            if (requests.Any(request => request.IsCompleted))
                throw new InvalidOperationException("A request completed before reaching the relational write barrier.");
            await Task.Delay(TimeSpan.FromMilliseconds(20));
        }
        throw new TimeoutException("The expected PostgreSQL lock waiters did not reach the deterministic barrier.");
    }

    private static async Task<HttpResponseMessage[]> ReleaseAndCollectAsync(Thm3PostgresFixture fixture,
        RowWriteBarrier barrier, Task<HttpResponseMessage>[] requests, int waiterCount)
    {
        try
        {
            await WaitForWaitersAsync(fixture, waiters => waiters.Count == waiterCount, requests);
            await barrier.ReleaseAsync();
            return await Task.WhenAll(requests).WaitAsync(TimeSpan.FromSeconds(30));
        }
        catch
        {
            await barrier.ReleaseAsync();
            await DrainAsync(requests);
            throw;
        }
    }

    private static async Task DrainAsync(params Task<HttpResponseMessage>?[] requests)
    {
        foreach (var request in requests)
        {
            if (request == null) continue;
            try
            {
                (await request.WaitAsync(TimeSpan.FromSeconds(30))).Dispose();
            }
            catch
            {
                // Preserve the barrier/assertion failure after giving the real request a chance to unwind.
            }
        }
    }

    private sealed class RowWriteBarrier(NpgsqlConnection connection, NpgsqlTransaction transaction) : IAsyncDisposable
    {
        private bool _released;
        public int ProcessId => connection.ProcessID;

        public static async Task<RowWriteBarrier> HoldAsync(Thm3PostgresFixture fixture, params Guid[] ids)
        {
            var connection = await fixture.OpenConnectionAsync();
            NpgsqlTransaction? transaction = null;
            try
            {
                transaction = await connection.BeginTransactionAsync();
                await using var command = new NpgsqlCommand("SELECT id FROM expected_transactions WHERE id = ANY(@ids) ORDER BY id FOR UPDATE", connection, transaction);
                command.Parameters.AddWithValue("ids", ids);
                await using var reader = await command.ExecuteReaderAsync();
                var count = 0;
                while (await reader.ReadAsync()) count++;
                count.Should().Be(ids.Length);
                return new RowWriteBarrier(connection, transaction);
            }
            catch
            {
                if (transaction != null) await transaction.DisposeAsync();
                await connection.DisposeAsync();
                throw;
            }
        }

        public async Task ReleaseAsync()
        {
            if (_released) return;
            await transaction.RollbackAsync();
            _released = true;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await ReleaseAsync();
            }
            finally
            {
                await transaction.DisposeAsync();
                await connection.DisposeAsync();
            }
        }
    }
}
