namespace CoreFinance.Api.Infrastructures.ServicesExtensions;

/// <summary>
///     Provides extension methods for retrieving configuration options. (EN)<br />
///     Cung cấp các extension methods để truy xuất các tùy chọn cấu hình. (VI)
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    ///     Retrieves a configuration section and binds it to an options object of the specified type. (EN)<br />
    ///     Truy xuất một phần cấu hình và liên kết nó với một đối tượng tùy chọn thuộc kiểu được chỉ định. (VI)
    /// </summary>
    /// <param name="configuration">
    ///     The configuration instance. (EN)<br />
    ///     Thể hiện cấu hình. (VI)
    /// </param>
    /// <param name="sectionName">
    ///     The name of the configuration section. (EN)<br />
    ///     Tên của phần cấu hình. (VI)
    /// </param>
    /// <typeparam name="T">The type of the options object.</typeparam>
    /// <returns>
    ///     The options object bound to the configuration section, or the default value if the section is not found. (EN)<br />
    ///     Đối tượng tùy chọn được liên kết với phần cấu hình hoặc giá trị mặc định nếu không tìm thấy phần đó. (VI)
    /// </returns>
    public static T? GetOptions<T>(this IConfiguration configuration, string sectionName)
        where T : new()
    {
        return configuration.GetSection(sectionName).Get<T>();
    }
}