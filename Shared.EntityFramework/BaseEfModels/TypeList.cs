namespace Shared.EntityFramework.BaseEfModels;

/// <summary>
///     (EN) Provides predefined Type objects and helper methods for common data types.<br />
///     (VI) Cung cấp các đối tượng Type được định nghĩa trước và các phương thức hỗ trợ cho các kiểu dữ liệu thông dụng.
/// </summary>
public static class TypeList
{
    /// <summary>
    ///     (EN) The type of character.<br />
    ///     (VI) Kiểu ký tự.
    /// </summary>
    public static readonly Type TypeOfChar = typeof(char);

    /// <summary>
    ///     (EN) The type of nullable character.<br />
    ///     (VI) Kiểu ký tự có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableChar = typeof(char?);

    /// <summary>
    ///     (EN) The type of string.<br />
    ///     (VI) Kiểu chuỗi ký tự.
    /// </summary>
    public static readonly Type TypeOfString = typeof(string);

    /// <summary>
    ///     (EN) The type of boolean.<br />
    ///     (VI) Kiểu boolean.
    /// </summary>
    public static readonly Type TypeOfBoolean = typeof(bool);

    /// <summary>
    ///     (EN) The type of nullable boolean.<br />
    ///     (VI) Kiểu boolean có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableBoolean = typeof(bool?);

    /// <summary>
    ///     (EN) The type of byte.<br />
    ///     (VI) Kiểu byte.
    /// </summary>
    public static readonly Type TypeOfByte = typeof(byte);

    /// <summary>
    ///     (EN) The type of nullable byte.<br />
    ///     (VI) Kiểu byte có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableByte = typeof(byte?);

    /// <summary>
    ///     (EN) The type of short (Int16).<br />
    ///     (VI) Kiểu short (Int16).
    /// </summary>
    public static readonly Type TypeOfShort = typeof(short);

    /// <summary>
    ///     (EN) The type of nullable short (Int16).<br />
    ///     (VI) Kiểu short (Int16) có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableShort = typeof(short?);

    /// <summary>
    ///     (EN) The type of unsigned short (UInt16).<br />
    ///     (VI) Kiểu unsigned short (UInt16).
    /// </summary>
    public static readonly Type TypeOfUnsignedShort = typeof(ushort);

    /// <summary>
    ///     (EN) The type of nullable unsigned short (UInt16).<br />
    ///     (VI) Kiểu unsigned short (UInt16) có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableUnsignedShort = typeof(ushort?);

    /// <summary>
    ///     (EN) The type of int (Int32).<br />
    ///     (VI) Kiểu int (Int32).
    /// </summary>
    public static readonly Type TypeOfInt = typeof(int);

    /// <summary>
    ///     (EN) The type of nullable int (Int32).<br />
    ///     (VI) Kiểu int (Int32) có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableInt = typeof(int?);

    /// <summary>
    ///     (EN) The type of unsigned int (UInt32).<br />
    ///     (VI) Kiểu unsigned int (UInt32).
    /// </summary>
    public static readonly Type TypeOfUnsignedInt = typeof(uint);

    /// <summary>
    ///     (EN) The type of nullable unsigned int (UInt32).<br />
    ///     (VI) Kiểu unsigned int (UInt32) có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableUnsignedInt = typeof(uint?);

    /// <summary>
    ///     (EN) The type of long (Int64).<br />
    ///     (VI) Kiểu long (Int64).
    /// </summary>
    public static readonly Type TypeOfLong = typeof(long);

    /// <summary>
    ///     (EN) The type of nullable long (Int64).<br />
    ///     (VI) Kiểu long (Int64) có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableLong = typeof(long?);

    /// <summary>
    ///     (EN) The type of unsigned long (UInt64).<br />
    ///     (VI) Kiểu unsigned long (UInt64).
    /// </summary>
    public static readonly Type TypeOfUnsignedLong = typeof(ulong);

    /// <summary>
    ///     (EN) The type of nullable unsigned long (UInt64).<br />
    ///     (VI) Kiểu unsigned long (UInt64) có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableUnsignedLong = typeof(ulong?);

    /// <summary>
    ///     (EN) The type of float (Single).<br />
    ///     (VI) Kiểu float (Single).
    /// </summary>
    public static readonly Type TypeOfFloat = typeof(float);

    /// <summary>
    ///     (EN) The type of nullable float (Single).<br />
    ///     (VI) Kiểu float (Single) có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableFloat = typeof(float?);

    /// <summary>
    ///     (EN) The type of double.<br />
    ///     (VI) Kiểu double.
    /// </summary>
    public static readonly Type TypeOfDouble = typeof(double);

