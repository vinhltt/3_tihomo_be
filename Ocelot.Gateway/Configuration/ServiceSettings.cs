namespace Ocelot.Gateway.Configuration;

/// <summary>
///     Consul service discovery configuration settings
/// </summary>
public class ConsulSettings
{
    public const string SectionName = "ConsulSettings";

    /// <summary>
    ///     Consul server address
    /// </summary>
    public string Address { get; set; } = "http://localhost:8500";

    /// <summary>
    ///     Service discovery polling interval in seconds
    /// </summary>
    public int PollingInterval { get; set; } = 30;

    /// <summary>
    ///     Whether to use Consul for service discovery
    /// </summary>
    public bool Enabled { get; set; } = false;
}

/// <summary>
///     CORS configuration settings
/// </summary>
public class CorsSettings
{
    public const string SectionName = "CorsSettings";

    /// <summary>
    ///     Policy name for CORS
    /// </summary>
    public string PolicyName { get; set; } = "AllowSpecificOrigins";

    /// <summary>
    ///     Allowed origins for CORS
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = [];

    /// <summary>
    ///     Allowed methods for CORS
    /// </summary>
    public List<string> AllowedMethods { get; set; } = ["GET", "POST", "PUT", "DELETE", "OPTIONS"];

    /// <summary>
    ///     Allowed headers for CORS
    /// </summary>
    public List<string> AllowedHeaders { get; set; } = ["*"];

    /// <summary>
    ///     Whether to allow credentials
    /// </summary>
    public bool AllowCredentials { get; set; } = true;
}