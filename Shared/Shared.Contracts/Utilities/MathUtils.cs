namespace Shared.Contracts.Utilities;

/// <summary>
///     Provides utility methods for mathematical operations. (EN)<br />
///     Cung cấp các phương thức tiện ích cho các phép toán học. (VI)
/// </summary>
public static class MathUtils
{
    /// <summary>
    ///     Calculates the sum of an array of nullable decimal values, ignoring null values. (EN)<br />
    ///     Tính tổng của một mảng các giá trị decimal nullable, bỏ qua các giá trị null. (VI)
    /// </summary>
    /// <param name="values">
    ///     An array of nullable decimal values. (EN)<br />
    ///     Một mảng các giá trị decimal nullable. (VI)
    /// </param>
    /// <returns>
    ///     The sum of the non-null values, or null if the input array is null. (EN)<br />
    ///     Tổng của các giá trị không null hoặc null nếu mảng đầu vào là null. (VI)
    /// </returns>
    public static decimal? SumDecimal(
        params decimal?[]? values)
    {
        return values?.Where(value => value.HasValue)
            .Sum(value => value!.Value);
    }

    /// <summary>
    ///     Converts a nullable decimal value to a decimal, returning 0 if the value is null. (EN)<br />
    ///     Chuyển đổi giá trị decimal nullable sang decimal, trả về 0 nếu giá trị là null. (VI)
    /// </summary>
    /// <param name="value">
    ///     The nullable decimal value. (EN)<br />
    ///     Giá trị decimal nullable. (VI)
    /// </param>
    /// <returns>
    ///     The decimal value, or 0 if the input is null. (EN)<br />
    ///     Giá trị decimal hoặc 0 nếu đầu vào là null. (VI)
    /// </returns>
    public static decimal NullToZero(
        this decimal? value)
    {
        return value ?? 0m;
    }

    /// <summary>
    ///     Converts a nullable integer value to an integer, returning 0 if the value is null. (EN)<br />
    ///     Chuyển đổi giá trị integer nullable sang integer, trả về 0 nếu giá trị là null. (VI)
    /// </summary>
    /// <param name="value">
    ///     The nullable integer value. (EN)<br />
    ///     Giá trị integer nullable. (VI)
    /// </param>
    /// <returns>
    ///     The integer value, or 0 if the input is null. (EN)<br />
    ///     Giá trị integer hoặc 0 nếu đầu vào là null. (VI)
    /// </returns>
    public static int NullToZero(
        this int? value)
    {
        return value ?? 0;
    }
}