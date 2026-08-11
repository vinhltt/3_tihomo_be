using Identity.Api.Services;
using Identity.Application.Common.Interfaces;
using Identity.Application.Services.RefreshTokens;
using Identity.Contracts;
using Identity.Domain.Dtos.Authentication;
using Identity.Domain.Dtos.RefreshTokens;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LoginResponse = Identity.Contracts.LoginResponse;
using RefreshTokenRequest = Identity.Domain.Dtos.Authentication.RefreshTokenRequest;
using RefreshTokenResponse = Identity.Domain.Dtos.Authentication.RefreshTokenResponse;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    ITokenVerificationService tokenVerificationService,
    IUserService userService,
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    ILogger<AuthController> logger)
    : ControllerBase
{
    /// <summary>
    ///     Authenticate user with social login token
    /// </summary>
    [HttpPost("social-login")]
    public async Task<ActionResult<LoginResponse>> SocialLogin([FromBody] SocialLoginRequest request)
    {
        try
        {
            logger.LogInformation("=== Social Login Request Start ===");
            logger.LogInformation("Provider: {Provider}, Token Length: {TokenLength}", 
                request?.Provider ?? "null", 
                request?.Token?.Length ?? 0);

            if (string.IsNullOrEmpty(request?.Provider) || string.IsNullOrEmpty(request?.Token))
            {
                logger.LogWarning("Social login failed - Provider or token is empty");
                return BadRequest("Provider and token are required");
            }

            logger.LogDebug("Token Preview: {TokenPreview}...", 
                request.Token.Length > 50 ? request.Token.Substring(0, 50) : request.Token);

            // Verify the token with the social provider
            logger.LogInformation("Verifying token with {Provider}...", request.Provider);
            var socialUserInfo = await tokenVerificationService.VerifyTokenAsync(request.Provider, request.Token);

            if (socialUserInfo == null)
            {
                logger.LogWarning("Token verification failed for provider {Provider}", request.Provider);
                return Unauthorized("Invalid token");
            }

            logger.LogInformation("Token verified successfully. Email: {Email}, Name: {Name}", 
                socialUserInfo.Email, socialUserInfo.Name);

            // Get or create user
            logger.LogInformation("Getting or creating user for email: {Email}", socialUserInfo.Email);
            var user = await userService.GetOrCreateUserAsync(socialUserInfo);

            if (user == null)
            {
                logger.LogError("Failed to get or create user for email: {Email}", socialUserInfo.Email);
                return StatusCode(500, "Failed to process user information");
            }

            logger.LogInformation("User found/created: Id={UserId}, Email={Email}, IsActive={IsActive}", 
                user.Id, user.Email, user.IsActive);

            // Check if user is active
            if (!user.IsActive)
            {
                logger.LogWarning("User {UserId} is deactivated", user.Id);
                return Unauthorized("Account is deactivated");
            }

            // Tạo JWT access token
            logger.LogInformation("Generating JWT access token for user {UserId}", user.Id);
            var accessToken = jwtService.GenerateAccessToken(user);
            logger.LogDebug("Access token generated: {TokenPreview}...", 
                accessToken.Length > 50 ? accessToken.Substring(0, 50) : accessToken);

            // Generate and store refresh token in database
            // Tạo và lưu refresh token vào database
            logger.LogInformation("Generating refresh token for user {UserId}", user.Id);
            var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id);

            var tokenExpiration = jwtService.GetTokenExpiration();
            logger.LogInformation("Token expiration set to: {Expiration}", tokenExpiration);

            // Map user to response
            var userInfo = await userService.MapToUserInfoAsync(user);

            var response = new LoginResponse
            {
                User = userInfo,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = tokenExpiration
            };

            logger.LogInformation("User {UserId} logged in successfully via {Provider}", user.Id, request.Provider);
            logger.LogInformation("=== Social Login Request End - SUCCESS ===");

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during social login with provider {Provider}", request?.Provider ?? "unknown");
            logger.LogError("Exception Type: {ExceptionType}, Message: {Message}", 
                ex.GetType().Name, ex.Message);
            if (ex.InnerException != null)
            {
                logger.LogError("Inner Exception: {InnerExceptionType}, Message: {InnerMessage}", 
                    ex.InnerException.GetType().Name, ex.InnerException.Message);
            }
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Basic login with real JWT token generation (EN)<br/>
    /// Login cơ bản với tạo JWT token thật (VI)
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Create a test user for JWT generation
            var testUser = new Identity.Domain.Entities.User
            {
                Id = Guid.CreateVersion7(),
                Email = request.Username,
                Username = request.Username,
                Name = "Test User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Generate real JWT token
            var accessToken = jwtService.GenerateAccessToken(testUser);
            var tokenExpiration = jwtService.GetTokenExpiration();

            // Map to response user info
            var userInfo = await userService.MapToUserInfoAsync(testUser);

            return Ok(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = "test_refresh_123", // TODO: Generate real refresh token
                ExpiresAt = tokenExpiration,
                User = userInfo
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Validate access token
    /// </summary>
    [HttpPost("validate-token")]
    public ActionResult ValidateToken([FromBody] string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token)) return BadRequest("Token is required");

            var principal = jwtService.ValidateToken(token);

            if (principal == null) return Unauthorized("Invalid token");

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = principal.FindFirst(ClaimTypes.Name)?.Value;

            return Ok(new
            {
                Valid = true,
                UserId = userId,
                Email = email,
                Name = name
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating token");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Refresh access token using refresh token
    ///     Làm mới access token bằng refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // Validate refresh token and get user ID
            // Validate refresh token và lấy user ID
            var userId = await refreshTokenService.ValidateRefreshTokenAsync(request.RefreshToken);
            if (!userId.HasValue) return BadRequest("Invalid or expired refresh token");

            // Get user information
            // Lấy thông tin user
            var user = await userService.GetUserByIdAsync(userId.Value);
            if (user == null || !user.IsActive) return BadRequest("User not found or inactive");

            // Rotate refresh token (revoke old, generate new)
            // Xoay vòng refresh token (thu hồi cũ, tạo mới)
            var newRefreshToken = await refreshTokenService.RotateRefreshTokenAsync(request.RefreshToken);
            if (newRefreshToken == null)
                return BadRequest("Failed to rotate refresh token"); // Generate new access token using JWT service

            // Tạo access token mới bằng JWT service
            var newAccessToken = jwtService.GenerateAccessToken(user);
            var response = new RefreshTokenResponse(
                newAccessToken,
                newRefreshToken,
                DateTime.UtcNow.AddHours(1) // 1 hour expiration for access token
            );

            logger.LogInformation("Successfully refreshed tokens for user {UserId}", userId.Value);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Logout user and revoke refresh tokens
    ///     Đăng xuất user và thu hồi refresh tokens
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.RefreshToken)) return BadRequest("Refresh token is required");

            // Revoke the specific refresh token
            // Thu hồi refresh token cụ thể
            var revokeSuccess = await refreshTokenService.RevokeRefreshTokenAsync(
                request.RefreshToken,
                "user_logout"
            );

            if (!revokeSuccess) logger.LogWarning("Failed to revoke refresh token during logout");
            // Still return success since user intent is to logout
            // Vẫn trả về success vì ý định của user là logout
            logger.LogInformation("User logged out successfully");
            return Ok(new { Message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Revoke all refresh tokens for current user
    ///     Thu hồi tất cả refresh token của user hiện tại
    /// </summary>
    [HttpPost("revoke-all-tokens")]
    public async Task<ActionResult> RevokeAllTokens([FromBody] RevokeAllTokensRequest request)
    {
        try
        {
            if (request.UserId == Guid.Empty) return BadRequest("Valid user ID is required");

            // Revoke all refresh tokens for the user
            // Thu hồi tất cả refresh token của user
            var revokedCount = await refreshTokenService.RevokeAllUserTokensAsync(
                request.UserId,
                "admin_revoke_all"
            );

            logger.LogInformation("Revoked {Count} refresh tokens for user {UserId}", revokedCount, request.UserId);

            return Ok(new
            {
                Message = "All refresh tokens revoked successfully",
                RevokedCount = revokedCount
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error revoking all tokens for user {UserId}", request.UserId);
            return StatusCode(500, "Internal server error");
        }
    }
}