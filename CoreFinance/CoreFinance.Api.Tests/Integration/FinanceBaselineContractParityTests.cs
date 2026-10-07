using System.Net;
using System.Text.Json;

namespace CoreFinance.Api.Tests.Integration;

/// <summary>
///     Characterization of finance contract branches not covered elsewhere: PUT, POST filter paging/search, soft delete (EN)<br/>
///     Characterization các nhánh hợp đồng finance chưa được phủ: PUT, POST filter paging/search, soft delete (VI)
/// </summary>
[Trait("Category", "Characterization")]
public class FinanceBaselineContractParityTests
{
    private static Dictionary<string, string> Tx(Guid accountId, string? description, decimal spent, string? category = null)
    {
        var form = new Dictionary<string, string>
        {
            ["AccountId"] = accountId.ToString(), ["TransactionDate"] = "2026-10-02T00:00:00Z",
            ["RevenueAmount"] = "0", ["SpentAmount"] = spent.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["Balance"] = "0"
        };
        if (description is not null) form["Description"] = description;
        if (category is not null) form["CategorySummary"] = category;
        return form;
    }

    private static async Task<Guid> CreateTx(BackendFinanceClient api, Dictionary<string, string> form)
    {
        var response = await api.PostFormAsync("/api/Transaction", form);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await BackendFinanceClient.IdOf(response);
    }

    private static async Task<JsonElement> Filter(BackendFinanceClient api, object body)
    {
        var response = await api.PostJsonAsync("/api/Transaction/filter", body);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();
    }

    [Fact]
    public async Task Put_Should_Require_Route_Id_Match_And_Keep_Request_UserId()
    {
        using var api = await BackendFinanceClient.LoginAsync("tx-put");
        var accountId = await api.CreateAccountAsync("Put account");
        var id = await CreateTx(api, Tx(accountId, "Before update", 1000));
        var requestUser = Guid.CreateVersion7();

        var mismatch = Tx(accountId, "Mismatch", 1);
        mismatch["Id"] = Guid.CreateVersion7().ToString();
        (await api.PutFormAsync($"/api/Transaction/{id}", mismatch)).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var update = Tx(accountId, "After update", 2000);
        update["Id"] = id.ToString();
        update["UserId"] = requestUser.ToString();
        var put = await api.PutFormAsync($"/api/Transaction/{id}", update);

        put.StatusCode.Should().Be(HttpStatusCode.OK, await put.Content.ReadAsStringAsync());
        var row = await BackendFinanceClient.ReadTransactionRowAsync(id);
        row!["description"].Should().Be("After update");
        row["spent_amount"].Should().Be(2000m);
        row["user_id"].Should().Be(requestUser, "PUT has no POST-style claim override in the baseline");
    }

    [Fact]
    public async Task Filter_Should_Default_And_Clamp_Paging_Including_Empty_Results()
    {
        using var api = await BackendFinanceClient.LoginAsync("tx-paging");
        var accountId = await api.CreateAccountAsync("Paging account");
        for (var i = 1; i <= 3; i++) await CreateTx(api, Tx(accountId, $"Paging row {i}", i));

        var defaults = await Filter(api, new { });
        defaults.GetProperty("pagination").GetProperty("pageSize").GetInt32().Should().Be(10);
        defaults.GetProperty("pagination").GetProperty("pageIndex").GetInt32().Should().Be(1);
        defaults.GetProperty("data").GetArrayLength().Should().Be(3);

        var clamped = await Filter(api, new { pagination = new { pageIndex = 99, pageSize = 0 } });
        clamped.GetProperty("pagination").GetProperty("pageSize").GetInt32().Should().Be(1, "page size has a minimum of 1");
        clamped.GetProperty("pagination").GetProperty("pageIndex").GetInt32().Should().Be(3, "page index clamps to the last page");
        clamped.GetProperty("data").GetArrayLength().Should().Be(1);

        var empty = await Filter(api, new { searchValue = "no-such-row", pagination = new { pageIndex = 5, pageSize = 10 } });
        empty.GetProperty("pagination").GetProperty("pageIndex").GetInt32().Should().Be(1, "empty results clamp to page 1");
        empty.GetProperty("data").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Search_Should_Be_Case_Insensitive_And_Exclude_Null_Description_Even_When_Category_Matches()
    {
        using var api = await BackendFinanceClient.LoginAsync("tx-search");
        var accountId = await api.CreateAccountAsync("Search account");
        var byDescription = await CreateTx(api, Tx(accountId, "Morning COFFEE", 10));
        var byCategory = await CreateTx(api, Tx(accountId, "Lunch", 20, "coffee shops"));
        await CreateTx(api, Tx(accountId, null, 30, "coffee beans"));

        var result = await Filter(api, new { searchValue = "Coffee" });

        result.GetProperty("data").EnumerateArray().Select(e => e.GetProperty("id").GetGuid())
            .Should().BeEquivalentTo([byDescription, byCategory]);
    }

    [Fact]
    public async Task Soft_Delete_Should_Mark_Row_And_Hide_It()
    {
        using var api = await BackendFinanceClient.LoginAsync("tx-delete");
        var accountId = await api.CreateAccountAsync("Delete account");
        var id = await CreateTx(api, Tx(accountId, "To delete", 5));

        (await api.DeleteAsync($"/api/Transaction/{id}")).StatusCode.Should().Be(HttpStatusCode.OK);

        var row = await BackendFinanceClient.ReadTransactionRowAsync(id);
        row.Should().NotBeNull("soft delete keeps the row");
        row!["is_deleted"].Should().NotBeNull();
        (await api.DeleteAsync($"/api/Transaction/{id}")).StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "baseline: deleting an already-deleted or unknown id surfaces ArgumentNullException as 400");
        (await api.GetAsync($"/api/Transaction/{id}")).StatusCode.Should().Be(HttpStatusCode.InternalServerError,
            "baseline: a missing entity is service-null -> 500");
    }
}
