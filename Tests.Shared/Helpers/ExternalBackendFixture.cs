using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tests.Shared.Helpers;

/// <summary>
///     Access to the real 6-service backend started by run-backend-characterization.sh (EN)<br/>
///     Truy cập backend 6 service thật do run-backend-characterization.sh khởi chạy (VI)
/// </summary>
/// <remarks>
///     Missing configuration fails the test with a diagnostic: characterization never skips or falls back to in-memory.
/// </remarks>
public static class ExternalBackend
{
    private static readonly Lazy<IReadOnlyDictionary<string, Uri>> Urls = new(() =>
    {
        var raw = Require("TIHOMO_BACKEND_URLS");
        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => p.Split('=', 2))
            .ToDictionary(p => p[0], p => new Uri(p[1]));
    });

    public static string Require(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } v
            ? v
            : throw new InvalidOperationException(
                $"{name} is not set. Run characterization through run-backend-characterization.sh; it must not be skipped.");

    public static HttpClient Client(string service) =>
        new() { BaseAddress = Urls.Value.TryGetValue(service, out var u)
            ? u
            : throw new InvalidOperationException($"Service '{service}' missing from TIHOMO_BACKEND_URLS"),
            Timeout = TimeSpan.FromSeconds(30) };

    /// <summary>PostgreSQL connection string for one fixture database.</summary>
    public static string Database(string dbEnvVar) => $"{Require("TIHOMO_TEST_PG")};Database={Require(dbEnvVar)}";

    /// <summary>Logs in through the gateway route the frontend uses and returns the issued access token.</summary>
    public static async Task<string> LoginThroughGatewayAsync(string username)
    {
        using var gateway = Client("gateway");
        var response = await gateway.PostAsJsonAsync("/api/identity/Auth/login", new { username, password = "unused-by-baseline" });
        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("accessToken").GetString()!;
    }

    public static AuthenticationHeaderValue Bearer(string token) => new("Bearer", token);

    /// <summary>Unique synthetic username per test so rows never collide across runs.</summary>
    public static string NewUser(string prefix) => $"{prefix}-{Guid.CreateVersion7():N}@characterization.test";
}
