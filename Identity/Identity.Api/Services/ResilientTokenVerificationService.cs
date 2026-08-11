using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.CircuitBreaker;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Polly.Timeout;

namespace Identity.Api.Services;

/// <summary>
///     Resilient token verification service with circuit breaker, retry, timeout, and fallback patterns
///     Wraps EnhancedTokenVerificationService with Polly resilience policies
///     Service xác minh token có tính bền vững với circuit breaker, retry, timeout và fallback patterns
/// </summary>
public class ResilientTokenVerificationService : ITokenVerificationService
{
    // Circuit breaker state tracking
    private static readonly ActivitySource ActivitySource = new("Identity.TokenVerification.Resilience");
    private readonly IConfiguration _configuration;
    private readonly IDistributedCache _distributedCache;
    private readonly ITokenVerificationService _enhancedService;
    private readonly ILogger<ResilientTokenVerificationService> _logger;
    private readonly IMemoryCache _memoryCache;

    // Polly policies for resilience
    private readonly ResiliencePipeline<SocialUserInfo?> _providerApiPipeline;
    private readonly TelemetryService _telemetryService;

    public ResilientTokenVerificationService(
        ITokenVerificationService enhancedService,
        ILogger<ResilientTokenVerificationService> logger,
        IConfiguration configuration,
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        TelemetryService telemetryService)
    {
        _enhancedService = enhancedService;
        _logger = logger;
        _configuration = configuration;
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _telemetryService = telemetryService;

        // Build resilience pipeline for provider API calls
        _providerApiPipeline = BuildProviderApiPipeline();
    }

    /// <summary>
    ///     Verify token using resilient patterns with fallback to enhanced service
    ///     Xác minh token bằng resilient patterns với fallback về enhanced service
    /// </summary>
    public async Task<SocialUserInfo?> VerifyTokenAsync(string provider, string token)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var activity = ActivitySource.StartActivity("VerifyTokenResilient");
            activity?.SetTag("provider", provider);

            // Execute with resilience pipeline
            var result = await _providerApiPipeline.ExecuteAsync(async cancellationToken =>
            {
                return await _enhancedService.VerifyTokenAsync(provider, token);
            });

            stopwatch.Stop();

            if (result != null)
            {
                activity?.SetTag("verification.result", "success");
                activity?.SetTag("verification.method", "resilient");

                // Record successful verification
                // Ghi lại xác minh thành công
                _telemetryService.RecordTokenVerification(provider, true, stopwatch.Elapsed);

                return result;
            }

