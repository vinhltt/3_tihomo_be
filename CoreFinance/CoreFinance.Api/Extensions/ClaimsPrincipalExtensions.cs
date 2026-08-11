using System.Security.Claims;

namespace CoreFinance.Api.Extensions;

/// <summary>
///     Extension methods for ClaimsPrincipal to extract user information. (EN)<br/>
///     Phương thức mở rộng cho ClaimsPrincipal để trích xuất thông tin người dùng. (VI)
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    ///     Extracts the UserId from JWT claims. (EN)<br/>
    ///     Trích xuất UserId từ JWT claims. (VI)
    /// </summary>
    /// <param name="principal">The ClaimsPrincipal object from the request context</param>
    /// <returns>UserId as Guid if found, otherwise null</returns>
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        // Try multiple claim types that might contain the user ID
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier); // Add explicit check for nameid

        if (userIdClaim?.Value != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    /// <summary>
    ///     Extracts the Username from JWT claims. (EN)<br/>
    ///     Trích xuất Username từ JWT claims. (VI)
    /// </summary>
    /// <param name="principal">The ClaimsPrincipal object from the request context</param>
    /// <returns>Username as string if found, otherwise null</returns>
    public static string? GetUsername(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Name)?.Value
               ?? principal.FindFirst("preferred_username")?.Value
               ?? principal.FindFirst("username")?.Value;
    }

    /// <summary>
    ///     Extracts the Email from JWT claims. (EN)<br/>
    ///     Trích xuất Email từ JWT claims. (VI)
    /// </summary>
    /// <param name="principal">The ClaimsPrincipal principal</param>
    /// <returns>Email as string if found, otherwise null</returns>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value
               ?? principal.FindFirst("email")?.Value;
    }
}