namespace CoreFinance.Api.Configuration;

/// <summary>
///     JWT authentication configuration settings (EN)<br/>
///     Cấu hình xác thực JWT (VI)
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>
    ///     JWT issuer (EN)<br/>
    ///     Nhà phát hành JWT (VI)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    ///     JWT audience (EN)<br/>
    ///     Đối tượng nhận JWT (VI)
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    ///     JWT secret key for signature validation (EN)<br/>
    ///     Khóa bí mật JWT để xác thực chữ ký (VI)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    ///     Token expiration time in minutes (EN)<br/>
    ///     Thời gian hết hạn token tính bằng phút (VI)
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    ///     Whether to validate the issuer (EN)<br/>
    ///     Có xác thực nhà phát hành hay không (VI)
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    ///     Whether to validate the audience (EN)<br/>
    ///     Có xác thực đối tượng nhận hay không (VI)
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    ///     Whether to validate the lifetime (EN)<br/>
    ///     Có xác thực thời gian sống hay không (VI)
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    ///     Whether to validate the issuer signing key (EN)<br/>
    ///     Có xác thực khóa ký của nhà phát hành hay không (VI)
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;
}