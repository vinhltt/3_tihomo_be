namespace Shared.Contracts.ConfigurationOptions;

/// <summary>
///     Represents CORS (Cross-Origin Resource Sharing) options. (EN)<br />
///     Đại diện cho các tùy chọn CORS (Chia sẻ tài nguyên giữa các nguồn gốc khác nhau). (VI)
/// </summary>
public class CorsOptions
{
    /// <summary>
    ///     Gets or sets the name of the CORS policy. (EN)<br />
    ///     Lấy hoặc đặt tên của chính sách CORS. (VI)
    /// </summary>
    public string PolicyName { get; set; } = "";

    /// <summary>
    ///     Gets or sets the allowed origins for the CORS policy. (EN)<br />
    ///     Lấy hoặc đặt các nguồn gốc được phép cho chính sách CORS. (VI)
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>
    ///     Gets or sets the allowed HTTP methods for the CORS policy. (EN)<br />
    ///     Lấy hoặc đặt các phương thức HTTP được phép cho chính sách CORS. (VI)
    /// </summary>
    public string[] AllowedMethods { get; set; } = [];

    /// <summary>
    ///     Gets or sets the allowed headers for the CORS policy. (EN)<br />
    ///     Lấy hoặc đặt các header được phép cho chính sách CORS. (VI)
    /// </summary>
    public string[] AllowedHeaders { get; set; } = [];

    /// <summary>
    ///     Gets or sets the exposed headers for the CORS policy. (EN)<br />
    ///     Lấy hoặc đặt các header được hiển thị cho chính sách CORS. (VI)
    /// </summary>
    public string[] ExposedHeaders { get; set; } = [];

    /// <summary>
    ///     Gets or sets the preflight max age in minutes for the CORS policy. (EN)<br />
    ///     Lấy hoặc đặt thời gian tối đa cho yêu cầu preflight tính bằng phút cho chính sách CORS. (VI)
    /// </summary>
    public string PreflightMaxAgeInMinutes { get; set; } = "";
}