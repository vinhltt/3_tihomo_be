using System.Net;
using System.Text.Json;

namespace CoreFinance.Api.Tests.Integration;

/// <summary>
///     (EN) Real HTTP + PostgreSQL proof that the active-templates route takes its owner only from the bearer token.<br />
///     (VI) Bằng chứng HTTP + PostgreSQL thật rằng route mẫu đang hoạt động chỉ lấy owner từ bearer token.
/// </summary>
[Trait("Category", "Characterization")]
public sealed class RecurringActiveTemplatesPostgresTests
{
    private const string ActiveRoute = "/api/RecurringTransactionTemplate/active";

    /// <summary>
    ///     (EN) Each owner sees only their own active templates; an inactive template and the other owner's rows never appear.<br />
    ///     (VI) Mỗi owner chỉ thấy các mẫu đang hoạt động của mình; mẫu ngưng hoạt động và dữ liệu owner khác không xuất hiện.
    /// </summary>
    [Fact]
    public async Task Active_Route_Should_Return_Only_The_Token_Owners_Active_Templates()
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var ownerA = await fixture.LoginAsync("active-templates-a");
        using var ownerB = await fixture.LoginAsync("active-templates-b");
        var accountA = await ownerA.CreateAccountAsync("Active templates account A");
        var accountB = await ownerB.CreateAccountAsync("Active templates account B");
        var activeA = Template(ownerA.UserId, accountA, "Owner A active", fixture.Today, isActive: true);
        var inactiveA = Template(ownerA.UserId, accountA, "Owner A inactive", fixture.Today, isActive: false);
        var activeB = Template(ownerB.UserId, accountB, "Owner B active", fixture.Today, isActive: true);
        await SeedAsync(fixture, ownerA.UserId, activeA, inactiveA);
        await SeedAsync(fixture, ownerB.UserId, activeB);

        // Act
        var returnedToA = await ReadActiveAsync(ownerA);
        var returnedToB = await ReadActiveAsync(ownerB);

        // Assert
        returnedToA.Select(row => row.Id).Should().BeEquivalentTo(new[] { activeA.Id },
            "owner A must receive only A's active template, not A's inactive one nor B's");
        returnedToA.Should().OnlyContain(row => row.IsActive && row.UserId == ownerA.UserId);
        returnedToB.Select(row => row.Id).Should().BeEquivalentTo(new[] { activeB.Id },
            "owner B must receive only B's active template");
        returnedToB.Should().OnlyContain(row => row.IsActive && row.UserId == ownerB.UserId);
    }

    /// <summary>
    ///     (EN) A request without a token is rejected with 401, and the removed {userId} route no longer matches.<br />
    ///     (VI) Request không có token bị từ chối 401, và route {userId} đã bị gỡ không còn khớp.
    /// </summary>
    [Fact]
    public async Task Active_Route_Should_Reject_Missing_Token_With_401_And_No_Longer_Match_A_UserId_Segment()
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var owner = await fixture.LoginAsync("active-templates-auth");
        var account = await owner.CreateAccountAsync("Active templates auth account");
        await SeedAsync(fixture, owner.UserId,
            Template(owner.UserId, account, "Owner active", fixture.Today, isActive: true));
        using var anonymous = fixture.Factory.CreateClient();

        // Act
        using var withoutToken = await anonymous.GetAsync(ActiveRoute);
        using var legacyRoute = await owner.GetAsync($"{ActiveRoute}/{owner.UserId}");

        // Assert
        withoutToken.StatusCode.Should().Be(HttpStatusCode.Unauthorized, await withoutToken.Content.ReadAsStringAsync());
        legacyRoute.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "the owner is taken from the token only, so a userId path segment must not match any action");
    }

    private static async Task<List<ActiveTemplate>> ReadActiveAsync(BackendFinanceClient api)
    {
        using var response = await api.GetAsync(ActiveRoute);
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        using var json = JsonDocument.Parse(content);
        return json.RootElement.EnumerateArray()
            .Select(row => new ActiveTemplate(row.GetProperty("id").GetGuid(), row.GetProperty("userId").GetGuid(),
                row.GetProperty("isActive").GetBoolean()))
            .ToList();
    }

    private static RecurringTransactionTemplate Template(Guid owner, Guid account, string name, DateTime start,
        bool isActive) => new()
    {
        Id = Guid.NewGuid(), UserId = owner, AccountId = account, Name = name, Amount = 100m,
        TransactionType = RecurringTransactionType.Expense, Frequency = RecurrenceFrequency.Monthly,
        StartDate = start, NextExecutionDate = start, IsActive = isActive, AutoGenerate = false, DaysInAdvance = 1,
        CreatedAt = new DateTime(2026, 1, 1), UpdatedAt = new DateTime(2026, 1, 1), CreateBy = owner.ToString()
    };

    private static Task SeedAsync(Thm3PostgresFixture fixture, Guid owner, params RecurringTransactionTemplate[] templates) =>
        fixture.WithOwnerAsync(owner, async services =>
        {
            // Persist directly so no create-time generation runs; every row keeps the owner given here.
            var database = services.GetRequiredService<CoreFinanceDbContext>();
            database.RecurringTransactionTemplates.AddRange(templates);
            await database.SaveChangesAsync();
        });

    private sealed record ActiveTemplate(Guid Id, Guid UserId, bool IsActive);
}
