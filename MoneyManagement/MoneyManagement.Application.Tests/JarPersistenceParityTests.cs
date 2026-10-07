using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Tests.Shared.Helpers;

namespace MoneyManagement.Application.Tests;

/// <summary>
///     Characterization: Jar create/read against the real MoneyManagement API and PostgreSQL (EN)<br/>
///     Characterization: tạo/đọc Jar qua MoneyManagement API thật và PostgreSQL (VI)
/// </summary>
[Trait("Category", "Characterization")]
public class JarPersistenceParityTests
{
    [Fact]
    public async Task Created_Jar_Should_Persist_And_Be_Readable_By_Its_Owner()
    {
        var token = await ExternalBackend.LoginThroughGatewayAsync(ExternalBackend.NewUser("jar"));
        using var money = ExternalBackend.Client("money");
        money.DefaultRequestHeaders.Authorization = ExternalBackend.Bearer(token);
        var body = new StringContent(
            JsonSerializer.Serialize(new { name = "Necessities", jarType = 1, allocationPercentage = 55m, targetAmount = 1000000m }),
            Encoding.UTF8, "application/json");

        var create = await money.PostAsync("/api/Jar", body);

        create.StatusCode.Should().Be(HttpStatusCode.Created, await create.Content.ReadAsStringAsync());
        using var created = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var id = created.RootElement.GetProperty("id").GetGuid();

        var read = await money.GetAsync($"/api/Jar/{id}");
        read.StatusCode.Should().Be(HttpStatusCode.OK, await read.Content.ReadAsStringAsync());
        using var jar = JsonDocument.Parse(await read.Content.ReadAsStringAsync());
        jar.RootElement.GetProperty("name").GetString().Should().Be("Necessities");
        jar.RootElement.GetProperty("jarType").GetInt32().Should().Be(1);
        jar.RootElement.GetProperty("allocationPercentage").GetDecimal().Should().Be(55m);
        jar.RootElement.GetProperty("targetAmount").GetDecimal().Should().Be(1000000m);
        jar.RootElement.GetProperty("currentBalance").GetDecimal().Should().Be(0m, "a new jar starts empty");

        var duplicate = new StringContent(JsonSerializer.Serialize(new { name = "Second", jarType = 1 }), Encoding.UTF8, "application/json");
        (await money.PostAsync("/api/Jar", duplicate)).StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "one jar per type per user");
    }

    [Fact]
    public async Task Jar_Should_Not_Be_Readable_By_Another_User()
    {
        using var owner = ExternalBackend.Client("money");
        owner.DefaultRequestHeaders.Authorization =
            ExternalBackend.Bearer(await ExternalBackend.LoginThroughGatewayAsync(ExternalBackend.NewUser("jar-owner")));
        var create = await owner.PostAsync("/api/Jar",
            new StringContent(JsonSerializer.Serialize(new { name = "Play", jarType = 4 }), Encoding.UTF8, "application/json"));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        using var created = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        using var stranger = ExternalBackend.Client("money");
        stranger.DefaultRequestHeaders.Authorization =
            ExternalBackend.Bearer(await ExternalBackend.LoginThroughGatewayAsync(ExternalBackend.NewUser("jar-stranger")));

        var read = await stranger.GetAsync($"/api/Jar/{created.RootElement.GetProperty("id").GetGuid()}");

        read.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
