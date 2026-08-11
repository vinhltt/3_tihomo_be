using System.Text.RegularExpressions;

namespace Shared.Contracts.Utilities;

/// <summary>
///     Helper methods for string manipulation. (EN)<br />
///     Các phương thức hỗ trợ thao tác chuỗi ký tự. (VI)
/// </summary>
public static class StringHelper
{
    public static readonly string TypePascalCaseStringPattern = @"^_+";
    public static readonly string TypePascalCaseStringPatternReplace = @"([a-z0-9])(A-Z)";

    /// <summary>
    ///     Converts a PascalCase or camelCase string to snake_case. (EN)<br />
    ///     Chuyển đổi chuỗi từ PascalCase hoặc camelCase sang snake_case. (VI)
    /// </summary>
    /// <param name="value">
    ///     The input string. (EN)<br />
    ///     Chuỗi đầu vào. (VI)
    /// </param>
    /// <returns>
    ///     The snake_case string. (EN)<br />
    ///     Chuỗi snake_case. (VI)
    /// </returns>
    public static string ToSnakeCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        var startUnderScores = Regex.Match(value, TypePascalCaseStringPatternReplace);
        return startUnderScores + Regex.Replace(value, TypePascalCaseStringPatternReplace, "$1_$2").ToLower();
    }

    /// <summary>
    ///     Nullifies a string if it is null or empty after trimming. (EN)<br />
    ///     Trả về giá trị mặc định nếu chuỗi là null hoặc rỗng sau khi cắt khoảng trắng. (VI)
    /// </summary>
    /// <param name="value">
    ///     The input string. (EN)<br />
    ///     Chuỗi đầu vào. (VI)
    /// </param>
    /// <param name="defaultValue">
    ///     The default value to return if the string is null or empty. (EN)<br />
    ///     Giá trị mặc định trả về nếu chuỗi là null hoặc rỗng. (VI)
    /// </param>
    /// <param name="trim">
    ///     A boolean indicating whether to trim the string before checking for emptiness. (EN)<br />
    ///     Biến boolean cho biết có cắt khoảng trắng trước khi kiểm tra rỗng hay không. (VI)
    /// </param>
    /// <returns>
    ///     The nullified string or the default value. (EN)<br />
    ///     Chuỗi đã nullified hoặc giá trị mặc định. (VI)
    /// </returns>
    public static string Nullify(this string? value, string defaultValue = "", bool trim = true)
    {
        if (value == null) return defaultValue;

        if (trim) value = value.Trim();

        return value.Length == 0 ? defaultValue : value;
    }

    /// <summary>
    ///     Trims start, end, and middle spaces in a string. (EN)<br />
    ///     Cắt khoảng trắng ở đầu, cuối và giữa chuỗi. (VI)
    /// </summary>
    /// <param name="value">
    ///     The input string. (EN)<br />
    ///     Chuỗi đầu vào. (VI)
    /// </param>
    /// <returns>
    ///     The trimmed string. (EN)<br />
    ///     Chuỗi đã cắt khoảng trắng. (VI)
    /// </returns>
    public static string FullTrim(this string value)
    {
        return Regex.Replace(value, @"\s+", " ");
    }
}