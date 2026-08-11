namespace Shared.Contracts.ConfigurationOptions;

/// <summary>
///     Represents database settings options. (EN)<br />
///     Đại diện cho các tùy chọn cài đặt cơ sở dữ liệu. (VI)
/// </summary>
public class DbSettingOptions
{
    /// <summary>
    ///     Gets or sets a value indicating whether to enable detailed errors. (EN)<br />
    ///     Lấy hoặc đặt giá trị cho biết có bật lỗi chi tiết hay không. (VI)
    /// </summary>
    public bool? EnableDetailedErrors { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to enable sensitive data logging. (EN)<br />
    ///     Lấy hoặc đặt giá trị cho biết có bật ghi nhật ký dữ liệu nhạy cảm hay không. (VI)
    /// </summary>
    public bool? EnableSensitiveDataLogging { get; set; }
}