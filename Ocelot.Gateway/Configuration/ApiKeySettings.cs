namespace Ocelot.Gateway.Configuration;

/// <summary>
///     API Key authentication configuration settings
/// </summary>
public class ApiKeySettings
{
    public const string SectionName = "ApiKeySettings";

    /// <summary>
    ///     Header name for API key
    /// </summary>
    public string HeaderName { get; set; } = "X-API-Key";

    /// <summary>
    ///     Valid API keys with their associated client names
    /// </summary>
    public Dictionary<string, string> ValidApiKeys { get; set; } = [];

    /// <summary>
    ///     Default rate limit for API key requests
    /// </summary>
    public int DefaultRateLimit { get; set; } = 1000;
}

/// <summary>
///     Rate limiting configuration settings
/// </summary>
public class RateLimitSettings
{
    public const string SectionName = "RateLimitSettings";

    /// <summary>
    ///     Whether rate limiting is enabled globally
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    ///     Default rate limit rules
    /// </summary>
    public RateLimitRule DefaultRule { get; set; } = new();

    /// <summary>
    ///     IP whitelist that bypasses rate limiting
    /// </summary>
    public List<string> IpWhitelist { get; set; } = [];
}

/// <summary>
///     Rate limiting rule configuration
/// </summary>
public class RateLimitRule
{
    /// <summary>
    ///     Time period for rate limiting (e.g., "1m", "1h", "1d")
    /// </summary>
    public string Period { get; set; } = "1m";

    /// <summary>
    ///     Time period in seconds
    /// </summary>
    public int PeriodTimespan { get; set; } = 60;

    /// <summary>
    ///     Maximum number of requests allowed in the period
    /// </summary>
    public int Limit { get; set; } = 100;
}