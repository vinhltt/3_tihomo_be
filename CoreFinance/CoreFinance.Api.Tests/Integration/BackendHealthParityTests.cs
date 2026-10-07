using System.Net;
using System.Text.Json;
using Tests.Shared.Helpers;

namespace CoreFinance.Api.Tests.Integration;

/// <summary>
///     Characterization: all 6 backend APIs answer /health as healthy (EN)<br/>
///     Characterization: cả 6 API backend trả /health healthy (VI)
/// </summary>
[Trait("Category", "Characterization")]
public class BackendHealthParityTests
{
    [Theory]
    [InlineData("gateway")]
    [InlineData("identity")]
    [InlineData("money")]
    [InlineData("planning")]
    [InlineData("corefinance")]
    [InlineData("excel")]
    public async Task Health_Should_Report_Healthy(string service)
    {
        using var client = ExternalBackend.Client(service);

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);
        body.Should().NotContain("<html", "a landing page is not a health response");
        // Health-check JSON writers use either "Status" or "status"; plain-text writers return the status word.
        var status = body.TrimStart().StartsWith('{')
            ? (JsonDocument.Parse(body).RootElement.TryGetProperty("Status", out var s)
                ? s
                : JsonDocument.Parse(body).RootElement.GetProperty("status")).GetString()
            : body.Trim();
        status.Should().Be("Healthy");
    }
}
