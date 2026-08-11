using Identity.Application.Common.Interfaces;
using Identity.Domain.Dtos.Authentication;
using Identity.Domain.Entities;
using Identity.Domain.Repositories;

namespace Identity.Application.Services.Authentication;

/// <summary>
///     Service for authentication operations including login, token management, and API key validation (EN)<br />
///     Dịch vụ cho các thao tác xác thực bao gồm đăng nhập, quản lý token và xác thực khóa API (VI)
/// </summary>
/// <param name="userRepository">
///     Repository for user data access (EN)<br />
///     Repository để truy cập dữ liệu người dùng (VI)
/// </param>
/// <param name="refreshTokenRepository">
///     Repository for refresh token data access (EN)<br />
///     Repository để truy cập dữ liệu refresh token (VI)
/// </param>
/// <param name="passwordHasher">
///     Service for password hashing and verification (EN)<br />
///     Dịch vụ để băm và xác minh mật khẩu (VI)
/// </param>
/// <param name="jwtTokenService">
///     Service for JWT token generation and validation (EN)<br />
///     Dịch vụ để tạo và xác thực JWT token (VI)
/// </param>
/// <param name="googleAuthService">
///     Service for Google OAuth authentication (EN)<br />
///     Dịch vụ cho xác thực Google OAuth (VI)
/// </param>
/// <param name="apiKeyRepository">
///     Repository for API key data access (EN)<br />
///     Repository để truy cập dữ liệu khóa API (VI)
/// </param>
/// <param name="apiKeyHasher">
///     Service for API key hashing and verification (EN)<br />
///     Dịch vụ để băm và xác minh khóa API (VI)
/// </param>
public class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IGoogleAuthService googleAuthService,
    IApiKeyRepository apiKeyRepository,
    IApiKeyHasher apiKeyHasher) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByUsernameOrEmailAsync(request.Username, cancellationToken);

        if (user == null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.IsActive) throw new UnauthorizedAccessException("Account is disabled");

        // Update last login
        await userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow, cancellationToken);

        return await GenerateLoginResponseAsync(user, cancellationToken);
    }

    public async Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var googleUser = await googleAuthService.VerifyGoogleTokenAsync(request.IdToken);

        var user = await userRepository.GetByGoogleIdAsync(googleUser.GoogleId, cancellationToken);

        if (user == null)
        {
            // Check if user exists with same email
            user = await userRepository.GetByEmailAsync(googleUser.Email, cancellationToken);
            if (user != null)
            {
                // Link Google account to existing user
                user.GoogleId = googleUser.GoogleId;
                user.AvatarUrl = googleUser.AvatarUrl;
                user.EmailConfirmed = googleUser.IsEmailVerified; // Set based on Google verification
                await userRepository.UpdateAsync(user, cancellationToken);
            }
            else
            {
                // Create new user
                user = new User
                {
                    Id = Guid.CreateVersion7(),
                    Email = googleUser.Email,
                    Username = googleUser.Email, // Use email as username for Google users
                    FullName = googleUser.FullName,
                    AvatarUrl = googleUser.AvatarUrl,
                    GoogleId = googleUser.GoogleId,
                    IsActive = true,
                    EmailConfirmed = googleUser.IsEmailVerified, // Google users have verified emails
                    PasswordHash = string.Empty, // Google users don't have password
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await userRepository.AddAsync(user, cancellationToken);
            }
        }

        if (!user.IsActive) throw new UnauthorizedAccessException("Account is disabled");

        // Update last login
        await userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow, cancellationToken);

        return await GenerateLoginResponseAsync(user, cancellationToken);
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var user = await userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        if (user == null || !user.IsActive) throw new UnauthorizedAccessException("User not found or disabled");

        // Revoke old refresh token
        await refreshTokenRepository.RevokeTokenAsync(request.RefreshToken, "system", cancellationToken);

        // Generate new tokens
        var userProfile = new UserProfile(user.Id, user.Email, user.Username, user.FullName, user.AvatarUrl, []);
        var newAccessToken = jwtTokenService.GenerateAccessToken(userProfile);
        var newRefreshToken = jwtTokenService.GenerateRefreshToken();

        // Store new refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.Add(jwtTokenService.RefreshTokenLifetime)
        };

        await refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        return new RefreshTokenResponse(
            newAccessToken,
            newRefreshToken,
            DateTime.UtcNow.Add(jwtTokenService.AccessTokenLifetime));
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        await refreshTokenRepository.RevokeTokenAsync(request.RefreshToken, "user", cancellationToken);
    }

    public async Task<ApiKeyVerificationResponse> VerifyApiKeyAsync(string apiKey,
        CancellationToken cancellationToken = default)
    {
        var hashedKey = apiKeyHasher.HashApiKey(apiKey);
        var apiKeyEntity = await apiKeyRepository.GetActiveKeyByHashAsync(hashedKey, cancellationToken);

        if (apiKeyEntity == null) return new ApiKeyVerificationResponse(false, null, [], null);

        // Check if key is expired
        if (apiKeyEntity.ExpiresAt.HasValue && apiKeyEntity.ExpiresAt <= DateTime.UtcNow)
            return new ApiKeyVerificationResponse(false, null, [], null);

        // Update usage
        await apiKeyRepository.UpdateLastUsedAsync(apiKeyEntity.Id, DateTime.UtcNow, cancellationToken);
        await apiKeyRepository.IncrementUsageCountAsync(apiKeyEntity.Id, cancellationToken);

        var user = await userRepository.GetByIdAsync(apiKeyEntity.UserId, cancellationToken);
        if (user == null || !user.IsActive) return new ApiKeyVerificationResponse(false, null, [], null);

        var userProfile = new UserProfile(user.Id, user.Email, user.Username, user.FullName, user.AvatarUrl, []);

        return new ApiKeyVerificationResponse(true, user.Id, apiKeyEntity.Scopes, userProfile);
    }

    private async Task<LoginResponse> GenerateLoginResponseAsync(User user, CancellationToken cancellationToken)
    {
        var userProfile = new UserProfile(user.Id, user.Email, user.Username, user.FullName, user.AvatarUrl, []);

        var accessToken = jwtTokenService.GenerateAccessToken(userProfile);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        // Store refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.Add(jwtTokenService.RefreshTokenLifetime)
        };

        await refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        return new LoginResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.Add(jwtTokenService.AccessTokenLifetime),
            userProfile);
    }
}