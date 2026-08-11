using Identity.Application.Services.RefreshTokens;

namespace Identity.Api.Services;

/// <summary>
///     Background service for cleaning up expired refresh tokens
///     Background service để dọn dẹp các refresh token đã hết hạn
/// </summary>
public class RefreshTokenCleanupService(
    IServiceProvider serviceProvider,
    ILogger<RefreshTokenCleanupService> logger)
    : BackgroundService
{
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Clean up every 6 hours

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Refresh token cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
            try
            {
                await CleanupExpiredTokensAsync();
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Refresh token cleanup service is stopping");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in refresh token cleanup service");
                // Wait before retrying
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }

        logger.LogInformation("Refresh token cleanup service stopped");
    }

    private async Task CleanupExpiredTokensAsync()
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var refreshTokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

            var cleanedCount = await refreshTokenService.CleanupExpiredTokensAsync();

            if (cleanedCount > 0)
                logger.LogInformation("Cleaned up {Count} expired refresh tokens", cleanedCount);
            else
                logger.LogDebug("No expired refresh tokens to clean up");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to clean up expired refresh tokens");
            throw;
        }
    }
}