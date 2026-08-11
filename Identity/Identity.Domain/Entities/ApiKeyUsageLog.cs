using System.ComponentModel.DataAnnotations;
using Shared.EntityFramework.BaseEfModels;

namespace Identity.Domain.Entities;

/// <summary>
/// API Key Usage Log - Log sử dụng API key cho analytics và audit (EN)<br/>
/// Log sử dụng khóa API cho phân tích và kiểm tra (VI)
/// </summary>
public class ApiKeyUsageLog : BaseEntity<Guid>
{
    /// <summary>
    /// API Key ID - ID của API key (EN)<br/>
    /// ID của khóa API (VI)
    /// </summary>
    [Required]
    public Guid ApiKeyId { get; set; }
    
    /// <summary>
    /// Timestamp - Thời gian yêu cầu (EN)<br/>
    /// Thời gian yêu cầu (VI)
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// HTTP Method - Phương thức HTTP (EN)<br/>
    /// Phương thức HTTP (VI)
    /// </summary>
    [Required]
    [StringLength(10)]
    public string Method { get; set; } = string.Empty;
    
    /// <summary>
    /// API Endpoint - Endpoint API được gọi (EN)<br/>
    /// Endpoint API được gọi (VI)
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Endpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// HTTP Status Code - Mã trạng thái HTTP (EN)<br/>
    /// Mã trạng thái HTTP (VI)
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Response Time - Thời gian phản hồi (milliseconds) (EN)<br/>
    /// Thời gian phản hồi (milliseconds) (VI)
    /// </summary>
    public int ResponseTime { get; set; }
    
    /// <summary>
    /// Client IP Address - Địa chỉ IP của client (EN)<br/>
    /// Địa chỉ IP của client (VI)
    /// </summary>
    [Required]
    [StringLength(45)] // IPv6 max length
    public string IpAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// User Agent - Chuỗi User Agent (EN)<br/>
    /// Chuỗi User Agent (VI)
    /// </summary>
    [StringLength(1000)]
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Request Size - Kích thước yêu cầu (bytes) (EN)<br/>
    /// Kích thước yêu cầu (bytes) (VI)
    /// </summary>
    public long RequestSize { get; set; }
    
    /// <summary>
    /// Response Size - Kích thước phản hồi (bytes) (EN)<br/>
    /// Kích thước phản hồi (bytes) (VI)
    /// </summary>
    public long ResponseSize { get; set; }
    
    /// <summary>
    /// Error Message - Thông báo lỗi (nếu có) (EN)<br/>
    /// Thông báo lỗi (nếu có) (VI)
    /// </summary>
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Request ID - ID yêu cầu để correlation (EN)<br/>
    /// ID yêu cầu để correlation (VI)
    /// </summary>
    [StringLength(100)]
    public string? RequestId { get; set; }
    
    /// <summary>
    /// Scopes Used - Các scope được sử dụng trong request (EN)<br/>
    /// Các scope được sử dụng trong request (VI)
    /// </summary>
    public List<string> ScopesUsed { get; set; } = [];
    
    /// <summary>
    /// Is Success - Có thành công hay không (EN)<br/>
    /// Có thành công hay không (VI)
    /// </summary>
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
    
    // Navigation properties
    /// <summary>
    /// API Key - Navigation property tới ApiKey (EN)<br/>
    /// Khóa API - Navigation property tới ApiKey (VI)
    /// </summary>
    public virtual ApiKey ApiKey { get; set; } = null!;
} 