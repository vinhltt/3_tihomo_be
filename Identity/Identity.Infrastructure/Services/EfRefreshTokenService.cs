using Identity.Application.Services.RefreshTokens;
using Identity.Domain.Entities;
using Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Services;

/// <summary>
///     EF Core implementation of refresh token service
///     Triển khai EF Core cho service refresh token
/// </summary>
public class EfRefreshTokenService(
    IdentityDbContext context,
    ILogger<RefreshTokenService> logger)
    : RefreshTokenService(logger)
{
    /// <summary>
    ///     Generate new refresh token for user
    ///     Tạo refresh token mới cho user
    /// </summary>
    public override async Task<string> GenerateRefreshTokenAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate cryptographically secure random token
            // Tạo token ngẫu nhiên an toàn mật mã
            var tokenString = GenerateSecureToken();

            // Create refresh token entity
            // Tạo entity refresh token
            var refreshToken = new RefreshToken
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Token = tokenString,
                ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save to database
            // Lưu vào database
            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Generated refresh token for user {UserId}", userId);

            return tokenString;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to generate refresh token for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    ///     Validate and get user from refresh token
    ///     Validate và lấy thông tin user từ refresh token
    /// </summary>
    public override async Task<Guid?> ValidateRefreshTokenAsync(string refreshToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await context.RefreshTokens
                .Where(rt => rt.Token == refreshToken
                             && !rt.IsRevoked
                             && rt.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync(cancellationToken);

            if (token == null)
            {
                Logger.LogWarning("Invalid or expired refresh token provided");
                return null;
            }

            Logger.LogInformation("Validated refresh token for user {UserId}", token.UserId);
            return token.UserId;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to validate refresh token");
            return null;
        }
    }

    /// <summary>
    ///     Revoke refresh token
    ///     Thu hồi refresh token
    /// </summary>
    public override async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string revokedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await context.RefreshTokens
                .Where(rt => rt.Token == refreshToken && !rt.IsRevoked)
                .FirstOrDefaultAsync(cancellationToken);

            if (token == null)
            {
                Logger.LogWarning("Attempted to revoke non-existent or already revoked refresh token");
                return false;
            }

            // Mark as revoked
            // Đánh dấu là đã thu hồi
            token.IsRevoked = true;
            token.RevokedBy = revokedBy;
            token.RevokedAt = DateTime.UtcNow;
            token.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Revoked refresh token for user {UserId} by {RevokedBy}", token.UserId, revokedBy);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to revoke refresh token");
            return false;
        }
    }

    /// <summary>
    ///     Rotate refresh token (revoke old and generate new)
    ///     Xoay vòng refresh token (thu hồi cũ và tạo mới)
    /// </summary>
    public override async Task<string?> RotateRefreshTokenAsync(string oldRefreshToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate old token and get user ID
            // Validate token cũ và lấy user ID
            var userId = await ValidateRefreshTokenAsync(oldRefreshToken, cancellationToken);
            if (!userId.HasValue)
            {
                Logger.LogWarning("Cannot rotate invalid refresh token");
                return null;
            }

            // Use transaction to ensure atomicity
            // Sử dụng transaction để đảm bảo tính nguyên tử
            using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Revoke old token
                // Thu hồi token cũ
                var revokeSuccess =
                    await RevokeRefreshTokenAsync(oldRefreshToken, "system_rotation", cancellationToken);
                if (!revokeSuccess)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return null;
                }

                // Generate new token
                // Tạo token mới
                var newToken = await GenerateRefreshTokenAsync(userId.Value, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                Logger.LogInformation("Successfully rotated refresh token for user {UserId}", userId.Value);
                return newToken;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to rotate refresh token");
            return null;
        }
    }

    /// <summary>
    ///     Revoke all refresh tokens for user
    ///     Thu hồi tất cả refresh token của user
    /// </summary>
    public override async Task<int> RevokeAllUserTokensAsync(Guid userId, string revokedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var activeTokens = await context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync(cancellationToken);

            if (activeTokens.Count == 0)
            {
                Logger.LogInformation("No active refresh tokens found for user {UserId}", userId);
                return 0;
            }

            // Revoke all active tokens
            // Thu hồi tất cả token đang hoạt động
            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedBy = revokedBy;
                token.RevokedAt = DateTime.UtcNow;
                token.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Revoked {Count} refresh tokens for user {UserId} by {RevokedBy}",
                activeTokens.Count, userId, revokedBy);

            return activeTokens.Count;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to revoke all refresh tokens for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    ///     Clean up expired refresh tokens
    ///     Dọn dẹp các refresh token đã hết hạn
    /// </summary>
    public override async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var expiredTokens = await context.RefreshTokens
                .Where(rt => rt.ExpiresAt <= DateTime.UtcNow || rt.IsRevoked)
                .ToListAsync(cancellationToken);

            if (expiredTokens.Count == 0)
            {
                Logger.LogInformation("No expired refresh tokens to clean up");
                return 0;
            }

            // Remove expired/revoked tokens from database
            // Xóa các token hết hạn/đã thu hồi khỏi database
            context.RefreshTokens.RemoveRange(expiredTokens);
            await context.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Cleaned up {Count} expired refresh tokens", expiredTokens.Count);
            return expiredTokens.Count;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to clean up expired refresh tokens");
            throw;
        }
    }
}