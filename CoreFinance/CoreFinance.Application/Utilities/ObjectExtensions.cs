using System.Collections;

namespace CoreFinance.Application.Utilities;

/// <summary>
///     Extension methods for object operations (EN)<br />
///     Các phương thức mở rộng cho thao tác object (VI)
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    ///     Checks if an object is null and throws ArgumentNullException if it is (EN)<br />
    ///     Kiểm tra object có null không và throw ArgumentNullException nếu có (VI)
    /// </summary>
    /// <typeparam name="T">Type of the object</typeparam>
    /// <param name="obj">Object to check</param>
    /// <param name="parameterName">Parameter name for exception</param>
    /// <returns>The non-null object</returns>
    /// <exception cref="ArgumentNullException">Thrown when obj is null</exception>
    public static T ThrowIfNull<T>(this T? obj, string? parameterName = null) where T : class
    {
        if (obj is null) throw new ArgumentNullException(parameterName ?? "value");
        return obj;
    }

    /// <summary>
    ///     Checks if an object is null and throws ArgumentNullException if it is (for nullable value types) (EN)<br />
    ///     Kiểm tra object có null không và throw ArgumentNullException nếu có (cho nullable value types) (VI)
    /// </summary>
    /// <typeparam name="T">Type of the value type</typeparam>
    /// <param name="obj">Object to check</param>
    /// <param name="parameterName">Parameter name for exception</param>
    /// <returns>The non-null value</returns>
    /// <exception cref="ArgumentNullException">Thrown when obj is null</exception>
    public static T ThrowIfNull<T>(this T? obj, string? parameterName = null) where T : struct
    {
        if (!obj.HasValue) throw new ArgumentNullException(parameterName ?? "value");
        return obj.Value;
    }

    /// <summary>
    ///     Converts object to string, returning empty string if null (EN)<br />
    ///     Chuyển object thành string, trả về empty string nếu null (VI)
    /// </summary>
    /// <param name="obj">Object to convert</param>
    /// <returns>String representation or empty string</returns>
    public static string ToStringOrEmpty(this object? obj)
    {
        return obj?.ToString() ?? string.Empty;
    }

    /// <summary>
    ///     Converts object to string, returning default value if null (EN)<br />
    ///     Chuyển object thành string, trả về giá trị mặc định nếu null (VI)
    /// </summary>
    /// <param name="obj">Object to convert</param>
    /// <param name="defaultValue">Default value to return if object is null</param>
    /// <returns>String representation or default value</returns>
    public static string ToStringOrDefault(this object? obj, string defaultValue)
    {
        return obj?.ToString() ?? defaultValue;
    }

    /// <summary>
    ///     Checks if object is not null (EN)<br />
    ///     Kiểm tra object có khác null không (VI)
    /// </summary>
    /// <param name="obj">Object to check</param>
    /// <returns>True if object is not null</returns>
    public static bool IsNotNull(this object? obj)
    {
        return obj is not null;
    }

    /// <summary>
    ///     Checks if object is null (EN)<br />
    ///     Kiểm tra object có null không (VI)
    /// </summary>
    /// <param name="obj">Object to check</param>
    /// <returns>True if object is null</returns>
    public static bool IsNull(this object? obj)
    {
        return obj is null;
    }

    /// <summary>
    ///     Executes action if object is not null (EN)<br />
    ///     Thực thi action nếu object không null (VI)
    /// </summary>
    /// <typeparam name="T">Type of the object</typeparam>
    /// <param name="obj">Object to check</param>
    /// <param name="action">Action to execute</param>
    public static void IfNotNull<T>(this T? obj, Action<T> action) where T : class
    {
        if (obj is not null) action(obj);
    }

    /// <summary>
    ///     Maps object to another type if not null, otherwise returns default (EN)<br />
    ///     Map object sang kiểu khác nếu không null, ngược lại trả về default (VI)
    /// </summary>
    /// <typeparam name="TInput">Input type</typeparam>
    /// <typeparam name="TOutput">Output type</typeparam>
    /// <param name="obj">Object to map</param>
    /// <param name="mapper">Mapping function</param>
    /// <returns>Mapped object or default</returns>
    public static TOutput? MapIfNotNull<TInput, TOutput>(this TInput? obj, Func<TInput, TOutput> mapper)
        where TInput : class
    {
        return obj is not null ? mapper(obj) : default;
    }

    /// <summary>
    ///     Returns the object if not null, otherwise returns the alternative (EN)<br />
    ///     Trả về object nếu không null, ngược lại trả về alternative (VI)
    /// </summary>
    /// <typeparam name="T">Type of the object</typeparam>
    /// <param name="obj">Object to check</param>
    /// <param name="alternative">Alternative value</param>
    /// <returns>Object or alternative</returns>
    public static T OrElse<T>(this T? obj, T alternative) where T : class
    {
        return obj ?? alternative;
    }

    /// <summary>
    ///     Returns the object if not null, otherwise returns the result of the factory function (EN)<br />
    ///     Trả về object nếu không null, ngược lại trả về kết quả của factory function (VI)
    /// </summary>
    /// <typeparam name="T">Type of the object</typeparam>
    /// <param name="obj">Object to check</param>
    /// <param name="factory">Factory function for alternative</param>
    /// <returns>Object or factory result</returns>
    public static T OrElse<T>(this T? obj, Func<T> factory) where T : class
    {
        return obj ?? factory();
    }

    /// <summary>
    ///     Safely converts object to string for logging/debugging purposes (EN)<br />
    ///     Chuyển đổi object thành string một cách an toàn cho mục đích logging/debugging (VI)
    /// </summary>
    /// <param name="obj">Object to convert</param>
    /// <returns>String representation suitable for logging</returns>
    public static string TryParseToString(this object? obj)
    {
        if (obj is null)
            return "null";

        try
        {
            // Handle collections differently
            if (obj is IEnumerable enumerable && obj is not string)
            {
                var items = new List<string>();
                foreach (var item in enumerable)
                {
                    items.Add(item?.ToString() ?? "null");
                    if (items.Count >= 10) // Limit to prevent huge logs
                    {
                        items.Add("...");
                        break;
                    }
                }

                return $"[{string.Join(", ", items)}]";
            }

            return obj.ToString() ?? "null";
        }
        catch (Exception)
        {
            // Fallback for objects that might throw during ToString()
            return $"<{obj.GetType().Name}>";
        }
    }
}