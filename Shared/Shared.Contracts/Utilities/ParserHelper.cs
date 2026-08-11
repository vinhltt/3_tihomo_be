using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Shared.Contracts.Utilities;

/// <summary>
///     Provides helper methods for parsing and data conversion. (EN)<br />
///     Cung cấp các phương thức trợ giúp cho việc phân tích cú pháp và chuyển đổi dữ liệu. (VI)
/// </summary>
public static class ParserHelper
{
    /// <summary>
    ///     Regex pattern to match underscores at the beginning of a string. (EN)<br />
    ///     Mẫu Regex để khớp với các dấu gạch dưới ở đầu chuỗi. (VI)
    /// </summary>
    public static readonly string TypePascalCaseStringPattern = @"^_+";

    /// <summary>
    ///     Regex pattern to match lowercase letter followed by uppercase letter for snake_case conversion. (EN)<br />
    ///     Mẫu Regex để khớp với chữ thường theo sau là chữ hoa để chuyển đổi sang snake_case. (VI)
    /// </summary>
    public static readonly string TypePascalCaseStringPatternReplace = @"([a-z0-9])(A-Z)";

    /// <summary>
    ///     Converts a string from PascalCase or camelCase to snake_case. (EN)<br />
    ///     Chuyển đổi một chuỗi từ PascalCase hoặc camelCase sang snake_case. (VI)
    /// </summary>
    /// <param name="value">
    ///     The string to convert. (EN)<br />
    ///     Chuỗi cần chuyển đổi. (VI)
    /// </param>
    /// <returns>
    ///     The snake_case representation of the input string. (EN)<br />
    ///     Biểu diễn snake_case của chuỗi đầu vào. (VI)
    /// </returns>
    public static string ToSnakeCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        var startUnderScores = Regex.Match(value, TypePascalCaseStringPatternReplace);
        return startUnderScores + Regex.Replace(value, TypePascalCaseStringPatternReplace, "$1_$2").ToLower();
    }

    /// <summary>
    ///     Removes leading, trailing, and multiple internal spaces from a string. (EN)<br />
    ///     Xóa khoảng trắng ở đầu, cuối và nhiều khoảng trắng bên trong chuỗi. (VI)
    /// </summary>
    /// <param name="value">
    ///     The string to trim. (EN)<br />
    ///     Chuỗi cần cắt khoảng trắng. (VI)
    /// </param>
    /// <returns>
    ///     The trimmed string. (EN)<br />
    ///     Chuỗi sau khi cắt khoảng trắng. (VI)
    /// </returns>
    public static string FullTrim(this string value)
    {
        return Regex.Replace(value, @"\s+", " ");
    }

    /// <summary>
    ///     Parses a JSON string to an object of the specified type, returning a default instance of T on failure. (EN)<br />
    ///     Phân tích cú pháp chuỗi JSON thành đối tượng thuộc kiểu được chỉ định, trả về một thể hiện T mặc định khi thất bại.
    ///     (VI)
    /// </summary>
    /// <param name="data">
    ///     The JSON string to parse. (EN)<br />
    ///     Chuỗi JSON cần phân tích cú pháp. (VI)
    /// </param>
    /// <param name="result">
    ///     When this method returns, contains the parsed object if the parsing was successful, or a default value of T if the
    ///     parsing failed. (EN)<br />
    ///     Khi phương thức này trả về, chứa đối tượng được phân tích cú pháp nếu thành công hoặc giá trị mặc định của T nếu
    ///     thất bại. (VI)
    /// </param>
    /// <typeparam name="T">The type of the object to parse the JSON string into.</typeparam>
    public static void TryParse<T>(this string data, out T? result) where T : new()
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(data);
        }
        catch
        {
            result = new T();
        }
    }

    /// <summary>
    ///     Parses a JSON string to an object of the specified type. (EN)<br />
    ///     Phân tích cú pháp chuỗi JSON thành đối tượng thuộc kiểu được chỉ định. (VI)
    /// </summary>
    /// <param name="data">
    ///     The JSON string to parse. (EN)<br />
    ///     Chuỗi JSON cần phân tích cú pháp. (VI)
    /// </param>
    /// <typeparam name="T">The type of the object to parse the JSON string into.</typeparam>
    /// <returns>
    ///     The parsed object. (EN)<br />
    ///     Đối tượng được phân tích cú pháp. (VI)
    /// </returns>
    public static T? Parse<T>(this string data)
    {
        return JsonSerializer.Deserialize<T>(data);
    }

    /// <summary>
    ///     Serializes an object to a JSON string, returning an empty string on failure. (EN)<br />
    ///     Tuần tự hóa đối tượng thành chuỗi JSON, trả về chuỗi rỗng khi thất bại. (VI)
    /// </summary>
    /// <param name="data">
    ///     The object to serialize. (EN)<br />
    ///     Đối tượng cần tuần tự hóa. (VI)
    /// </param>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <returns>
    ///     The JSON string representation of the object, or an empty string if serialization failed. (EN)<br />
    ///     Biểu diễn chuỗi JSON của đối tượng hoặc chuỗi rỗng nếu tuần tự hóa thất bại. (VI)
    /// </returns>
    public static string TryParseToString<T>(this T? data)
    {
        var result = string.Empty;
        try
        {
            result = JsonSerializer.Serialize(data);
            return result;
        }
        catch
        {
            return result;
        }
    }

    /// <summary>
    ///     Serializes an object to a Base64 encoded JSON string, returning null on failure. (EN)<br />
    ///     Tuần tự hóa đối tượng thành chuỗi JSON mã hóa Base64, trả về null khi thất bại. (VI)
    /// </summary>
    /// <param name="data">
    ///     The object to serialize and encode. (EN)<br />
    ///     Đối tượng cần tuần tự hóa và mã hóa. (VI)
    /// </param>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <returns>
    ///     The Base64 encoded JSON string representation of the object, or null if serialization or encoding failed. (EN)
    ///     <br />
    ///     Biểu diễn chuỗi JSON mã hóa Base64 của đối tượng hoặc null nếu tuần tự hóa hoặc mã hóa thất bại. (VI)
    /// </returns>
    public static string? TryParseToBase64<T>(this T data)
    {
        try
        {
            var result = TryParseToString(data);
            return Base64Encode(result);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Encodes a string to a Base64 string. (EN)<br />
    ///     Mã hóa chuỗi thành chuỗi Base64. (VI)
    /// </summary>
    /// <param name="plainText">
    ///     The string to encode. (EN)<br />
    ///     Chuỗi cần mã hóa. (VI)
    /// </param>
    /// <returns>
    ///     The Base64 encoded string. (EN)<br />
    ///     Chuỗi đã được mã hóa Base64. (VI)
    /// </returns>
    public static string Base64Encode(this string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    /// <summary>
    ///     Decodes a Base64 encoded string to a plain text string. (EN)<br />
    ///     Giải mã chuỗi mã hóa Base64 thành chuỗi văn bản thuần túy. (VI)
    /// </summary>
    /// <param name="base64EncodedData">
    ///     The Base64 encoded string to decode. (EN)<br />
    ///     Chuỗi mã hóa Base64 cần giải mã. (VI)
    /// </param>
    /// <returns>
    ///     The decoded plain text string. (EN)<br />
    ///     Chuỗi văn bản thuần túy đã được giải mã. (VI)
    /// </returns>
    public static string Base64Decode(this string base64EncodedData)
    {
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }
}