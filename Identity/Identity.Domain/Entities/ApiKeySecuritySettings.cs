using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Entities;

/// <summary>
/// API Key Security Settings - Cài đặt bảo mật cho API key (EN)<br/>
/// Cài đặt bảo mật cho khóa API (VI)
/// </summary>
public class ApiKeySecuritySettings
{
    /// <summary>
    /// Require HTTPS - Yêu cầu sử dụng HTTPS (EN)<br/>
    /// Yêu cầu sử dụng HTTPS (VI)
    /// </summary>
    public bool RequireHttps { get; set; } = true;
    
    /// <summary>
    /// Allow CORS Requests - Cho phép yêu cầu CORS (EN)<br/>
    /// Cho phép yêu cầu CORS (VI)
    /// </summary>
    public bool AllowCorsRequests { get; set; } = false;
    
    /// <summary>
    /// Allowed Origins - Danh sách nguồn được phép (EN)<br/>
    /// Danh sách nguồn được phép (VI)
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = [];
    
    /// <summary>
    /// Enable Usage Analytics - Bật phân tích sử dụng (EN)<br/>
    /// Bật phân tích sử dụng (VI)
    /// </summary>
    public bool EnableUsageAnalytics { get; set; } = true;
    
    /// <summary>
    /// Max Requests Per Second - Giới hạn yêu cầu tối đa mỗi giây (EN)<br/>
    /// Giới hạn yêu cầu tối đa mỗi giây (VI)
    /// </summary>
    [Range(1, 1000)]
    public int MaxRequestsPerSecond { get; set; } = 10;
    
    /// <summary>
    /// Enable IP Validation - Bật xác thực IP (EN)<br/>
    /// Bật xác thực IP (VI)
    /// </summary>
    public bool EnableIpValidation { get; set; } = false;
    
    /// <summary>
    /// Enable Rate Limiting - Bật giới hạn tốc độ (EN)<br/>
    /// Bật giới hạn tốc độ (VI)
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;
} 