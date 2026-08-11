using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Identity.Api.Services;

/// <summary>
///     Telemetry and metrics service for Identity API observability
///     Quản lý telemetry và metrics cho khả năng quan sát của Identity API
/// </summary>
public class TelemetryService : IDisposable
{
    // Activity source for distributed tracing
    // Nguồn activity cho distributed tracing
    public static readonly ActivitySource ActivitySource = new("Identity.Api");

    // Meter for custom metrics
    // Meter cho custom metrics
    private readonly Meter _meter;
    public readonly ObservableGauge<long> ActiveRequests;
    public readonly Counter<long> CacheHits;
    public readonly Counter<long> CacheMisses;
    public readonly Histogram<double> CacheOperationDuration;
    public readonly Counter<long> CircuitBreakerOpened;

    // Gauges for current state metrics
    // Gauges cho current state metrics
    public readonly ObservableGauge<int> CircuitBreakerState;
    public readonly Histogram<double> ExternalProviderResponseTime;
    public readonly Counter<long> FallbackActivations;
    public readonly Counter<long> RetryAttempts;

    // Counters for business metrics
    // Counters cho business metrics
    public readonly Counter<long> TokenVerificationAttempts;

    // Histograms for timing metrics
    // Histograms cho timing metrics
    public readonly Histogram<double> TokenVerificationDuration;
    public readonly Counter<long> TokenVerificationFailures;
    public readonly Counter<long> TokenVerificationSuccesses;

    private long _activeRequestCount;
    private int _circuitBreakerStateValue; // 0=Closed, 1=Open, 2=HalfOpen

    public TelemetryService()
    {
        _meter = new Meter("Identity.Api.Metrics", "1.0.0");

        // Initialize counters
        // Khởi tạo counters
        TokenVerificationAttempts = _meter.CreateCounter<long>(
            "identity_token_verification_attempts_total",
            description: "Total number of token verification attempts");

        TokenVerificationSuccesses = _meter.CreateCounter<long>(
            "identity_token_verification_successes_total",
            description: "Total number of successful token verifications");

        TokenVerificationFailures = _meter.CreateCounter<long>(
            "identity_token_verification_failures_total",
            description: "Total number of failed token verifications");

        CircuitBreakerOpened = _meter.CreateCounter<long>(
            "identity_circuit_breaker_opened_total",
            description: "Total number of times circuit breaker opened");

        RetryAttempts = _meter.CreateCounter<long>(
            "identity_retry_attempts_total",
            description: "Total number of retry attempts");

        FallbackActivations = _meter.CreateCounter<long>(
            "identity_fallback_activations_total",
            description: "Total number of fallback mechanism activations");

        CacheHits = _meter.CreateCounter<long>(
            "identity_cache_hits_total",
            description: "Total number of cache hits");

        CacheMisses = _meter.CreateCounter<long>(
            "identity_cache_misses_total",
            description: "Total number of cache misses");

        // Initialize histograms
        // Khởi tạo histograms
        TokenVerificationDuration = _meter.CreateHistogram<double>(
            "identity_token_verification_duration_seconds",
            "s",
            "Duration of token verification operations");

        ExternalProviderResponseTime = _meter.CreateHistogram<double>(
            "identity_external_provider_response_duration_seconds",
            "s",
            "Response time from external authentication providers");

        CacheOperationDuration = _meter.CreateHistogram<double>(
            "identity_cache_operation_duration_seconds",
            "s",
            "Duration of cache operations");

        // Initialize observable gauges
        // Khởi tạo observable gauges
        CircuitBreakerState = _meter.CreateObservableGauge(
            "identity_circuit_breaker_state",
            () => _circuitBreakerStateValue,
            description: "Current circuit breaker state (0=Closed, 1=Open, 2=HalfOpen)");

        ActiveRequests = _meter.CreateObservableGauge(
            "identity_active_requests_current",
            () => _activeRequestCount,
            description: "Current number of active requests");
    }

