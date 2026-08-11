using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Services.RefreshTokens;

/// <summary>
///     Service for managing refresh tokens
///     Service quản lý refresh token
/// </summary>
public abstract class RefreshTokenService(ILogger<RefreshTokenService> logger) : IRefreshTokenService
{
    protected readonly ILogger<RefreshTokenService> Logger = logger;
    protected readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30); // 30 days

    /// <summary>
    ///     Generate new refresh token for user
    ///     Tạo refresh token mới cho user
    /// </summary>
    public abstract Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Validate and get user from refresh token
    ///     Validate và lấy thông tin user từ refresh token
    /// </summary>
    public abstract Task<Guid?> ValidateRefreshTokenAsync(string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Revoke refresh token
    ///     Thu hồi refresh token
    /// </summary>
    public abstract Task<bool> RevokeRefreshTokenAsync(string refreshToken, string revokedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotate refresh token (revoke old and generate new)
    ///     Xoay vòng refresh token (thu hồi cũ và tạo mới)
    /// </summary>
    public abstract Task<string?> RotateRefreshTokenAsync(string oldRefreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Revoke all refresh tokens for user
    ///     Thu hồi tất cả refresh token của user
    /// </summary>
    public abstract Task<int> RevokeAllUserTokensAsync(Guid userId, string revokedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Clean up expired refresh tokens
    ///     Dọn dẹp các refresh token đã hết hạn
    /// </summary>
    public abstract Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Generate cryptographically secure random token
    ///     Tạo token ngẫu nhiên an toàn mật mã
    /// </summary>
    protected static string GenerateSecureToken()
    {
        var tokenBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes);
    }
}