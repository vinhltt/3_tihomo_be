namespace Identity.Application.Services.RefreshTokens;

/// <summary>
///     Interface for refresh token operations
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    ///     Generate new refresh token for user
    ///     Tạo refresh token mới cho user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated refresh token</returns>
    Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Validate and get user from refresh token
    ///     Validate và lấy thông tin user từ refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User ID if token is valid, null otherwise</returns>
    Task<Guid?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Revoke refresh token
    ///     Thu hồi refresh token
    /// </summary>
    /// <param name="refreshToken">Token to revoke</param>
    /// <param name="revokedBy">Who revoked the token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if revoked successfully</returns>
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, string revokedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotate refresh token (revoke old and generate new)
    ///     Xoay vòng refresh token (thu hồi cũ và tạo mới)
    /// </summary>
    /// <param name="oldRefreshToken">Old refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New refresh token if successful, null otherwise</returns>
    Task<string?> RotateRefreshTokenAsync(string oldRefreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Revoke all refresh tokens for user
    ///     Thu hồi tất cả refresh token của user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="revokedBy">Who revoked the tokens</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tokens revoked</returns>
    Task<int> RevokeAllUserTokensAsync(Guid userId, string revokedBy, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Clean up expired refresh tokens
    ///     Dọn dẹp các refresh token đã hết hạn
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tokens cleaned up</returns>
    Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}