using System.Diagnostics.Metrics;

namespace Ocelot.Gateway.Services;

/// <summary>
/// Service for tracking authentication metrics
/// Dịch vụ theo dõi các metrics xác thực
/// </summary>
public class AuthMetricsService : IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<long> _apiKeyExchangeAttempts;
    private readonly Counter<long> _apiKeyExchangeSuccesses;
    private readonly Counter<long> _apiKeyExchangeFailures;
    private readonly Histogram<double> _apiKeyExchangeDuration;
    private readonly Counter<long> _authenticationRequests;

    public AuthMetricsService()
    {
        _meter = new Meter("TiHoMo.Gateway.Auth");
        
        _apiKeyExchangeAttempts = _meter.CreateCounter<long>(
            "auth_apikey_exchange_attempts_total",
            description: "Total number of API key exchange attempts");
            
        _apiKeyExchangeSuccesses = _meter.CreateCounter<long>(
            "auth_apikey_exchange_successes_total", 
            description: "Total number of successful API key exchanges");
            
        _apiKeyExchangeFailures = _meter.CreateCounter<long>(
            "auth_apikey_exchange_failures_total",
            description: "Total number of failed API key exchanges");
            
        _apiKeyExchangeDuration = _meter.CreateHistogram<double>(
            "auth_apikey_exchange_duration_seconds",
            unit: "seconds",
            description: "Duration of API key exchange operations");
            
        _authenticationRequests = _meter.CreateCounter<long>(
            "auth_requests_total",
            description: "Total number of authentication requests");
    }

    /// <summary>
    /// Record API key exchange attempt
    /// Ghi lại lần thử exchange API key
    /// </summary>
    public void RecordApiKeyExchangeAttempt()
    {
        _apiKeyExchangeAttempts.Add(1);
    }

    /// <summary>
    /// Record successful API key exchange
    /// Ghi lại exchange API key thành công
    /// </summary>
    public void RecordApiKeyExchangeSuccess(double durationSeconds)
    {
        _apiKeyExchangeSuccesses.Add(1);
        _apiKeyExchangeDuration.Record(durationSeconds);
    }

    /// <summary>
    /// Record failed API key exchange
    /// Ghi lại exchange API key thất bại
    /// </summary>
    public void RecordApiKeyExchangeFailure(double durationSeconds, string reason)
    {
        _apiKeyExchangeFailures.Add(1, new KeyValuePair<string, object?>("reason", reason));
        _apiKeyExchangeDuration.Record(durationSeconds);
    }

    /// <summary>
    /// Record authentication request
    /// Ghi lại yêu cầu xác thực
    /// </summary>
    public void RecordAuthenticationRequest(string path, bool hasApiKey)
    {
        _authenticationRequests.Add(1, 
            new KeyValuePair<string, object?>("path", path),
            new KeyValuePair<string, object?>("has_api_key", hasApiKey));
    }

    public void Dispose()
    {
        _meter?.Dispose();
    }
}