    /// <summary>
    ///     (EN) The type of nullable double.<br />
    ///     (VI) Kiểu double có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableDouble = typeof(double?);

    /// <summary>
    ///     (EN) The type of decimal.<br />
    ///     (VI) Kiểu decimal.
    /// </summary>
    public static readonly Type TypeOfDecimal = typeof(decimal);

    /// <summary>
    ///     (EN) The type of nullable decimal.<br />
    ///     (VI) Kiểu decimal có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableDecimal = typeof(decimal?);

    /// <summary>
    ///     (EN) The type of TimeSpan.<br />
    ///     (VI) Kiểu TimeSpan.
    /// </summary>
    public static readonly Type TypeOfTimeSpan = typeof(TimeSpan);

    /// <summary>
    ///     (EN) The type of nullable TimeSpan.<br />
    ///     (VI) Kiểu TimeSpan có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableTimeSpan = typeof(TimeSpan?);

    /// <summary>
    ///     (EN) The type of DateTime.<br />
    ///     (VI) Kiểu DateTime.
    /// </summary>
    public static readonly Type TypeOfDateTime = typeof(DateTime);

    /// <summary>
    ///     (EN) The type of nullable DateTime.<br />
    ///     (VI) Kiểu DateTime có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableDateTime = typeof(DateTime?);

    /// <summary>
    ///     (EN) The type of DateOnly.<br />
    ///     (VI) Kiểu DateOnly.
    /// </summary>
    public static readonly Type TypeOfDateOnly = typeof(DateOnly);

    /// <summary>
    ///     (EN) The type of nullable DateOnly.<br />
    ///     (VI) Kiểu DateOnly có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableDateOnly = typeof(DateOnly?);

    /// <summary>
    ///     (EN) The type of TimeOnly.<br />
    ///     (VI) Kiểu TimeOnly.
    /// </summary>
    public static readonly Type TypeOfTimeOnly = typeof(TimeOnly);

    /// <summary>
    ///     (EN) The type of nullable TimeOnly.<br />
    ///     (VI) Kiểu TimeOnly có thể null.
    /// </summary>
    public static readonly Type TypeOfNullableTimeOnly = typeof(TimeOnly?);

    /// <summary>
    ///     The type of date time offset (EN)<br />
    ///     Kiểu date time offset (VI)
    /// </summary>
    public static readonly Type TypeOfDateTimeOffset = typeof(DateTimeOffset);

    /// <summary>
    ///     The type of nullable date time offset (EN)<br />
    ///     Kiểu date time offset có thể null (VI)
    /// </summary>
    public static readonly Type TypeOfNullableDateTimeOffset = typeof(DateTimeOffset?);

    /// <summary>
    ///     The type of unique identifier (EN)<br />
    ///     Kiểu định danh duy nhất (VI)
    /// </summary>
    public static readonly Type TypeOfGuid = typeof(Guid);

    /// <summary>
    ///     The type of nullable unique identifier (EN)<br />
    ///     Kiểu định danh duy nhất có thể null (VI)
    /// </summary>
    public static readonly Type TypeOfNullableGuid = typeof(Guid?);

    /// <summary>
    ///     (EN) An array of all simple types.<br />
    ///     (VI) Mảng chứa tất cả các kiểu dữ liệu đơn giản.
    /// </summary>
    public static readonly Type[] TypeSimpleTypes =
    [
        TypeOfChar,
        TypeOfNullableChar,
        TypeOfString,
        TypeOfBoolean,
        TypeOfNullableBoolean,
        TypeOfByte,
        TypeOfNullableByte,
        TypeOfShort,
        TypeOfNullableShort,
        TypeOfUnsignedShort,
        TypeOfNullableUnsignedShort,
        TypeOfInt,
        TypeOfNullableInt,
        TypeOfUnsignedInt,
        TypeOfNullableUnsignedInt,
        TypeOfLong,
        TypeOfNullableLong,
        TypeOfUnsignedLong,
        TypeOfNullableUnsignedLong,
        TypeOfDecimal,
        TypeOfNullableDecimal,
        TypeOfDouble,
        TypeOfNullableDouble,
        TypeOfFloat,
        TypeOfNullableFloat,
        TypeOfTimeSpan,
        TypeOfNullableTimeSpan,
        TypeOfDateTime,
        TypeOfNullableDateTime,
        TypeOfDateTimeOffset,
        TypeOfNullableDateTimeOffset,
        TypeOfGuid,
        TypeOfNullableGuid,
        TypeOfDateOnly,
        TypeOfNullableDateOnly,
        TypeOfTimeOnly,
        TypeOfNullableTimeOnly
    ];

