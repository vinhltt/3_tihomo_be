using System.Net;
using System.Text.Json;

namespace CoreFinance.Api.Tests.Integration;

/// <summary>
///     Characterization: POST form transaction is durable in PostgreSQL and owned by the token user (EN)<br/>
///     Characterization: POST form giao dịch được lưu bền trong PostgreSQL và thuộc user của token (VI)
/// </summary>
[Trait("Category", "Characterization")]
public class TransactionPersistenceParityTests
{
    [Fact]
    public async Task Post_Form_Transaction_Should_Persist_Row_And_Read_Back()
    {
        using var api = await BackendFinanceClient.LoginAsync("tx-persist");
        var accountId = await api.CreateAccountAsync("Persistence account");
        var spoofedUser = Guid.CreateVersion7();

        var create = await api.PostFormAsync("/api/Transaction", new Dictionary<string, string>
        {
            ["AccountId"] = accountId.ToString(), ["TransactionDate"] = "2026-10-01T00:00:00Z",
            ["RevenueAmount"] = "0", ["SpentAmount"] = "45000", ["Balance"] = "955000",
            ["Description"] = "Coffee characterization", ["UserId"] = spoofedUser.ToString()
        });

        create.StatusCode.Should().Be(HttpStatusCode.OK, await create.Content.ReadAsStringAsync());
        var id = await BackendFinanceClient.IdOf(create);

        var row = await BackendFinanceClient.ReadTransactionRowAsync(id);
        row.Should().NotBeNull("the transaction must be durable, not only echoed");
        row!["account_id"].Should().Be(accountId);
        row["user_id"].Should().Be(api.UserId, "POST overrides a request UserId with the token claim");
        row["revenue_amount"].Should().Be(0m);
        row["spent_amount"].Should().Be(45000m);
        row["balance"].Should().Be(955000m);
        row["transaction_date"].Should().Be(new DateTime(2026, 10, 1, 0, 0, 0));
        row["description"].Should().Be("Coffee characterization");
        row["is_deleted"].Should().BeNull();

        var read = await api.GetAsync($"/api/Transaction/{id}");
        read.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await read.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("userId").GetGuid().Should().Be(api.UserId);
        doc.RootElement.GetProperty("spentAmount").GetDecimal().Should().Be(45000m);
        doc.RootElement.GetProperty("balance").GetDecimal().Should().Be(955000m);
    }
}
