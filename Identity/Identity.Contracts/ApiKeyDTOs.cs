using System.ComponentModel.DataAnnotations;

namespace Identity.Contracts;

#region Request DTOs

/// <summary>
/// Simple request model for API key creation (end users) (EN)<br/>
/// Model yêu cầu đơn giản để tạo API key cho end users (VI)
/// </summary>
public class CreateSimpleApiKeyRequest
{
    /// <summary>
    /// Name - Tên API key (EN)<br/>
    /// Tên API key (VI)
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Enhanced request model for API key creation with security features (EN)<br/>
/// Model yêu cầu nâng cao để tạo API key với tính năng bảo mật (VI)
/// </summary>
public class CreateApiKeyRequest
{
    /// <summary>
    /// Name - Tên mô tả của API key (EN)<br/>
    /// Tên mô tả của khóa API (VI)
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
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
    [Required]
    [MinLength(1)]
    public List<string> Scopes { get; set; } = [];
    
    /// <summary>
    /// Expires At - Thời gian hết hạn (EN)<br/>
    /// Thời gian hết hạn (VI)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
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
    /// IP Whitelist - Danh sách IP được phép (EN)<br/>
    /// Danh sách IP được phép (VI)
    /// </summary>
    public List<string> IpWhitelist { get; set; } = [];
    
    /// <summary>
    /// Security Settings - Cài đặt bảo mật (EN)<br/>
    /// Cài đặt bảo mật (VI)
    /// </summary>
    public ApiKeySecuritySettingsDto SecuritySettings { get; set; } = new();
}

/// <summary>
/// Request model for API key update (EN)<br/>
/// Model yêu cầu để cập nhật API key (VI)
/// </summary>
public class UpdateApiKeyRequest
{
    /// <summary>
    /// Name - Tên mô tả của API key (EN)<br/>
    /// Tên mô tả của khóa API (VI)
    /// </summary>
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; set; }
    
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
    public List<string>? Scopes { get; set; }
    
    /// <summary>
    /// Expires At - Thời gian hết hạn (EN)<br/>
    /// Thời gian hết hạn (VI)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Rate Limit Per Minute - Giới hạn request per minute (EN)<br/>
    /// Giới hạn yêu cầu mỗi phút (VI)
    /// </summary>
    [Range(1, 1000)]
    public int? RateLimitPerMinute { get; set; }
    
    /// <summary>
    /// Daily Usage Quota - Giới hạn request per day (EN)<br/>
    /// Hạn ngạch sử dụng hàng ngày (VI)
    /// </summary>
    [Range(1, 100000)]
    public int? DailyUsageQuota { get; set; }
    
    /// <summary>
    /// IP Whitelist - Danh sách IP được phép (EN)<br/>
    /// Danh sách IP được phép (VI)
    /// </summary>
    public List<string>? IpWhitelist { get; set; }
    
    /// <summary>
    /// Security Settings - Cài đặt bảo mật (EN)<br/>
    /// Cài đặt bảo mật (VI)
    /// </summary>
    public ApiKeySecuritySettingsDto? SecuritySettings { get; set; }
}

/// <summary>
/// Request model for usage analytics query (EN)<br/>
/// Model yêu cầu cho truy vấn phân tích sử dụng (VI)
/// </summary>
public class UsageQueryRequest
{
    /// <summary>
    /// Start Date - Ngày bắt đầu (EN)<br/>
    /// Ngày bắt đầu (VI)
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// End Date - Ngày kết thúc (EN)<br/>
    /// Ngày kết thúc (VI)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Group By - Nhóm theo (day, hour, month) (EN)<br/>
    /// Nhóm theo (ngày, giờ, tháng) (VI)
    /// </summary>
    public string GroupBy { get; set; } = "day";
    
    /// <summary>
    /// Include Errors - Bao gồm lỗi (EN)<br/>
    /// Bao gồm lỗi (VI)
    /// </summary>
    public bool IncludeErrors { get; set; } = true;
    
    /// <summary>
    /// Limit - Giới hạn kết quả (EN)<br/>
    /// Giới hạn kết quả (VI)
    /// </summary>
    [Range(1, 1000)]
    public int Limit { get; set; } = 100;
}

/// <summary>
/// Request model for list API keys query (EN)<br/>
/// Model yêu cầu cho danh sách API keys (VI)
/// </summary>
public class ListApiKeysQuery
{
    /// <summary>
    /// Status - Trạng thái (active, revoked, expired) (EN)<br/>
    /// Trạng thái (hoạt động, thu hồi, hết hạn) (VI)
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Scope - Lọc theo scope (EN)<br/>
    /// Lọc theo phạm vi (VI)
    /// </summary>
    public string? Scope { get; set; }
    
