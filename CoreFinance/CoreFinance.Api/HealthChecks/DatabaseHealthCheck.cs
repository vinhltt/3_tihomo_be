using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using CoreFinance.Infrastructure;

namespace CoreFinance.Api.HealthChecks;

/// <summary>
/// Custom database health check with enhanced error reporting and timeout handling. (EN)<br/>
/// Kiểm tra sức khỏe cơ sở dữ liệu tùy chỉnh với báo cáo lỗi và xử lý timeout cải tiến. (VI)
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly CoreFinanceDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(CoreFinanceDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Starting database health check");

            // Create a timeout token for the health check
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(10)); // 10 second timeout

            // Test database connectivity with a simple query
            await _context.Database.ExecuteSqlRawAsync("SELECT 1", timeoutCts.Token);
            
            _logger.LogDebug("Database health check passed");
            
            return HealthCheckResult.Healthy("Database connection is healthy", new Dictionary<string, object>
            {
                ["database"] = _context.Database.GetDbConnection().Database,
                ["provider"] = _context.Database.ProviderName ?? "Unknown"
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Database health check was cancelled");
            return HealthCheckResult.Degraded("Database health check was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed: {ErrorMessage}", ex.Message);
            
            return HealthCheckResult.Unhealthy("Database connection failed", ex, new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["errorType"] = ex.GetType().Name,
                ["connectionString"] = MaskConnectionString(_context.Database.GetDbConnection().ConnectionString)
            });
        }
    }

    private static string MaskConnectionString(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "Not available";

        try
        {
            // Simple masking - replace password value with ***
            if (connectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase))
            {
                var parts = connectionString.Split(';');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Trim().StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                    {
                        parts[i] = "Password=***";
                        break;
                    }
                }
                return string.Join(";", parts);
            }
            return connectionString;
        }
        catch
        {
            return "Connection string masking failed";
        }
    }
}