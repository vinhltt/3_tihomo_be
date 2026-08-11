namespace Identity.Application.Services.Security;

/// <summary>
/// Interface for IP Validation Service - Interface cho dịch vụ xác thực IP (EN)<br/>
/// Interface cho dịch vụ xác thực IP (VI)
/// </summary>
public interface IIpValidationService
{
    /// <summary>
    /// Validate client IP against whitelist (EN)<br/>
    /// Xác thực IP client với danh sách cho phép (VI)
    /// </summary>
    /// <param name="clientIp">Client IP address (EN)<br/>Địa chỉ IP client (VI)</param>
    /// <param name="ipWhitelist">List of allowed IPs/CIDR ranges (EN)<br/>Danh sách IP/CIDR được phép (VI)</param>
    /// <returns>True if IP is allowed (EN)<br/>True nếu IP được phép (VI)</returns>
    bool IsIpAllowed(string clientIp, List<string> ipWhitelist);
    
    /// <summary>
    /// Validate IP address format (EN)<br/>
    /// Xác thực định dạng địa chỉ IP (VI)
    /// </summary>
    /// <param name="ipAddress">IP address string (EN)<br/>Chuỗi địa chỉ IP (VI)</param>
    /// <returns>True if valid IP format (EN)<br/>True nếu định dạng IP hợp lệ (VI)</returns>
    bool IsValidIpAddress(string ipAddress);
    
    /// <summary>
    /// Validate CIDR range format (EN)<br/>
    /// Xác thực định dạng CIDR range (VI)
    /// </summary>
    /// <param name="cidrRange">CIDR range string (EN)<br/>Chuỗi CIDR range (VI)</param>
    /// <returns>True if valid CIDR format (EN)<br/>True nếu định dạng CIDR hợp lệ (VI)</returns>
    bool IsValidCidrRange(string cidrRange);
    
    /// <summary>
    /// Validate IP whitelist entries (EN)<br/>
    /// Xác thực các mục trong whitelist IP (VI)
    /// </summary>
    /// <param name="ipWhitelist">List of IP addresses/ranges (EN)<br/>Danh sách địa chỉ IP/ranges (VI)</param>
    /// <returns>Validation result with errors (EN)<br/>Kết quả xác thực với các lỗi (VI)</returns>
    (bool IsValid, List<string> Errors) ValidateIpWhitelist(List<string> ipWhitelist);
} 