    /// <summary>
    /// Search - Tìm kiếm theo tên (EN)<br/>
    /// Tìm kiếm theo tên (VI)
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// Cursor - Con trỏ phân trang (EN)<br/>
    /// Con trỏ phân trang (VI)
    /// </summary>
    public string? Cursor { get; set; }
    
    /// <summary>
    /// Limit - Giới hạn kết quả (EN)<br/>
    /// Giới hạn kết quả (VI)
    /// </summary>
    [Range(1, 100)]
    public int Limit { get; set; } = 20;
}

#endregion

#region Response DTOs

/// <summary>
/// Enhanced response model for API key creation (EN)<br/>
/// Model phản hồi nâng cao để tạo API key (VI)
/// </summary>
public class CreateApiKeyResponse
{
    /// <summary>
    /// ID - ID của API key (EN)<br/>
    /// ID của khóa API (VI)
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Name - Tên mô tả của API key (EN)<br/>
    /// Tên mô tả của khóa API (VI)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// API Key - Khóa API thực tế (chỉ hiện lần đầu) (EN)<br/>
    /// Khóa API thực tế (chỉ hiện lần đầu) (VI)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Key Prefix - Prefix của API key (EN)<br/>
    /// Prefix của khóa API (VI)
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;
    
    /// <summary>
    /// Description - Mô tả chi tiết (EN)<br/>
    /// Mô tả chi tiết (VI)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Scopes - Danh sách permissions (EN)<br/>
    /// Danh sách quyền hạn (VI)
    /// </summary>
    public List<string> Scopes { get; set; } = [];
    
    /// <summary>
    /// Rate Limit Per Minute - Giới hạn request per minute (EN)<br/>
    /// Giới hạn yêu cầu mỗi phút (VI)
    /// </summary>
    public int RateLimitPerMinute { get; set; }
    
    /// <summary>
    /// Daily Usage Quota - Giới hạn request per day (EN)<br/>
    /// Hạn ngạch sử dụng hàng ngày (VI)
    /// </summary>
    public int DailyUsageQuota { get; set; }
    
    /// <summary>
    /// IP Whitelist - Danh sách IP được phép (EN)<br/>
    /// Danh sách IP được phép (VI)
    /// </summary>
    public List<string> IpWhitelist { get; set; } = [];
    
    /// <summary>
    /// Security Settings - Cài đặt bảo mật (EN)<br/>
    /// Cài đặt bảo mật (VI)
    /// </summary>
    public ApiKeySecuritySettingsDto SecuritySettings { get; set; } = new();
    
    /// <summary>
    /// Created At - Thời gian tạo (EN)<br/>
    /// Thời gian tạo (VI)
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Expires At - Thời gian hết hạn (EN)<br/>
    /// Thời gian hết hạn (VI)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Enhanced response model for API key information (EN)<br/>
/// Model phản hồi nâng cao cho thông tin API key (VI)
/// </summary>
public class ApiKeyResponse
{
    /// <summary>
    /// ID - ID của API key (EN)<br/>
    /// ID của khóa API (VI)
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Name - Tên mô tả của API key (EN)<br/>
    /// Tên mô tả của khóa API (VI)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Key Prefix - Prefix của API key (EN)<br/>
    /// Prefix của khóa API (VI)
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;
    
    /// <summary>
    /// Description - Mô tả chi tiết (EN)<br/>
    /// Mô tả chi tiết (VI)
    /// </summary>
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
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Rate Limit Per Minute - Giới hạn request per minute (EN)<br/>
    /// Giới hạn yêu cầu mỗi phút (VI)
    /// </summary>
    public int RateLimitPerMinute { get; set; }
    
    /// <summary>
    /// Daily Usage Quota - Giới hạn request per day (EN)<br/>
    /// Hạn ngạch sử dụng hàng ngày (VI)
    /// </summary>
    public int DailyUsageQuota { get; set; }
    
    /// <summary>
    /// Today Usage Count - Số lần sử dụng hôm nay (EN)<br/>
    /// Số lần sử dụng hôm nay (VI)
    /// </summary>
    public int TodayUsageCount { get; set; }
    
    /// <summary>
    /// Usage Count - Tổng số lần sử dụng (EN)<br/>
    /// Tổng số lần sử dụng (VI)
    /// </summary>
    public long UsageCount { get; set; }
    
