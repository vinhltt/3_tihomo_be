using System.Globalization;
using CoreFinance.Domain.Enums;
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
    private readonly HttpClient _http;

    public Guid UserId { get; private init; }

    private BackendFinanceClient(HttpClient http, Guid userId)
    {
        _http = http;
        UserId = userId;
    }

    public static async Task<BackendFinanceClient> LoginAsync(string prefix)
    {
        var token = await ExternalBackend.LoginThroughGatewayAsync(ExternalBackend.NewUser(prefix));
        return Authenticate(ExternalBackend.Client("corefinance"), token);
    }

    internal static BackendFinanceClient Authenticate(HttpClient http, string token)
    {
        var userId = Guid.Parse(new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token)
            .Claims.Single(c => c.Type == "nameid").Value);
        http.DefaultRequestHeaders.Authorization = ExternalBackend.Bearer(token);
        return new BackendFinanceClient(http, userId);
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

    public async Task<Guid> CreateExpectedAsync(DateTime expectedDate, decimal amount,
        RecurringTransactionType type = RecurringTransactionType.Expense, Guid? accountId = null,
        Guid? templateId = null, string? description = null, string? category = null)
    {
        var fields = new Dictionary<string, string>
        {
            ["ExpectedDate"] = expectedDate.ToString("O", CultureInfo.InvariantCulture),
            ["ExpectedAmount"] = amount.ToString(CultureInfo.InvariantCulture),
            ["TransactionType"] = type.ToString(),
            ["Status"] = ExpectedTransactionStatus.Pending.ToString()
        };
        if (accountId.HasValue) fields["AccountId"] = accountId.Value.ToString();
        if (templateId.HasValue) fields["RecurringTransactionTemplateId"] = templateId.Value.ToString();
        if (description != null) fields["Description"] = description;
        if (category != null) fields["Category"] = category;
        using var response = await PostFormAsync("/api/ExpectedTransaction", fields);
        response.EnsureSuccessStatusCode();
        return await IdOf(response);
    }

    public Task<HttpResponseMessage> RecordPaymentAsync(Guid id, Guid? accountId = null) =>
        PostJsonAsync($"/api/ExpectedTransaction/{id}/record-payment", new { accountId });

    public Task<HttpResponseMessage> ConfirmExpectedAsync(Guid id, Guid actualTransactionId) =>
        PostJsonAsync($"/api/ExpectedTransaction/{id}/confirm", new { actualTransactionId });

    public Task<HttpResponseMessage> CancelExpectedAsync(Guid id, string reason) =>
        PostJsonAsync($"/api/ExpectedTransaction/{id}/cancel", new { reason });

    public Task<HttpResponseMessage> AdjustExpectedAsync(Guid id, decimal newAmount, string reason) =>
        PostJsonAsync($"/api/ExpectedTransaction/{id}/adjust", new { newAmount, reason });

    public Task<HttpResponseMessage> PutExpectedAsync(Guid id, IDictionary<string, string> fields)
    {
        var form = new Dictionary<string, string>(fields) { ["Id"] = id.ToString() };
        return PutFormAsync($"/api/ExpectedTransaction/{id}", form);
    }

    public static async Task<Guid> IdOf(HttpResponseMessage response)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    /// <summary>Reads one column set of a transactions row with a new connection (durable read, not a response echo).</summary>
    public static Task<Dictionary<string, object?>?> ReadTransactionRowAsync(Guid id) =>
        ReadTransactionRowAsync(id, ExternalBackend.Database("TIHOMO_TEST_DB_COREFINANCE"));

    public static async Task<Dictionary<string, object?>?> ReadTransactionRowAsync(Guid id, string connectionString)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "select account_id, user_id, revenue_amount, spent_amount, balance, transaction_date, description, is_deleted, " +
            "category_type, category_summary from transactions where id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return Enumerable.Range(0, reader.FieldCount)
            .ToDictionary(reader.GetName, i => reader.IsDBNull(i) ? null : reader.GetValue(i));
    }

    public void Dispose() => _http.Dispose();
}
