using Shared.EntityFramework.BaseEfModels;

namespace Identity.Domain.Entities;

/// <summary>
///     OAuth2/OpenID Connect client configuration for SSO
///     Cấu hình client OAuth2/OpenID Connect cho SSO
/// </summary>
public class OAuthClient : BaseEntity<Guid>
{
    /// <summary>
    ///     Client identifier used in OAuth2 flows
    ///     Định danh client dùng trong OAuth2 flows
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    ///     Hashed client secret for confidential clients
    ///     Client secret đã hash cho confidential clients
    /// </summary>
    public string? ClientSecretHash { get; set; }

    /// <summary>
    ///     Human-readable client name
    ///     Tên client dễ đọc
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Client description
    ///     Mô tả client
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Client type: Public (mobile/SPA) or Confidential (server-side)
    ///     Loại client: Public (mobile/SPA) hoặc Confidential (server-side)
    /// </summary>
    public OAuthClientType Type { get; set; }

    /// <summary>
    ///     Platform this client is designed for
    ///     Nền tảng mà client này được thiết kế
    /// </summary>
    public ClientPlatform Platform { get; set; }

    /// <summary>
    ///     Allowed redirect URIs (comma-separated)
    ///     Các redirect URI được phép (phân cách bằng dấu phẩy)
    /// </summary>
    public string RedirectUris { get; set; } = string.Empty;

    /// <summary>
    ///     Allowed post-logout redirect URIs (comma-separated)
    ///     Các post-logout redirect URI được phép (phân cách bằng dấu phẩy)
    /// </summary>
    public string? PostLogoutRedirectUris { get; set; }

    /// <summary>
    ///     Allowed scopes (comma-separated)
    ///     Các scope được phép (phân cách bằng dấu phẩy)
    /// </summary>
    public string AllowedScopes { get; set; } = string.Empty;

    /// <summary>
    ///     Whether client is active
    ///     Client có đang hoạt động hay không
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Token lifetime in seconds
    ///     Thời gian sống của token tính bằng giây
    /// </summary>
    public int AccessTokenLifetime { get; set; } = 3600; // 1 hour

    /// <summary>
    ///     Refresh token lifetime in seconds
    ///     Thời gian sống của refresh token tính bằng giây
    /// </summary>
    public int RefreshTokenLifetime { get; set; } = 2592000; // 30 days

    /// <summary>
    ///     Whether refresh tokens are allowed
    ///     Có cho phép refresh token hay không
    /// </summary>
    public bool AllowRefreshTokens { get; set; } = true;

    /// <summary>
    ///     Whether to require PKCE for this client
    ///     Có yêu cầu PKCE cho client này hay không
    /// </summary>
    public bool RequirePkce { get; set; } = true;

    /// <summary>
    ///     Application URL for reference
    ///     URL ứng dụng để tham khảo
    /// </summary>
    public string? ApplicationUrl { get; set; }
}

/// <summary>
///     OAuth2 client types
///     Các loại OAuth2 client
/// </summary>
public enum OAuthClientType
{
    /// <summary>
    ///     Public client (mobile apps, SPAs) - cannot securely store secrets
    ///     Public client (mobile apps, SPAs) - không thể lưu trữ secret an toàn
    /// </summary>
    Public = 0,

    /// <summary>
    ///     Confidential client (server-side apps) - can securely store secrets
    ///     Confidential client (server-side apps) - có thể lưu trữ secret an toàn
    /// </summary>
    Confidential = 1
}

/// <summary>
///     Platforms that clients can be designed for
///     Các nền tảng mà client có thể được thiết kế
/// </summary>
public enum ClientPlatform
{
    /// <summary>
    ///     Web application (Nuxt.js, React, etc.)
    ///     Ứng dụng web (Nuxt.js, React, v.v.)
    /// </summary>
    Web = 0,

    /// <summary>
    ///     iOS mobile application
    ///     Ứng dụng di động iOS
    /// </summary>
    IOs = 1,

    /// <summary>
    ///     Android mobile application
    ///     Ứng dụng di động Android
    /// </summary>
    Android = 2,

    /// <summary>
    ///     Cross-platform mobile (Flutter, React Native)
    ///     Mobile đa nền tảng (Flutter, React Native)
    /// </summary>
    Mobile = 3,

    /// <summary>
    ///     Desktop application
    ///     Ứng dụng desktop
    /// </summary>
    Desktop = 4,

    /// <summary>
    ///     Server-to-server API client
    ///     API client server-to-server
    /// </summary>
    Api = 5
}