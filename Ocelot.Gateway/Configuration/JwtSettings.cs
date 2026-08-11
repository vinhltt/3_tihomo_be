namespace Ocelot.Gateway.Configuration;

/// <summary>
///     JWT authentication configuration settings
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>
    ///     JWT issuer
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    ///     JWT audience
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    ///     JWT secret key for signature validation
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    ///     Token expiration time in minutes
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    ///     Whether to validate the issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    ///     Whether to validate the audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    ///     Whether to validate the lifetime
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    ///     Whether to validate the issuer signing key
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;
}