using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Ocelot.Gateway.Configuration;

namespace Ocelot.Gateway.Middleware;

/// <summary>
/// Middleware to handle health routes before Ocelot routing. (EN)<br/>
/// Middleware xử lý health routes trước khi Ocelot routing. (VI)
/// </summary>
public class HealthRouteMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HealthRouteMiddleware> _logger;
    private readonly HealthCheckService _healthCheckService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ServicePorts _servicePorts;
    private readonly IConfiguration _configuration;

    public HealthRouteMiddleware(
        RequestDelegate next, 
        ILogger<HealthRouteMiddleware> logger,
        HealthCheckService healthCheckService,
        IHttpClientFactory httpClientFactory,
        IOptions<ServicePorts> servicePorts,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _healthCheckService = healthCheckService;
        _httpClientFactory = httpClientFactory;
        _servicePorts = servicePorts.Value;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();

        if (path == "/health")
        {
            await HandleGatewayHealthAsync(context);
            return;
        }

        if (path == "/health/all")
        {
            await HandleAggregatedHealthAsync(context);
            return;
        }

        // Continue to next middleware (Ocelot)
        await _next(context);
    }

    private async Task HandleGatewayHealthAsync(HttpContext context)
    {
        try
        {
            var report = await _healthCheckService.CheckHealthAsync();
            
            var response = new
            {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration.TotalMilliseconds,
                Results = report.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    entry.Value.Description,
                    Duration = entry.Value.Duration.TotalMilliseconds,
                    entry.Value.Data
                }),
                Timestamp = DateTime.UtcNow
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = report.Status == HealthStatus.Healthy ? 200 : 503;
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check gateway health");
            context.Response.StatusCode = 503;
            await context.Response.WriteAsync("Unhealthy");
        }
    }

    private async Task HandleAggregatedHealthAsync(HttpContext context)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            // Get service URLs based on environment
            var (identityUrl, coreFinanceUrl, excelUrl) = GetServiceUrls();

            var services = new Dictionary<string, object>
            {
                ["gateway"] = await GetGatewayHealthAsync(),
                ["identity"] = await GetServiceHealthAsync(client, identityUrl),
                ["corefinance"] = await GetServiceHealthAsync(client, coreFinanceUrl),
                ["excel"] = await GetServiceHealthAsync(client, excelUrl)
            };

            var overallStatus = services.Values.All(s => IsHealthy(s)) ? "Healthy" : "Unhealthy";

            var result = new
            {
                Status = overallStatus,
                Timestamp = DateTime.UtcNow,
                Services = services
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = overallStatus == "Healthy" ? 200 : 503;
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check aggregated health");
            context.Response.StatusCode = 503;
            await context.Response.WriteAsync("Unhealthy");
        }
    }

    private async Task<object> GetGatewayHealthAsync()
    {
        try
        {
            var report = await _healthCheckService.CheckHealthAsync();
            return new
            {
                Status = report.Status.ToString(),
                Description = "Gateway health check",
                Duration = report.TotalDuration.TotalMilliseconds,
                Data = new { ServiceType = "Gateway", CheckCount = report.Entries.Count }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway health check failed");
            return new
            {
                Status = "Unhealthy",
                Description = ex.Message,
                Duration = 0.0,
                Data = new { Error = ex.GetType().Name }
            };
        }
    }

    private async Task<object> GetServiceHealthAsync(HttpClient client, string healthUrl)
    {
        try
        {
            var response = await client.GetAsync(healthUrl);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Try to parse as JSON
                try
                {
                    var healthData = JsonSerializer.Deserialize<JsonElement>(content);
                    return new
                    {
                        Status = healthData.GetProperty("Status").GetString(),
                        Description = $"Service responded with {response.StatusCode}",
                        Duration = 0.0,
                        Data = healthData.ValueKind == JsonValueKind.Object ? (object)healthData : null
                    };
                }
                catch
                {
                    // If not JSON, treat as simple text
                    return new
                    {
                        Status = content.Trim(),
                        Description = $"Service responded with {response.StatusCode}",
                        Duration = 0.0,
                        Data = new { ResponseText = content }
                    };
                }
            }
            else
            {
                return new
                {
                    Status = "Unhealthy",
                    Description = $"Service returned {response.StatusCode}",
                    Duration = 0.0,
                    Data = new { StatusCode = (int)response.StatusCode, Content = content }
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check health for {HealthUrl}", healthUrl);
            return new
            {
                Status = "Unhealthy",
                Description = ex.Message,
                Duration = 0.0,
                Data = new { Error = ex.GetType().Name, HealthUrl = healthUrl }
            };
        }
    }

    private (string identityUrl, string coreFinanceUrl, string excelUrl) GetServiceUrls()
    {
        var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
        
        if (environment.Equals("Local", StringComparison.OrdinalIgnoreCase))
        {
            // Local development - use localhost with configured ports
            var identityPort = _servicePorts.IdentitySsoPort ?? 5801;
            var coreFinancePort = _servicePorts.CoreFinanceApiPort ?? 5802;
            var excelPort = _servicePorts.ExcelApiPort ?? 5805;

            return (
                $"http://localhost:{identityPort}/health",
                $"http://localhost:{coreFinancePort}/health",
                $"http://localhost:{excelPort}/health"
            );
        }
        else
        {
            // Docker environment - use service names
            return (
                "http://identity-api:8080/health",
                "http://corefinance-api:8080/health", 
                "http://excel-api:8080/health"
            );
        }
    }

    private static bool IsHealthy(object serviceStatus)
    {
        if (serviceStatus == null) return false;

        var statusProperty = serviceStatus.GetType().GetProperty("Status");
        if (statusProperty?.GetValue(serviceStatus) is string status)
        {
            return string.Equals(status, "Healthy", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}