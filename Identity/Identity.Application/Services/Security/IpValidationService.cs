using Microsoft.Extensions.Logging;
using System.Net;

namespace Identity.Application.Services.Security;

/// <summary>
/// IP Validation Service - Dịch vụ xác thực IP cho API keys (EN)<br/>
/// Dịch vụ xác thực IP cho khóa API (VI)
/// </summary>
/// <remarks>
/// Constructor for IpValidationService (EN)<br/>
/// Constructor cho IpValidationService (VI)
/// </remarks>
/// <param name="logger">Logger service (EN)<br/>Dịch vụ logger (VI)</param>
public class IpValidationService(ILogger<IpValidationService> logger) : IIpValidationService
{

    /// <summary>
    /// Validate client IP against whitelist (EN)<br/>
    /// Xác thực IP client với danh sách cho phép (VI)
    /// </summary>
    /// <param name="clientIp">Client IP address (EN)<br/>Địa chỉ IP client (VI)</param>
    /// <param name="ipWhitelist">List of allowed IPs/CIDR ranges (EN)<br/>Danh sách IP/CIDR được phép (VI)</param>
    /// <returns>True if IP is allowed (EN)<br/>True nếu IP được phép (VI)</returns>
    public bool IsIpAllowed(string? clientIp, List<string> ipWhitelist)
    {
        if (ipWhitelist == null || ipWhitelist.Count == 0)
        {
            // If no whitelist configured, allow all IPs
            return true;
        }

        if (string.IsNullOrEmpty(clientIp))
        {
            logger.LogWarning("Client IP is null or empty");
            return false;
        }

        try
        {
            var clientAddress = IPAddress.Parse(clientIp);
            
            foreach (var allowedIp in ipWhitelist)
            {
                if (string.IsNullOrWhiteSpace(allowedIp))
                    continue;

                if (IsIpInRange(clientAddress, allowedIp.Trim()))
                {
                    logger.LogDebug("Client IP {ClientIp} matched whitelist entry {AllowedIp}", clientIp, allowedIp);
                    return true;
                }
            }
            
            logger.LogWarning("Client IP {ClientIp} not found in whitelist", clientIp);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating IP {ClientIp} against whitelist", clientIp);
            return false;
        }
    }

    /// <summary>
    /// Check if IP address is in the given range (supports CIDR notation) (EN)<br/>
    /// Kiểm tra IP có trong phạm vi cho phép không (hỗ trợ CIDR notation) (VI)
    /// </summary>
    /// <param name="clientAddress">Client IP address (EN)<br/>Địa chỉ IP client (VI)</param>
    /// <param name="allowedRange">Allowed IP or CIDR range (EN)<br/>IP hoặc CIDR range được phép (VI)</param>
    /// <returns>True if IP is in range (EN)<br/>True nếu IP trong phạm vi (VI)</returns>
    private bool IsIpInRange(IPAddress clientAddress, string allowedRange)
    {
        try
        {
            // Check for exact IP match
            if (IPAddress.TryParse(allowedRange, out var exactIp))
            {
                return clientAddress.Equals(exactIp);
            }

            // Check for CIDR notation (e.g., 192.168.1.0/24)
            if (allowedRange.Contains('/'))
            {
                var parts = allowedRange.Split('/');
                if (parts.Length == 2 && 
                    IPAddress.TryParse(parts[0], out var networkAddress) && 
                    int.TryParse(parts[1], out var prefixLength))
                {
                    return IsIpInCidrRange(clientAddress, networkAddress, prefixLength);
                }
            }

            // Check for wildcard patterns (simple implementation)
            if (allowedRange.Contains('*'))
            {
                return IsIpMatchWildcard(clientAddress.ToString(), allowedRange);
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking IP range for {ClientAddress} against {AllowedRange}", 
                clientAddress, allowedRange);
            return false;
        }
    }

    /// <summary>
    /// Check if IP is in CIDR range (EN)<br/>
    /// Kiểm tra IP có trong CIDR range không (VI)
    /// </summary>
    /// <param name="clientAddress">Client IP address (EN)<br/>Địa chỉ IP client (VI)</param>
    /// <param name="networkAddress">Network address (EN)<br/>Địa chỉ mạng (VI)</param>
    /// <param name="prefixLength">CIDR prefix length (EN)<br/>Độ dài prefix CIDR (VI)</param>
    /// <returns>True if IP is in CIDR range (EN)<br/>True nếu IP trong CIDR range (VI)</returns>
    private bool IsIpInCidrRange(IPAddress clientAddress, IPAddress networkAddress, int prefixLength)
    {
        try
        {
            // Convert to byte arrays for comparison
            var clientBytes = clientAddress.GetAddressBytes();
            var networkBytes = networkAddress.GetAddressBytes();

            // Ensure both addresses are the same type (IPv4 or IPv6)
            if (clientBytes.Length != networkBytes.Length)
            {
                return false;
            }

            // Calculate number of bytes and bits to check
            var bytesToCheck = prefixLength / 8;
            var bitsToCheck = prefixLength % 8;

            // Check full bytes
            for (int i = 0; i < bytesToCheck; i++)
            {
                if (clientBytes[i] != networkBytes[i])
                {
                    return false;
                }
            }

            // Check remaining bits in the last partial byte
            if (bitsToCheck > 0 && bytesToCheck < clientBytes.Length)
            {
                var mask = (byte)(0xFF << (8 - bitsToCheck));
                var clientMasked = (byte)(clientBytes[bytesToCheck] & mask);
                var networkMasked = (byte)(networkBytes[bytesToCheck] & mask);
                
                if (clientMasked != networkMasked)
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking CIDR range for {ClientAddress} against {NetworkAddress}/{PrefixLength}", 
                clientAddress, networkAddress, prefixLength);
            return false;
        }
    }