    /// <summary>
    ///     Determines whether the specified type is a simple type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu dữ liệu đơn giản hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is a simple type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu đơn giản; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsSimpleType(Type type)
    {
        if (TypeSimpleTypes.Any(t => t == type)) return true;

        var underlyingType = Nullable.GetUnderlyingType(type);
        return underlyingType != null && TypeSimpleTypes.Any(t => t == type);
    }

    /// <summary>
    ///     Determines whether the specified type is a boolean type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu boolean hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is a boolean type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu boolean; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsBoolean(this Type type)
    {
        return type == TypeOfBoolean || type == TypeOfNullableBoolean;
    }

    /// <summary>
    ///     Determines whether the specified type is a DateTime type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu DateTime hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is a DateTime type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu DateTime; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsDateTime(this Type type)
    {
        return type == TypeOfDateTime || type == TypeOfNullableDateTime;
    }

    /// <summary>
    ///     Determines whether the specified type is an integer type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu số nguyên hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is an integer type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu số nguyên; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsInt(this Type type)
    {
        return type == TypeOfInt || type == TypeOfNullableInt;
    }

    /// <summary>
    ///     Determines whether the specified type is a byte type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu byte hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is a byte type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu byte; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsByte(this Type type)
    {
        return type == TypeOfByte || type == TypeOfNullableByte;
    }

    /// <summary>
    ///     Determines whether the specified type is a long type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu long hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is a long type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu long; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsLong(this Type type)
    {
        return type == TypeOfLong || type == TypeOfNullableLong;
    }

    /// <summary>
    ///     Determines whether the specified type is a short type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu short hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is a short type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu short; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsShort(this Type type)
    {
        return type == TypeOfShort || type == TypeOfNullableShort;
    }

    /// <summary>
    ///     Determines whether the specified type is a Guid type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu Guid hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is a Guid type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu Guid; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsGuid(this Type type)
    {
        return type == TypeOfGuid || type == TypeOfNullableGuid;
    }

    /// <summary>
    ///     Determines whether the specified type is an unsigned short type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu unsigned short hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is an unsigned short type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu unsigned short; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsUnsignedShort(this Type type)
    {
        return type == TypeOfUnsignedShort || type == TypeOfNullableUnsignedShort;
    }

    /// <summary>
    ///     Determines whether the specified type is an unsigned integer type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu số nguyên không dấu hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is an unsigned integer type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu số nguyên không dấu; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsUnsignedInt(this Type type)
    {
        return type == TypeOfUnsignedInt || type == TypeOfNullableUnsignedInt;
    }

    /// <summary>
    ///     Determines whether the specified type is an unsigned long type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu unsigned long hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is an unsigned long type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu unsigned long; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsUnsignedLong(this Type type)
    {
        return type == TypeOfUnsignedLong || type == TypeOfNullableUnsignedLong;
    }

    /// <summary>
    ///     Determines whether the specified type is a float type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu float hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is a float type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu float; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsFloat(this Type type)
    {
        return type == TypeOfFloat || type == TypeOfNullableFloat;
    }

    /// <summary>
    ///     Determines whether the specified type is a double type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu double hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is a double type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu double; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsDouble(this Type type)
    {
        return type == TypeOfDouble || type == TypeOfNullableDouble;
    }

    /// <summary>
    ///     Determines whether the specified type is a decimal type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu decimal hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is a decimal type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu decimal; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsDecimal(this Type type)
    {
        return type == TypeOfDecimal || type == TypeOfNullableDecimal;
    }

    /// <summary>
    ///     Determines whether the specified type is a TimeSpan type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu TimeSpan hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is a TimeSpan type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu TimeSpan; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsTimeSpan(this Type type)
    {
        return type == TypeOfTimeSpan || type == TypeOfNullableTimeSpan;
    }

    /// <summary>
    ///     Determines whether the specified type is a DateTimeOffset type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu DateTimeOffset hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is a DateTimeOffset type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu DateTimeOffset; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsDateTimeOffset(this Type type)
    {
        return type == TypeOfDateTimeOffset || type == TypeOfNullableDateTimeOffset;
    }

    /// <summary>
    ///     Determines whether the specified type is a string type. (EN)<br />
    ///     Xác định xem kiểu được chỉ định có phải là kiểu chuỗi hay không. (VI)
    /// </summary>
    /// <param name="type">
    ///     The type to check. (EN)<br />
    ///     Kiểu cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the specified type is a string type; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu kiểu được chỉ định là kiểu chuỗi; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsString(this Type type)
    {
        return type == TypeOfString;
    }
}