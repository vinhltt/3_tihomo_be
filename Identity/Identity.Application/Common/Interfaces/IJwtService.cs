using System.Security.Claims;
using Identity.Domain.Entities;

namespace Identity.Application.Common.Interfaces;

/// <summary>
/// JWT Service Interface (EN)<br/>
/// Interface dịch vụ JWT (VI)
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate access token for user (EN)<br/>
    /// Tạo access token cho user (VI)
    /// </summary>
    string GenerateAccessToken(User user);
    
    /// <summary>
    /// Generate refresh token (EN)<br/>
    /// Tạo refresh token (VI)
    /// </summary>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validate JWT token (EN)<br/>
    /// Xác thực JWT token (VI)
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
    
    /// <summary>
    /// Get token expiration time (EN)<br/>
    /// Lấy thời gian hết hạn token (VI)
    /// </summary>
    DateTime GetTokenExpiration();
}