    public void Dispose()
    {
        _meter?.Dispose();
        ActivitySource?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Increment active request count
    ///     Tăng số lượng request đang active
    /// </summary>
    public void IncrementActiveRequests()
    {
        Interlocked.Increment(ref _activeRequestCount);
    }

    /// <summary>
    ///     Decrement active request count
    ///     Giảm số lượng request đang active
    /// </summary>
    public void DecrementActiveRequests()
    {
        Interlocked.Decrement(ref _activeRequestCount);
    }

    /// <summary>
    ///     Update circuit breaker state
    ///     Cập nhật trạng thái circuit breaker
    /// </summary>
    /// <param name="state">Circuit breaker state (0=Closed, 1=Open, 2=HalfOpen)</param>
    public void UpdateCircuitBreakerState(int state)
    {
        _circuitBreakerStateValue = state;
    }

    /// <summary>
    ///     Record token verification attempt with timing
    ///     Ghi lại attempt token verification với timing
    /// </summary>
    /// <param name="provider">Authentication provider name</param>
    /// <param name="success">Whether verification was successful</param>
    /// <param name="duration">Duration of the operation</param>
    /// <param name="tags">Additional tags for metrics</param>
    public void RecordTokenVerification(string provider, bool success, TimeSpan duration,
        params KeyValuePair<string, object?>[] tags)
    {
        var allTags = new List<KeyValuePair<string, object?>>
        {
            new("provider", provider),
            new("success", success)
        };
        allTags.AddRange(tags);

        TokenVerificationAttempts.Add(1, [.. allTags]);

        if (success)
            TokenVerificationSuccesses.Add(1, [.. allTags]);
        else
            TokenVerificationFailures.Add(1, [.. allTags]);

        TokenVerificationDuration.Record(duration.TotalSeconds, [.. allTags]);
    }

    /// <summary>
    ///     Record external provider response time
    ///     Ghi lại response time từ external provider
    /// </summary>
    public void RecordExternalProviderResponseTime(string provider, TimeSpan duration, bool success)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("provider", provider),
            new("success", success)
        };

        ExternalProviderResponseTime.Record(duration.TotalSeconds, tags);
    }

    /// <summary>
    ///     Record cache operation
    ///     Ghi lại cache operation
    /// </summary>
    public void RecordCacheOperation(string operation, bool hit, TimeSpan duration)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("operation", operation),
            new("cache_layer", "memory") // or "redis"
        };

        if (hit)
            CacheHits.Add(1, tags);
        else
            CacheMisses.Add(1, tags);

        CacheOperationDuration.Record(duration.TotalSeconds, tags);
    }

    /// <summary>
    ///     Record circuit breaker event
    ///     Ghi lại circuit breaker event
    /// </summary>
    public void RecordCircuitBreakerEvent(string eventType, string provider)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("event_type", eventType),
            new("provider", provider)
        };

        if (eventType == "opened")
        {
            CircuitBreakerOpened.Add(1, tags);
            UpdateCircuitBreakerState(1); // Open
        }
        else if (eventType == "closed")
        {
            UpdateCircuitBreakerState(0); // Closed
        }
        else if (eventType == "half_open")
        {
            UpdateCircuitBreakerState(2); // HalfOpen
        }
    }

    /// <summary>
    ///     Record retry attempt
    ///     Ghi lại retry attempt
    /// </summary>
    public void RecordRetryAttempt(string provider, int attemptNumber, string reason)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("provider", provider),
            new("attempt_number", attemptNumber),
            new("reason", reason)
        };

        RetryAttempts.Add(1, tags);
    }

    /// <summary>
    ///     Record fallback activation
    ///     Ghi lại fallback activation
    /// </summary>
    public void RecordFallbackActivation(string provider, string fallbackType, string reason)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("provider", provider),
            new("fallback_type", fallbackType),
            new("reason", reason)
        };

        FallbackActivations.Add(1, tags);
    }
}