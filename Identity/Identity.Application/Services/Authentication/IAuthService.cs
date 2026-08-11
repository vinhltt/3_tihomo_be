using Identity.Domain.Dtos.Authentication;

namespace Identity.Application.Services.Authentication;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default);

    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
    Task<ApiKeyVerificationResponse> VerifyApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
}