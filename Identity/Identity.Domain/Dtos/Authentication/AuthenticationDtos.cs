namespace Identity.Domain.Dtos.Authentication;

public record LoginRequest(string Username, string Password);

public record GoogleLoginRequest(string IdToken);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserProfile User);

public record RefreshTokenRequest(string RefreshToken);

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);

public record LogoutRequest(string RefreshToken);

public record ApiKeyVerificationResponse(
    bool IsValid,
    Guid? UserId,
    List<string> Scopes,
    UserProfile? User);

public record UserProfile(
    Guid Id,
    string Email,
    string Username,
    string FullName,
    string? AvatarUrl,
    List<string> Roles);

public record GoogleUserInfo(
    string GoogleId,
    string Email,
    string FullName,
    string? AvatarUrl,
    bool IsEmailVerified);