using System.Text.Json;
using Npgsql;
using Tests.Shared.Helpers;

namespace CoreFinance.Api.Tests.Integration;

/// <summary>
///     Calls the real CoreFinance API with the form binding it actually uses, and reads rows straight from PostgreSQL (EN)<br/>
///     Gọi CoreFinance API thật bằng form binding đang dùng và đọc row trực tiếp từ PostgreSQL (VI)
/// </summary>
internal sealed class BackendFinanceClient : IDisposable
{
    private readonly HttpClient _http = ExternalBackend.Client("corefinance");

    public Guid UserId { get; private init; }

    public static async Task<BackendFinanceClient> LoginAsync(string prefix)
    {
        var token = await ExternalBackend.LoginThroughGatewayAsync(ExternalBackend.NewUser(prefix));
        var userId = Guid.Parse(new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token)
            .Claims.Single(c => c.Type == "nameid").Value);
        var client = new BackendFinanceClient { UserId = userId };
        client._http.DefaultRequestHeaders.Authorization = ExternalBackend.Bearer(token);
        return client;
    }

    public Task<HttpResponseMessage> PostFormAsync(string path, IDictionary<string, string> fields) =>
        _http.PostAsync(path, new FormUrlEncodedContent(fields));

    public Task<HttpResponseMessage> PutFormAsync(string path, IDictionary<string, string> fields) =>
        _http.PutAsync(path, new FormUrlEncodedContent(fields));

    public Task<HttpResponseMessage> GetAsync(string path) => _http.GetAsync(path);
    public Task<HttpResponseMessage> DeleteAsync(string path) => _http.DeleteAsync(path);

    public Task<HttpResponseMessage> PostJsonAsync(string path, object body) =>
        _http.PostAsync(path, new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

    public async Task<Guid> CreateAccountAsync(string name)
    {
        var response = await PostFormAsync("/api/Account", new Dictionary<string, string>
        {
            ["Name"] = name, ["Type"] = "Bank", ["Currency"] = "VND", ["InitialBalance"] = "1000000"
        });
        response.EnsureSuccessStatusCode();
        return await IdOf(response);
    }

    public static async Task<Guid> IdOf(HttpResponseMessage response)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    /// <summary>Reads one column set of a transactions row with a new connection (durable read, not a response echo).</summary>
    public static async Task<Dictionary<string, object?>?> ReadTransactionRowAsync(Guid id)
    {
        await using var conn = new NpgsqlConnection(ExternalBackend.Database("TIHOMO_TEST_DB_COREFINANCE"));
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "select account_id, user_id, revenue_amount, spent_amount, balance, transaction_date, description, is_deleted " +
            "from transactions where id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return Enumerable.Range(0, reader.FieldCount)
            .ToDictionary(reader.GetName, i => reader.IsDBNull(i) ? null : reader.GetValue(i));
    }

    public void Dispose() => _http.Dispose();
}
