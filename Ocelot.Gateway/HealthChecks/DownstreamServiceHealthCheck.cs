using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ocelot.Gateway.HealthChecks;

/// <summary>
///     Health check for downstream services
/// </summary>
public class DownstreamServiceHealthCheck(
    HttpClient httpClient,
    ILogger<DownstreamServiceHealthCheck> logger,
    string serviceName,
    string healthCheckUrl)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Checking health for service: {ServiceName} at {Url}", serviceName, healthCheckUrl);

            using var response = await httpClient.GetAsync(healthCheckUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogDebug("Health check successful for {ServiceName}: {StatusCode}", serviceName,
                    response.StatusCode);

                return HealthCheckResult.Healthy($"{serviceName} is healthy",
                    new Dictionary<string, object>
                    {
                        ["service"] = serviceName,
                        ["status_code"] = (int)response.StatusCode,
                        ["response_time"] = DateTime.UtcNow,
                        ["url"] = healthCheckUrl
                    });
            }

            logger.LogWarning("Health check failed for {ServiceName}: {StatusCode}", serviceName, response.StatusCode);
            return HealthCheckResult.Unhealthy($"{serviceName} returned {response.StatusCode}",
                data: new Dictionary<string, object>
                {
                    ["service"] = serviceName,
                    ["status_code"] = (int)response.StatusCode,
                    ["url"] = healthCheckUrl
                });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error during health check for {ServiceName}", serviceName);
            return HealthCheckResult.Unhealthy($"{serviceName} is unreachable: {ex.Message}",
                ex,
                new Dictionary<string, object>
                {
                    ["service"] = serviceName,
                    ["error"] = ex.Message,
                    ["url"] = healthCheckUrl
                });
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            logger.LogError(ex, "Timeout during health check for {ServiceName}", serviceName);
            return HealthCheckResult.Unhealthy($"{serviceName} health check timed out",
                ex,
                new Dictionary<string, object>
                {
                    ["service"] = serviceName,
                    ["error"] = "Timeout",
                    ["url"] = healthCheckUrl
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during health check for {ServiceName}", serviceName);
            return HealthCheckResult.Unhealthy($"{serviceName} health check failed: {ex.Message}",
                ex,
                new Dictionary<string, object>
                {
                    ["service"] = serviceName,
                    ["error"] = ex.Message,
                    ["url"] = healthCheckUrl
                });
        }
    }
}