            activity?.SetTag("verification.result", "failed");
            _telemetryService.RecordTokenVerification(provider, false, stopwatch.Elapsed);
            return null;
        }
        catch (BrokenCircuitException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("Provider API circuit breaker is open for {Provider}, using fallback: {Message}",
                provider, ex.Message);

            // Record circuit breaker activation
            // Ghi lại kích hoạt circuit breaker
            _telemetryService.RecordCircuitBreakerEvent(provider, "opened");
            _telemetryService.RecordTokenVerification(provider, false, stopwatch.Elapsed,
                new KeyValuePair<string, object?>("fallback_reason", "circuit_breaker"));

            return await TryFallbackVerification(provider, token);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Critical error in resilient token verification for provider {Provider}", provider);

            // Record verification error
            // Ghi lại lỗi xác minh
            _telemetryService.RecordTokenVerification(provider, false, stopwatch.Elapsed,
                new KeyValuePair<string, object?>("error_type", ex.GetType().Name));

            return await TryFallbackVerification(provider, token);
        }
    }

    /// <summary>
    ///     Verify Google token with resilience patterns
    /// </summary>
    public async Task<SocialUserInfo?> VerifyGoogleTokenAsync(string token)
    {
        return await VerifyTokenAsync("google", token);
    }

    /// <summary>
    ///     Verify Facebook token with resilience patterns
    /// </summary>
    public async Task<SocialUserInfo?> VerifyFacebookTokenAsync(string token)
    {
        return await VerifyTokenAsync("facebook", token);
    }

    /// <summary>
    ///     Fallback verification when circuit breaker is open or errors occur
    /// </summary>
    private async Task<SocialUserInfo?> TryFallbackVerification(string provider, string token)
    {
        try
        {
            // Try to get from cache as fallback
            var cacheKey = $"token_fallback:{provider}:{ComputeHash(token)}";
            var cachedResult = await _distributedCache.GetStringAsync(cacheKey);

            if (cachedResult != null)
            {
                _logger.LogInformation("Using cached {Provider} token validation result during fallback", provider);

                var fallbackResult = JsonSerializer.Deserialize<SocialUserInfo>(cachedResult);
                if (fallbackResult != null)
                {
                    // Mark as fallback result
                    fallbackResult.Provider = $"{provider}_fallback";
                    return fallbackResult;
                }
            }

            // Last resort: try local JWT parsing for basic info
            if (provider.Equals("google", StringComparison.CurrentCultureIgnoreCase)) return TryLocalGoogleTokenParsing(token);

            _logger.LogWarning("No fallback available for {Provider} token verification", provider);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallback verification failed for provider {Provider}", provider);
            return null;
        }
    }

    /// <summary>
    ///     Try local Google JWT parsing as ultimate fallback
    /// </summary>
    private SocialUserInfo? TryLocalGoogleTokenParsing(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo < DateTime.UtcNow)
            {
                _logger.LogWarning("Token expired in local parsing fallback");
                return null;
            }

            // Extract basic claims from JWT
            var userId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var picture = jwt.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation("Successfully parsed Google token locally as fallback");
                return new SocialUserInfo
                {
                    Id = userId,
                    Email = email ?? "",
                    Name = name ?? "",
                    PictureUrl = picture,
                    Provider = "google_local_fallback"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Local Google token parsing failed");
        }

        return null;
    }

    /// <summary>
    ///     Compute secure hash for cache key from token
    /// </summary>
    private static string ComputeHash(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    ///     Build comprehensive resilience pipeline for provider API calls using Polly v8
    /// </summary>
    private ResiliencePipeline<SocialUserInfo?> BuildProviderApiPipeline()
    {
        var builder = new ResiliencePipelineBuilder<SocialUserInfo?>(); // Add retry strategy with exponential backoff
        builder.AddRetry(new RetryStrategyOptions<SocialUserInfo?>
        {
            ShouldHandle = new PredicateBuilder<SocialUserInfo?>()
                .Handle<HttpRequestException>()
                .Handle<TaskCanceledException>()
                .Handle<TimeoutRejectedException>(),
            MaxRetryAttempts = 3,
            DelayGenerator = args =>
            {
                var delays = Backoff.DecorrelatedJitterBackoffV2(
                    TimeSpan.FromMilliseconds(200),
                    3).ToArray();

                var delay = args.AttemptNumber < delays.Length
                    ? delays[args.AttemptNumber]
                    : TimeSpan.FromSeconds(5);

                return ValueTask.FromResult((TimeSpan?)delay);
            },
            OnRetry = args =>
            {
                _logger.LogWarning(
                    "Provider API retry {RetryCount}/{MaxRetries} after {Delay}ms. Exception: {Exception}",
                    args.AttemptNumber + 1, 3,
                    args.RetryDelay.TotalMilliseconds,
                    args.Outcome.Exception?.Message);
                return ValueTask.CompletedTask;
            }
        });

        // Add circuit breaker
        builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<SocialUserInfo?>
        {
            ShouldHandle = new PredicateBuilder<SocialUserInfo?>()
                .Handle<HttpRequestException>()
                .Handle<TaskCanceledException>()
                .Handle<TimeoutRejectedException>(),
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(30),
            OnOpened = args =>
            {
                _logger.LogError(
                    "Provider API circuit breaker opened for {Duration}s. Exception: {Exception}",
                    args.BreakDuration.TotalSeconds, args.Outcome.Exception?.Message);
                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                _logger.LogInformation("Provider API circuit breaker closed - service recovered");
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = args =>
            {
                _logger.LogInformation("Provider API circuit breaker half-opened - testing service");
                return ValueTask.CompletedTask;
            }
        }); // Add timeout
        builder.AddTimeout(new TimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(10),
            OnTimeout = args =>
            {
                _logger.LogWarning("Provider API call timed out after {Timeout}s",
                    args.Timeout.TotalSeconds);
                return ValueTask.CompletedTask;
            }
        });

        return builder.Build();
    }
}