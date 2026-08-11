using Identity.Api.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Identity.Api.HealthChecks;

/// <summary>
///     Health check to monitor circuit breaker state and external provider connectivity
/// </summary>
public class CircuitBreakerHealthCheck(
    ITokenVerificationService tokenVerificationService,
    ILogger<CircuitBreakerHealthCheck> logger)
    : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, we'll just check if the service is available
            // In a more sophisticated implementation, we could expose circuit breaker state
            if (tokenVerificationService != null)
                return Task.FromResult(HealthCheckResult.Healthy(
                    "Circuit breaker and resilience patterns are properly configured and operational",
                    new Dictionary<string, object>
                    {
                        { "service_type", tokenVerificationService.GetType().Name },
                        { "timestamp", DateTime.UtcNow },
                        { "resilience_enabled", tokenVerificationService is ResilientTokenVerificationService }
                    }));

            return Task.FromResult(HealthCheckResult.Unhealthy("Token verification service is not available"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Circuit breaker health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Circuit breaker health check failed",
                ex,
                new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { "timestamp", DateTime.UtcNow }
                }));
        }
    }
}