    /// <summary>
    /// IP Whitelist - Danh sách IP được phép (EN)<br/>
    /// Danh sách IP được phép (VI)
    /// </summary>
    public List<string> IpWhitelist { get; set; } = [];
    
    /// <summary>
    /// Security Settings - Cài đặt bảo mật (EN)<br/>
    /// Cài đặt bảo mật (VI)
    /// </summary>
    public ApiKeySecuritySettingsDto SecuritySettings { get; set; } = new();
    
    /// <summary>
    /// Created At - Thời gian tạo (EN)<br/>
    /// Thời gian tạo (VI)
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Updated At - Thời gian cập nhật (EN)<br/>
    /// Thời gian cập nhật (VI)
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
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
    /// Is Active - Có đang hoạt động không (EN)<br/>
    /// Có đang hoạt động không (VI)
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Is Expired - Có hết hạn không (EN)<br/>
    /// Có hết hạn không (VI)
    /// </summary>
    public bool IsExpired { get; set; }
    
    /// <summary>
    /// Is Revoked - Có bị thu hồi không (EN)<br/>
    /// Có bị thu hồi không (VI)
    /// </summary>
    public bool IsRevoked { get; set; }
    
    /// <summary>
    /// Is Rate Limited - Có đang bị giới hạn tốc độ không (EN)<br/>
    /// Có đang bị giới hạn tốc độ không (VI)
    /// </summary>
    public bool IsRateLimited { get; set; }
}

/// <summary>
/// Response model for API key usage analytics (EN)<br/>
/// Model phản hồi cho phân tích sử dụng API key (VI)
/// </summary>
public class ApiKeyUsageResponse
{
    /// <summary>
    /// API Key ID - ID của API key (EN)<br/>
    /// ID của khóa API (VI)
    /// </summary>
    public Guid ApiKeyId { get; set; }
    
    /// <summary>
    /// Usage Statistics - Thống kê sử dụng (EN)<br/>
    /// Thống kê sử dụng (VI)
    /// </summary>
    public UsageStatistics Statistics { get; set; } = new();
    
    /// <summary>
    /// Usage Data - Dữ liệu sử dụng theo thời gian (EN)<br/>
    /// Dữ liệu sử dụng theo thời gian (VI)
    /// </summary>
    public List<UsageDataPoint> UsageData { get; set; } = [];
    
    /// <summary>
    /// Recent Activities - Hoạt động gần đây (EN)<br/>
    /// Hoạt động gần đây (VI)
    /// </summary>
    public List<ApiKeyUsageLogDto> RecentActivities { get; set; } = [];
}

/// <summary>
/// Response model for rotating API key (EN)<br/>
/// Model phản hồi cho xoay API key (VI)
/// </summary>
public class RotateApiKeyResponse
{
    /// <summary>
    /// ID - ID của API key (EN)<br/>
    /// ID của khóa API (VI)
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// New API Key - API key mới (EN)<br/>
    /// Khóa API mới (VI)
    /// </summary>
    public string NewApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// New Key Prefix - Prefix mới (EN)<br/>
    /// Prefix mới (VI)
    /// </summary>
    public string NewKeyPrefix { get; set; } = string.Empty;
    
    /// <summary>
    /// Old Key Prefix - Prefix cũ (để reference) (EN)<br/>
    /// Prefix cũ (để tham khảo) (VI)
    /// </summary>
    public string OldKeyPrefix { get; set; } = string.Empty;
    
    /// <summary>
    /// Rotated At - Thời gian xoay (EN)<br/>
    /// Thời gian xoay (VI)
    /// </summary>
    public DateTime RotatedAt { get; set; }
}

/// <summary>
/// Response model for verifying API key (EN)<br/>
/// Model phản hồi cho xác thực API key (VI)
/// </summary>
public class VerifyApiKeyResponse
{
    /// <summary>
    /// Is Valid - Có hợp lệ không (EN)<br/>
    /// Có hợp lệ không (VI)
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// User ID - ID của user (EN)<br/>
    /// ID của người dùng (VI)
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// User Email - Email của user (EN)<br/>
    /// Email của người dùng (VI)
    /// </summary>
    public string? UserEmail { get; set; }
    
    /// <summary>
    /// API Key ID - ID của API key (EN)<br/>
    /// ID của khóa API (VI)
    /// </summary>
    public Guid? ApiKeyId { get; set; }
    
