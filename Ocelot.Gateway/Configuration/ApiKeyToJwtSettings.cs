namespace Ocelot.Gateway.Configuration;

/// <summary>
/// Configuration settings for API Key to JWT middleware
/// Cấu hình cho middleware chuyển đổi API Key thành JWT
/// </summary>
public class ApiKeyToJwtSettings
{
    public const string SectionName = "ApiKeyToJwtSettings";
    
    /// <summary>
    /// Paths that should be processed by the middleware
    /// Các đường dẫn cần được xử lý bởi middleware
    /// </summary>
    public List<string> ProcessedPaths { get; set; } = new() { "/api/" };
    
    /// <summary>
    /// Paths that should be skipped by the middleware
    /// Các đường dẫn bỏ qua bởi middleware
    /// </summary>
    public List<string> SkippedPaths { get; set; } = new() { "/health", "/metrics", "/swagger" };
    
    /// <summary>
    /// Timeout for Identity service calls in milliseconds
    /// Timeout cho các cuộc gọi đến Identity service (milliseconds)
    /// </summary>
    public int TimeoutMs { get; set; } = 5000;
    
    /// <summary>
    /// Whether to include detailed error information in responses
    /// Có bao gồm thông tin lỗi chi tiết trong response không
    /// </summary>
    public bool IncludeDetailedErrors { get; set; } = false;
}