    /// <summary>
    /// Check if IP matches wildcard pattern (EN)<br/>
    /// Kiểm tra IP có khớp với wildcard pattern không (VI)
    /// </summary>
    /// <param name="clientIp">Client IP string (EN)<br/>Chuỗi IP client (VI)</param>
    /// <param name="wildcardPattern">Wildcard pattern (EN)<br/>Mẫu wildcard (VI)</param>
    /// <returns>True if IP matches pattern (EN)<br/>True nếu IP khớp với mẫu (VI)</returns>
    private bool IsIpMatchWildcard(string clientIp, string wildcardPattern)
    {
        try
        {
            // Simple wildcard matching for patterns like 192.168.1.*
            var pattern = wildcardPattern.Replace(".", @"\.")
                                        .Replace("*", @"[0-9]+");
            
            return System.Text.RegularExpressions.Regex.IsMatch(clientIp, $"^{pattern}$");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error matching wildcard pattern {Pattern} for IP {ClientIp}", 
                wildcardPattern, clientIp);
            return false;
        }
    }

    /// <summary>
    /// Validate IP address format (EN)<br/>
    /// Xác thực định dạng địa chỉ IP (VI)
    /// </summary>
    /// <param name="ipAddress">IP address string (EN)<br/>Chuỗi địa chỉ IP (VI)</param>
    /// <returns>True if valid IP format (EN)<br/>True nếu định dạng IP hợp lệ (VI)</returns>
    public bool IsValidIpAddress(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        if (!IPAddress.TryParse(ipAddress, out var parsedIp))
            return false;

        // For IPv4, ensure the original string matches the parsed representation
        // This prevents accepting incomplete addresses like "192.168.1" (which becomes "192.168.1.0")
        if (parsedIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            // IPv4 must have exactly 4 octets separated by dots
            var parts = ipAddress.Split('.');
            if (parts.Length != 4)
                return false;

            // Each part must be a valid number 0-255
            foreach (var part in parts)
            {
                if (!int.TryParse(part, out var num) || num < 0 || num > 255)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Validate CIDR range format (EN)<br/>
    /// Xác thực định dạng CIDR range (VI)
    /// </summary>
    /// <param name="cidrRange">CIDR range string (EN)<br/>Chuỗi CIDR range (VI)</param>
    /// <returns>True if valid CIDR format (EN)<br/>True nếu định dạng CIDR hợp lệ (VI)</returns>
    public bool IsValidCidrRange(string? cidrRange)
    {
        if (string.IsNullOrWhiteSpace(cidrRange))
            return false;

        var parts = cidrRange.Split('/');
        if (parts.Length != 2)
            return false;

        if (!IPAddress.TryParse(parts[0], out var ipAddress))
            return false;

        if (!int.TryParse(parts[1], out var prefixLength))
            return false;

        // Validate prefix length based on IP version
        if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            // IPv4: 0-32
            return prefixLength >= 0 && prefixLength <= 32;
        }
        else if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            // IPv6: 0-128
            return prefixLength >= 0 && prefixLength <= 128;
        }

        return false;
    }

    /// <summary>
    /// Validate wildcard pattern format (EN)<br/>
    /// Xác thực định dạng wildcard pattern (VI)
    /// </summary>
    /// <param name="wildcardPattern">Wildcard pattern string (EN)<br/>Chuỗi wildcard pattern (VI)</param>
    /// <returns>True if valid wildcard pattern (EN)<br/>True nếu định dạng wildcard pattern hợp lệ (VI)</returns>
    private static bool IsValidWildcardPattern(string wildcardPattern)
    {
        if (string.IsNullOrWhiteSpace(wildcardPattern))
            return false;

        var parts = wildcardPattern.Split('.');
        
        // Must have exactly 4 parts for IPv4 wildcard
        if (parts.Length != 4)
            return false;

        foreach (var part in parts)
        {
            if (part == "*")
                continue; // Wildcard is valid
                
            // If not wildcard, must be valid number 0-255
            if (!int.TryParse(part, out var num) || num < 0 || num > 255)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Validate IP whitelist entries (EN)<br/>
    /// Xác thực các mục trong whitelist IP (VI)
    /// </summary>
    /// <param name="ipWhitelist">List of IP addresses/ranges (EN)<br/>Danh sách địa chỉ IP/ranges (VI)</param>
    /// <returns>Validation result with errors (EN)<br/>Kết quả xác thực với các lỗi (VI)</returns>
    public (bool IsValid, List<string> Errors) ValidateIpWhitelist(List<string>? ipWhitelist)
    {
        var errors = new List<string>();

        if (ipWhitelist == null || ipWhitelist.Count == 0)
        {
            return (true, errors); // Empty whitelist is valid (allows all IPs)
        }

        foreach (var entry in ipWhitelist)
        {
            if (string.IsNullOrWhiteSpace(entry))
            {
                errors.Add("Empty or whitespace IP entry found");
                continue;
            }

            var trimmedEntry = entry.Trim();
            
            // Check if it's a valid IP address
            if (IsValidIpAddress(trimmedEntry))
            {
                continue;
            }
            
            // Check if it's a valid CIDR range
            if (IsValidCidrRange(trimmedEntry))
            {
                continue;
            }
            
            // Check if it's a wildcard pattern
            if (trimmedEntry.Contains('*'))
            {
                if (IsValidWildcardPattern(trimmedEntry))
                {
                    continue;
                }
                else
                {
                    errors.Add($"Invalid wildcard pattern: {trimmedEntry}");
                    continue;
                }
            }
            
            errors.Add($"Invalid IP address or range format: {trimmedEntry}");
        }

        return (errors.Count == 0, errors);
    }
} 