    /// <summary>
    /// Scopes - Danh sách permissions (EN)<br/>
    /// Danh sách quyền hạn (VI)
    /// </summary>
    public List<string> Scopes { get; set; } = [];
    
    /// <summary>
    /// Message - Thông báo (EN)<br/>
    /// Thông báo (VI)
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Error Message - Thông báo lỗi (EN)<br/>
    /// Thông báo lỗi (VI)
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Response model for API key exchange to JWT token (EN)<br/>
/// Model phản hồi cho trao đổi API key thành JWT token (VI)
/// </summary>
public class ApiKeyExchangeResponse
{
    /// <summary>
    /// Access Token - JWT token (EN)<br/>
    /// Access Token - JWT token (VI)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Expires At - Thời gian hết hạn (EN)<br/>
    /// Thời gian hết hạn (VI)
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Token Type - Loại token (EN)<br/>
    /// Loại token (VI)
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
    
    /// <summary>
    /// User ID - ID của user (EN)<br/>
    /// ID của người dùng (VI)
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// User Email - Email của user (EN)<br/>
    /// Email của người dùng (VI)
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;
}

/// <summary>
/// Response model for list API keys (EN)<br/>
/// Model phản hồi cho danh sách API keys (VI)
/// </summary>
public class ListApiKeysResponse
{
    /// <summary>
    /// Data - Danh sách API keys (EN)<br/>
    /// Dữ liệu - Danh sách khóa API (VI)
    /// </summary>
    public List<ApiKeyResponse> Data { get; set; } = [];
    
    /// <summary>
    /// Pagination - Thông tin phân trang (EN)<br/>
    /// Phân trang - Thông tin phân trang (VI)
    /// </summary>
    public PaginationInfo Pagination { get; set; } = new();
    
    /// <summary>
    /// Meta - Metadata (EN)<br/>
    /// Siêu dữ liệu (VI)
    /// </summary>
    public ResponseMeta Meta { get; set; } = new();
}

#endregion

#region Supporting DTOs

/// <summary>
/// DTO for API key security settings (EN)<br/>
/// DTO cho cài đặt bảo mật API key (VI)
/// </summary>
public class ApiKeySecuritySettingsDto
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

/// <summary>
/// DTO for API key usage log (EN)<br/>
/// DTO cho log sử dụng API key (VI)
/// </summary>
public class ApiKeyUsageLogDto
{
    /// <summary>
    /// ID - ID của log (EN)<br/>
    /// ID của log (VI)
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Timestamp - Thời gian yêu cầu (EN)<br/>
    /// Thời gian yêu cầu (VI)
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Method - Phương thức HTTP (EN)<br/>
    /// Phương thức HTTP (VI)
    /// </summary>
    public string Method { get; set; } = string.Empty;
    
    /// <summary>
    /// Endpoint - Endpoint API (EN)<br/>
    /// Endpoint API (VI)
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Status Code - Mã trạng thái HTTP (EN)<br/>
    /// Mã trạng thái HTTP (VI)
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Response Time - Thời gian phản hồi (ms) (EN)<br/>
    /// Thời gian phản hồi (ms) (VI)
    /// </summary>
    public int ResponseTime { get; set; }
    
    /// <summary>
    /// IP Address - Địa chỉ IP (EN)<br/>
    /// Địa chỉ IP (VI)
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// User Agent - User Agent (EN)<br/>
    /// User Agent (VI)
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Request Size - Kích thước yêu cầu (EN)<br/>
    /// Kích thước yêu cầu (VI)
    /// </summary>
    public long RequestSize { get; set; }
    
    /// <summary>
    /// Response Size - Kích thước phản hồi (EN)<br/>
    /// Kích thước phản hồi (VI)
    /// </summary>
    public long ResponseSize { get; set; }
    
    /// <summary>
    /// Error Message - Thông báo lỗi (EN)<br/>
    /// Thông báo lỗi (VI)
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Scopes Used - Các scope được sử dụng (EN)<br/>
    /// Các scope được sử dụng (VI)
    /// </summary>
    public List<string> ScopesUsed { get; set; } = [];
    
    /// <summary>
    /// Is Success - Có thành công không (EN)<br/>
    /// Có thành công không (VI)
    /// </summary>
    public bool IsSuccess { get; set; }
}

/// <summary>
/// Usage statistics aggregated data (EN)<br/>
/// Dữ liệu thống kê sử dụng tổng hợp (VI)
/// </summary>
public class UsageStatistics
{
    /// <summary>
    /// Total Requests - Tổng số yêu cầu (EN)<br/>
    /// Tổng số yêu cầu (VI)
    /// </summary>
    public long TotalRequests { get; set; }
    
