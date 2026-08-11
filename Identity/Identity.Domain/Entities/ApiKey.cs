using System.ComponentModel.DataAnnotations;
using Identity.Domain.Enums;
using Shared.EntityFramework.BaseEfModels;

namespace Identity.Domain.Entities;

/// <summary>
/// API Key Entity - Thực thể API Key cho third-party integration (EN)<br/>
/// Thực thể khóa API cho tích hợp bên thứ ba (VI)
/// </summary>
public class ApiKey : BaseEntity<Guid>
{
    /// <summary>
    /// User ID - ID của user sở hữu API key (EN)<br/>
    /// ID của người dùng sở hữu khóa API (VI)
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Name - Tên mô tả của API key (EN)<br/>
    /// Tên mô tả của khóa API (VI)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Key Hash - Hash của API key (SHA-256) (EN)<br/>
    /// Hash của khóa API (SHA-256) (VI)
    /// </summary>
    [Required]
    [StringLength(500)]
    public string KeyHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Key Prefix - Prefix của API key (tihomo_xxxxx) (EN)<br/>
    /// Prefix của khóa API (tihomo_xxxxx) (VI)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string KeyPrefix { get; set; } = string.Empty;
    
    /// <summary>
    /// Description - Mô tả chi tiết API key (EN)<br/>
    /// Mô tả chi tiết khóa API (VI)
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Scopes - Danh sách permissions (EN)<br/>
    /// Danh sách quyền hạn (VI)
    /// </summary>
    public List<string> Scopes { get; set; } = [];
    
    /// <summary>
    /// Status - Trạng thái API key (EN)<br/>
    /// Trạng thái khóa API (VI)
    /// </summary>
    public ApiKeyStatus Status { get; set; } = ApiKeyStatus.Active;
    
    /// <summary>
    /// Rate Limit Per Minute - Giới hạn request per minute (EN)<br/>
    /// Giới hạn yêu cầu mỗi phút (VI)
    /// </summary>
    [Range(1, 1000)]
    public int RateLimitPerMinute { get; set; } = 100;
    
    /// <summary>
    /// Daily Usage Quota - Giới hạn request per day (EN)<br/>
    /// Hạn ngạch sử dụng hàng ngày (VI)
    /// </summary>
    [Range(1, 100000)]
    public int DailyUsageQuota { get; set; } = 10000;
    
    /// <summary>
    /// Usage Count - Tổng số lần sử dụng (EN)<br/>
    /// Tổng số lần sử dụng (VI)
    /// </summary>
    public long UsageCount { get; set; } = 0;
    
    /// <summary>
    /// IP Whitelist - Danh sách IP được phép (EN)<br/>
    /// Danh sách IP được phép (VI)
    /// </summary>
    public List<string> IpWhitelist { get; set; } = [];
    
    /// <summary>
    /// Security Settings - Cài đặt bảo mật (EN)<br/>
    /// Cài đặt bảo mật (VI)
    /// </summary>
    public ApiKeySecuritySettings SecuritySettings { get; set; } = new();
    
    /// <summary>
    /// Expires At - Thời gian hết hạn (EN)<br/>
    /// Thời gian hết hạn (VI)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Last Used At - Lần sử dụng cuối (EN)<br/>
    /// Lần sử dụng cuối (VI)
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
    
    /// <summary>
    /// Revoked At - Thời gian thu hồi (EN)<br/>
    /// Thời gian thu hồi (VI)
    /// </summary>
    public DateTime? RevokedAt { get; set; }
    
    /// <summary>
    /// Today Usage Count - Số lần sử dụng hôm nay (EN)<br/>
    /// Số lần sử dụng hôm nay (VI)
    /// </summary>
    public int TodayUsageCount { get; set; } = 0;
    
    /// <summary>
    /// Last Reset Date - Ngày reset counter cuối cùng (EN)<br/>
    /// Ngày reset counter cuối cùng (VI)
    /// </summary>
    public DateTime? LastResetDate { get; set; }
    
    /// <summary>
    /// Is Active - Kiểm tra API key có hoạt động không (EN)<br/>
    /// Kiểm tra khóa API có hoạt động không (VI)
    /// </summary>
    public bool IsActive => Status == ApiKeyStatus.Active && 
                           (ExpiresAt == null || ExpiresAt > DateTime.UtcNow) &&
                           RevokedAt == null;
    
    /// <summary>
    /// Is Expired - Kiểm tra API key có hết hạn không (EN)<br/>
    /// Kiểm tra khóa API có hết hạn không (VI)
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt <= DateTime.UtcNow;
    
    /// <summary>
    /// Is Revoked - Kiểm tra API key có bị thu hồi không (EN)<br/>
    /// Kiểm tra khóa API có bị thu hồi không (VI)
    /// </summary>
    public bool IsRevoked => Status == ApiKeyStatus.Revoked || RevokedAt.HasValue;
    
    /// <summary>
    /// Is Rate Limited - Kiểm tra có đang bị giới hạn tốc độ không (EN)<br/>
    /// Kiểm tra có đang bị giới hạn tốc độ không (VI)
    /// </summary>
    public bool IsRateLimited => SecuritySettings.EnableRateLimiting && 
                                TodayUsageCount >= DailyUsageQuota;

    // Navigation properties
    /// <summary>
    /// User - Navigation property tới User (EN)<br/>
    /// Người dùng - Navigation property tới User (VI)
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Usage Logs - Navigation property tới ApiKeyUsageLog (EN)<br/>
    /// Log sử dụng - Navigation property tới ApiKeyUsageLog (VI)
    /// </summary>
    public virtual ICollection<ApiKeyUsageLog> UsageLogs { get; set; } = [];
}