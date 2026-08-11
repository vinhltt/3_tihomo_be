namespace Identity.Domain.Dtos.RefreshTokens;

/// <summary>
///     Request to refresh access token
///     Yêu cầu làm mới access token
/// </summary>
public record RefreshTokenRequest
{
    /// <summary>
    ///     Refresh token to validate and rotate
    ///     Refresh token để validate và xoay vòng
    /// </summary>
    public required string RefreshToken { get; init; }
}

/// <summary>
///     Response with new tokens after refresh
///     Phản hồi với token mới sau khi làm mới
/// </summary>
public record RefreshTokenResponse
{
    /// <summary>
    ///     New JWT access token
    ///     JWT access token mới
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    ///     New refresh token (rotated)
    ///     Refresh token mới (đã xoay vòng)
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    ///     Token type (Bearer)
    ///     Loại token (Bearer)
    /// </summary>
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    ///     Access token expiration time in seconds
    ///     Thời gian hết hạn access token tính bằng giây
    /// </summary>
    public int ExpiresIn { get; init; }
}

/// <summary>
///     Request to revoke refresh token
///     Yêu cầu thu hồi refresh token
/// </summary>
public record RevokeTokenRequest
{
    /// <summary>
    ///     Refresh token to revoke
    ///     Refresh token cần thu hồi
    /// </summary>
    public required string RefreshToken { get; init; }
}

/// <summary>
///     Request to revoke all refresh tokens for a user
///     Yêu cầu thu hồi tất cả refresh token của user
/// </summary>
public record RevokeAllTokensRequest
{
    /// <summary>
    ///     User ID to revoke all tokens for
    ///     User ID để thu hồi tất cả tokens
    /// </summary>
    public required Guid UserId { get; init; }
}