    /// <summary>
    /// Successful Requests - Yêu cầu thành công (EN)<br/>
    /// Yêu cầu thành công (VI)
    /// </summary>
    public long SuccessfulRequests { get; set; }
    
    /// <summary>
    /// Failed Requests - Yêu cầu thất bại (EN)<br/>
    /// Yêu cầu thất bại (VI)
    /// </summary>
    public long FailedRequests { get; set; }
    
    /// <summary>
    /// Average Response Time - Thời gian phản hồi trung bình (EN)<br/>
    /// Thời gian phản hồi trung bình (VI)
    /// </summary>
    public double AverageResponseTime { get; set; }
    
    /// <summary>
    /// Requests Today - Yêu cầu hôm nay (EN)<br/>
    /// Yêu cầu hôm nay (VI)
    /// </summary>
    public int RequestsToday { get; set; }
    
    /// <summary>
    /// Requests This Week - Yêu cầu tuần này (EN)<br/>
    /// Yêu cầu tuần này (VI)
    /// </summary>
    public long RequestsThisWeek { get; set; }
    
    /// <summary>
    /// Requests This Month - Yêu cầu tháng này (EN)<br/>
    /// Yêu cầu tháng này (VI)
    /// </summary>
    public long RequestsThisMonth { get; set; }
    
    /// <summary>
    /// Most Used Endpoint - Endpoint được sử dụng nhiều nhất (EN)<br/>
    /// Endpoint được sử dụng nhiều nhất (VI)
    /// </summary>
    public string? MostUsedEndpoint { get; set; }
    
    /// <summary>
    /// Peak Usage Hour - Giờ sử dụng cao nhất (EN)<br/>
    /// Giờ sử dụng cao nhất (VI)
    /// </summary>
    public int? PeakUsageHour { get; set; }
}

/// <summary>
/// Usage data point for charts/analytics (EN)<br/>
/// Điểm dữ liệu sử dụng cho biểu đồ/phân tích (VI)
/// </summary>
public class UsageDataPoint
{
    /// <summary>
    /// Date - Ngày (EN)<br/>
    /// Ngày (VI)
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Request Count - Số lượng yêu cầu (EN)<br/>
    /// Số lượng yêu cầu (VI)
    /// </summary>
    public long RequestCount { get; set; }
    
    /// <summary>
    /// Error Count - Số lượng lỗi (EN)<br/>
    /// Số lượng lỗi (VI)
    /// </summary>
    public long ErrorCount { get; set; }
    
    /// <summary>
    /// Average Response Time - Thời gian phản hồi trung bình (EN)<br/>
    /// Thời gian phản hồi trung bình (VI)
    /// </summary>
    public double AverageResponseTime { get; set; }
}

/// <summary>
/// Pagination information (EN)<br/>
/// Thông tin phân trang (VI)
/// </summary>
public class PaginationInfo
{
    /// <summary>
    /// Next Cursor - Con trỏ tiếp theo (EN)<br/>
    /// Con trỏ tiếp theo (VI)
    /// </summary>
    public string? NextCursor { get; set; }
    
    /// <summary>
    /// Has More - Có thêm dữ liệu không (EN)<br/>
    /// Có thêm dữ liệu không (VI)
    /// </summary>
    public bool HasMore { get; set; }
    
    /// <summary>
    /// Limit - Giới hạn (EN)<br/>
    /// Giới hạn (VI)
    /// </summary>
    public int Limit { get; set; }
    
    /// <summary>
    /// Total Count - Tổng số (EN)<br/>
    /// Tổng số (VI)
    /// </summary>
    public long? TotalCount { get; set; }
}

/// <summary>
/// Response metadata (EN)<br/>
/// Siêu dữ liệu phản hồi (VI)
/// </summary>
public class ResponseMeta
{
    /// <summary>
    /// Timestamp - Thời gian phản hồi (EN)<br/>
    /// Thời gian phản hồi (VI)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Correlation ID - ID correlation (EN)<br/>
    /// ID correlation (VI)
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Request ID - ID yêu cầu (EN)<br/>
    /// ID yêu cầu (VI)
    /// </summary>
    public string? RequestId { get; set; }
    
    /// <summary>
    /// Processing Time - Thời gian xử lý (ms) (EN)<br/>
    /// Thời gian xử lý (ms) (VI)
    /// </summary>
    public double? ProcessingTime { get; set; }
}

#endregion 