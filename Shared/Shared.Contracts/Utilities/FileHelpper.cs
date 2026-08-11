using System.Reflection;

namespace Shared.Contracts.Utilities;

/// <summary>
///     Provides helper methods for file operations. (EN)<br />
///     Cung cấp các phương thức trợ giúp cho các thao tác tệp. (VI)
/// </summary>
public class FileHelper
{
    /// <summary>
    ///     Reads the content of a file asynchronously. (EN)<br />
    ///     Đọc nội dung của một tệp một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="filepath">
    ///     The full path to the file. (EN)<br />
    ///     Đường dẫn đầy đủ đến tệp. (VI)
    /// </param>
    /// <returns>
    ///     The content of the file as a string. (EN)<br />
    ///     Nội dung của tệp dưới dạng chuỗi. (VI)
    /// </returns>
    /// <exception cref="System.IO.FileNotFoundException">Thrown when the specified file is not found.</exception>
    public static async Task<string> ReadFile(
        string filepath)
    {
        if (!File.Exists(filepath))
            throw new FileNotFoundException(filepath);

        var result = await File.ReadAllTextAsync(filepath);

        return result;
    }

    /// <summary>
    ///     Gets the directory path of the executing application. (EN)<br />
    ///     Lấy đường dẫn thư mục của ứng dụng đang thực thi. (VI)
    /// </summary>
    /// <returns>
    ///     The directory path of the application. (EN)<br />
    ///     Đường dẫn thư mục của ứng dụng. (VI)
    /// </returns>
    /// <exception cref="System.Exception">Thrown when the application folder cannot be determined.</exception>
    public static string GetApplicationFolder()
    {
        var path = Assembly.GetExecutingAssembly()
            .Location;

        return Path.GetDirectoryName(path)
               ?? throw new Exception(
                   "FolderNotFoundException");
    }
}