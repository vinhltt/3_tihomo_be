using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Shared.EntityFramework.BaseEfModels;
// ReSharper disable InvokeAsExtensionMethod

namespace Shared.EntityFramework.EntityFrameworkUtilities;

public static class ExpressionUtils
{
    /// <summary>
    ///     (EN) Represents a constant expression with the value true.<br />
    ///     (VI) Biểu thị một biểu thức hằng số có giá trị true.
    /// </summary>
    public static readonly Expression TypeTrueExpression = Expression.Constant(true);

    /// <summary>
    ///     (EN) Represents a constant expression with the value false.<br />
    ///     (VI) Biểu thị một biểu thức hằng số có giá trị false.
    /// </summary>
    public static readonly Expression TypeFalseExpression = Expression.Constant(true);

    /// <summary>
    ///     (EN) Represents a constant expression with the value null.<br />
    ///     (VI) Biểu thị một biểu thức hằng số có giá trị null.
    /// </summary>
    public static readonly Expression TypeNullExpression = Expression.Constant(null);

    /// <summary>
    ///     (EN) Represents a constant expression with the value 0.<br />
    ///     (VI) Biểu thị một biểu thức hằng số có giá trị 0.
    /// </summary>
    public static readonly Expression TypeZeroExpression = Expression.Constant(0);

    /// <summary>
    ///     (EN) Represents a constant expression with an empty string value.<br />
    ///     (VI) Biểu thị một biểu thức hằng số có giá trị chuỗi rỗng.
    /// </summary>
    public static readonly Expression TypeStringEmptyExpression = Expression.Constant(0);

    /// <summary>
    ///     (EN) MethodInfo for the string.Trim() method.<br />
    ///     (VI) MethodInfo cho phương thức string.Trim().
    /// </summary>
    public static readonly MethodInfo? TypeTrimMethod = typeof(string).GetRuntimeMethod("Trim", []);

    /// <summary>
    ///     (EN) MethodInfo for the string.StartsWith(string) method.<br />
    ///     (VI) MethodInfo cho phương thức string.StartsWith(string).
    /// </summary>
    public static readonly MethodInfo? TypeStartsWithMethod =
        typeof(string).GetRuntimeMethod("StartsWith", [typeof(string)]);

    /// <summary>
    ///     (EN) MethodInfo for the string.EndsWith(string) method.<br />
    ///     (VI) MethodInfo cho phương thức string.EndsWith(string).
    /// </summary>
    public static readonly MethodInfo? TypeEndsWithMethod =
        typeof(string).GetRuntimeMethod("EndsWith", [typeof(string)]);

    /// <summary>
    ///     (EN) Creates a constant expression for the given value and type.<br />
    ///     (VI) Tạo một biểu thức hằng số cho giá trị và kiểu dữ liệu đã cho.
    /// </summary>
    /// <param name="value">The string value to convert. (EN)<br />Giá trị chuỗi cần chuyển đổi. (VI)</param>
    /// <param name="type">The target type of the constant expression. (EN)<br />Kiểu dữ liệu đích của biểu thức hằng số. (VI)</param>
    /// <returns>A ConstantExpression representing the converted value.</returns>
    public static ConstantExpression CreateConstantExpression(string value, Type type)
    {
        if (TypeList.IsString(type)) return Expression.Constant(value, type);

        if (TypeList.IsBoolean(type)) return Expression.Constant(bool.Parse(value), type);

        if (TypeList.IsDateTime(type)) return Expression.Constant(DateTime.Parse(value), type);

        if (TypeList.IsInt(type)) return Expression.Constant(int.Parse(value), type);

        if (TypeList.IsByte(type)) return Expression.Constant(byte.Parse(value), type);

        if (TypeList.IsLong(type)) return Expression.Constant(long.Parse(value), type);

        if (TypeList.IsLong(type)) return Expression.Constant(long.Parse(value), type);

        if (TypeList.IsShort(type)) return Expression.Constant(short.Parse(value), type);

        if (TypeList.IsGuid(type)) return Expression.Constant(Guid.Parse(value), type);

        if (TypeList.IsUnsignedShort(type)) return Expression.Constant(ushort.Parse(value), type);

        if (TypeList.IsUnsignedInt(type)) return Expression.Constant(uint.Parse(value), type);

        if (TypeList.IsDecimal(type)) return Expression.Constant(decimal.Parse(value), type);

        if (TypeList.IsTimeSpan(type)) return Expression.Constant(TimeSpan.Parse(value), type);

        if (TypeList.IsDouble(type)) return Expression.Constant(double.Parse(value), type);

        if (TypeList.IsTimeSpan(type)) return Expression.Constant(TimeSpan.Parse(value), type);

        if (TypeList.IsDateTimeOffset(type)) return Expression.Constant(DateTimeOffset.Parse(value), type);

        if (type == typeof(byte[])) return Expression.Constant(Convert.FromBase64String(value), type);

        return Expression.Constant(value, type);
    }

    /// <summary>
    ///     (EN) Creates a constant expression for an array of string values, attempting to parse them into the specified
    ///     target type.<br />
    ///     (VI) Tạo một biểu thức hằng số cho mảng các giá trị chuỗi, cố gắng phân tích cú pháp chúng thành kiểu đích được chỉ
    ///     định.
    /// </summary>
    /// <param name="values">The array of string values to convert. (EN)<br />Mảng các giá trị chuỗi cần chuyển đổi. (VI)</param>
    /// <param name="type">
    ///     The target type of the elements in the constant expression array. (EN)<br />Kiểu dữ liệu đích của
    ///     các phần tử trong mảng biểu thức hằng số. (VI)
    /// </param>
    /// <returns>A ConstantExpression representing the array of converted values.</returns>
    public static ConstantExpression CreateConstantExpression(string[] values, Type type)
    {
        if (TypeList.IsString(type))
            return Expression.Constant(values);
        if (TypeList.IsBoolean(type))
            return Expression.Constant(values.Select(bool.Parse).ToArray());
        if (TypeList.IsDateTime(type))
            return Expression.Constant(values.Select(DateTime.Parse).ToArray());
        if (TypeList.IsInt(type))
            return Expression.Constant(values.Select(int.Parse).ToArray());
        if (TypeList.IsByte(type))
            return Expression.Constant(values.Select(byte.Parse).ToArray());
        if (TypeList.IsLong(type))
            return Expression.Constant(values.Select(long.Parse).ToArray());
        if (TypeList.IsShort(type))
            return Expression.Constant(values.Select(short.Parse).ToArray());
        if (TypeList.IsGuid(type))
            return Expression.Constant(values.Select(Guid.Parse).ToArray());
        if (TypeList.IsUnsignedShort(type))
            return Expression.Constant(values.Select(ushort.Parse).ToArray());
        if (TypeList.IsUnsignedInt(type))
            return Expression.Constant(values.Select(uint.Parse).ToArray());
        if (TypeList.IsUnsignedLong(type))
            return Expression.Constant(values.Select(ulong.Parse).ToArray());
        if (TypeList.IsFloat(type))
            return Expression.Constant(values.Select(float.Parse).ToArray());
        if (TypeList.IsDouble(type))
            return Expression.Constant(values.Select(double.Parse).ToArray());
        if (TypeList.IsDecimal(type))
            return Expression.Constant(values.Select(decimal.Parse).ToArray());
        if (TypeList.IsTimeSpan(type))
            return Expression.Constant(values.Select(TimeSpan.Parse).ToArray());
        if (TypeList.IsDateTimeOffset(type))
            return Expression.Constant(values.Select(DateTimeOffset.Parse).ToArray());
        return Expression.Constant(values, type);
    }

    /// <summary>
    ///     Determines whether the given expression represents a null value. (EN)<br />
    ///     Xác định xem biểu thức đã cho có biểu thị giá trị null hay không. (VI)
    /// </summary>
    /// <param name="expression">The expression to check. (EN)<br />Biểu thức cần kiểm tra. (VI)</param>
    /// <returns>An Expression that evaluates to true if the expression is null; otherwise, false.</returns>
    public static Expression IsNull(Expression expression)
    {
        return Expression.Equal(expression, TypeNullExpression);
    }

    /// <summary>
    ///     Determines whether the given expression represents a non-null value. (EN)<br />
    ///     Xác định xem biểu thức đã cho có biểu thị giá trị khác null hay không. (VI)
    /// </summary>
    /// <param name="expression">The expression to check. (EN)<br />Biểu thức cần kiểm tra. (VI)</param>
    /// <returns>An Expression that evaluates to true if the expression is not null; otherwise, false.</returns>
    public static Expression IsNotNull(Expression expression)
    {
        return Expression.NotEqual(expression, TypeNullExpression);
    }

    /// <summary>
    ///     Determines whether the given expression represents an empty string. (EN)<br />
    ///     Xác định xem biểu thức đã cho có biểu thị chuỗi rỗng hay không. (VI)
    /// </summary>
    /// <param name="expression">The expression to check. (EN)<br />Biểu thức cần kiểm tra. (VI)</param>
    /// <returns>An Expression that evaluates to true if the expression is an empty string; otherwise, false.</returns>
    public static Expression IsEmpty(Expression expression)
    {
        return Expression.Equal(expression, TypeStringEmptyExpression);
    }

    /// <summary>
    ///     Determines whether the given expression represents a non-empty string. (EN)<br />
    ///     Xác định xem biểu thức đã cho có biểu thị chuỗi không rỗng hay không. (VI)
    /// </summary>
    /// <param name="expression">The expression to check. (EN)<br />Biểu thức cần kiểm tra. (VI)</param>
    /// <returns>An Expression that evaluates to true if the expression is a non-empty string; otherwise, false.</returns>
    public static Expression IsNotEmpty(Expression expression)
    {
        return Expression.NotEqual(expression, TypeStringEmptyExpression);
    }

    /// <summary>
    ///     Determines whether the given expression represents a null or empty string. (EN)<br />
    ///     Xác định xem biểu thức đã cho có biểu thị chuỗi null hoặc rỗng hay không. (VI)
    /// </summary>
    /// <param name="expression">The expression to check. (EN)<br />Biểu thức cần kiểm tra. (VI)</param>
    /// <returns>An Expression that evaluates to true if the expression is null or empty; otherwise, false.</returns>
    public static Expression IsNullOrEmpty(Expression expression)
    {
        return Expression.OrElse(IsNull(expression), IsEmpty(expression));
    }

    /// <summary>
    ///     Determines whether the given expression represents a non-null and non-empty string. (EN)<br />
    ///     Xác định xem biểu thức đã cho có biểu thị chuỗi không null và không rỗng hay không. (VI)
    /// </summary>
    /// <param name="expression">The expression to check. (EN)<br />Biểu thức cần kiểm tra. (VI)</param>
    /// <returns>An Expression that evaluates to true if the expression is not null and not empty; otherwise, false.</returns>
    public static Expression IsNotNullOrEmpty(Expression expression)
    {
        return Expression.AndAlso(IsNotNull(expression), IsNotEmpty(expression));
    }

    /// <summary>
    ///     Determines whether the given expression represents a null or white space string. (EN)<br />
    ///     Xác định xem biểu thức đã cho có biểu thị chuỗi null hoặc chỉ chứa khoảng trắng hay không. (VI)
    /// </summary>
    /// <param name="expression">The expression to check. (EN)<br />Biểu thức cần kiểm tra. (VI)</param>
    /// <returns>An Expression that evaluates to true if the expression is null or white space; otherwise, false.</returns>
    public static Expression IsNullOrWhiteSpace(Expression expression)
    {
        return Expression.OrElse(IsNull(expression),
            Expression.Equal(Expression.Call(expression, TypeTrimMethod!), TypeStringEmptyExpression));
    }

    /// <summary>
    ///     Determines whether the given expression represents a non-null and non-white space string. (EN)<br />
    ///     Xác định xem biểu thức đã cho có biểu thị chuỗi không null và không chứa khoảng trắng hay không. (VI)
    /// </summary>
    /// <param name="expression">The expression to check. (EN)<br />Biểu thức cần kiểm tra. (VI)</param>
    /// <returns>An Expression that evaluates to true if the expression is not null and not white space; otherwise, false.</returns>
    public static Expression IsNotNullOrWhiteSpace(Expression expression)
    {
        return Expression.AndAlso(IsNotNull(expression),
            Expression.NotEqual(Expression.Call(expression, TypeTrimMethod!), TypeStringEmptyExpression));
    }

    /// <summary>
    ///     Determines whether the left expression equals to right expression. (EN)<br />
    ///     Xác định xem biểu thức bên trái có bằng biểu thức bên phải hay không. (VI)
    /// </summary>
    /// <param name="left">The left expression. (EN)<br />Biểu thức bên trái. (VI)</param>
    /// <param name="right">The right expression. (EN)<br />Biểu thức bên phải. (VI)</param>
    /// <returns>An Expression that evaluates to true if the left expression equals the right expression; otherwise, false.</returns>
    public static Expression IsEqual(Expression left, Expression right)
    {
        if (Nullable.GetUnderlyingType(left.Type) == null && left.Type != typeof(string))
            return Expression.Equal(left, right);

        return Expression.AndAlso(IsNotNull(left), Expression.Equal(left, right));
    }

    /// <summary>
    ///     Determines whether the left expression is not equals to right expression. (EN)<br />
    ///     Xác định xem biểu thức bên trái có khác biểu thức bên phải hay không. (VI)
    /// </summary>
    /// <param name="left">The left expression. (EN)<br />Biểu thức bên trái. (VI)</param>
    /// <param name="right">The right expression. (EN)<br />Biểu thức bên phải. (VI)</param>
    /// <returns>
    ///     An Expression that evaluates to true if the left expression is not equal to the right expression; otherwise,
    ///     false.
    /// </returns>
    public static Expression IsNotEqual(Expression left, Expression right)
    {
        if (Nullable.GetUnderlyingType(left.Type) == null && left.Type != typeof(string))
            return Expression.NotEqual(left, right);

        return Expression.OrElse(IsNull(left), Expression.NotEqual(left, right));
    }

    /// <summary>
    ///     Determines whether the left expression starts with the right expression. (EN)<br />
    ///     Xác định xem biểu thức bên trái có bắt đầu bằng biểu thức bên phải hay không. (VI)
    /// </summary>
    /// <param name="left">The left expression (string). (EN)<br />Biểu thức bên trái (chuỗi). (VI)</param>
    /// <param name="right">The right expression (string). (EN)<br />Biểu thức bên phải (chuỗi). (VI)</param>
    /// <returns>
    ///     An Expression that evaluates to true if the left expression starts with the right expression; otherwise,
    ///     false.
    /// </returns>
    public static Expression IsStartsWith(Expression left, Expression right)
    {
        return Expression.AndAlso(IsNotNull(left), Expression.Call(left, TypeStartsWithMethod!, right));
    }

    /// <summary>
    ///     Determines whether the left expression ends with the right expression. (EN)<br />
    ///     Xác định xem biểu thức bên trái có kết thúc bằng biểu thức bên phải hay không. (VI)
    /// </summary>
    /// <param name="left">The left expression (string). (EN)<br />Biểu thức bên trái (chuỗi). (VI)</param>
    /// <param name="right">The right expression (string). (EN)<br />Biểu thức bên phải (chuỗi). (VI)</param>
    /// <returns>An Expression that evaluates to true if the left expression ends with the right expression; otherwise, false.</returns>
    public static Expression IsEndsWith(Expression left, Expression right)
    {
        return Expression.AndAlso(IsNotNull(left), Expression.Call(left, TypeEndsWithMethod!, right));
    }

    /// <summary>
    ///     Determines whether the left expression is greater than the right expression. (EN)<br />
    ///     Xác định xem biểu thức bên trái có lớn hơn biểu thức bên phải hay không. (VI)
    /// </summary>
    /// <param name="left">The left expression. (EN)<br />Biểu thức bên trái. (VI)</param>
    /// <param name="right">The right expression. (EN)<br />Biểu thức bên phải. (VI)</param>
    /// <returns>
    ///     An Expression that evaluates to true if the left expression is greater than the right expression; otherwise,
    ///     false.
    /// </returns>
    public static Expression IsGreaterThan(Expression left, Expression right)
    {
        if (TypeList.IsString(left.Type))
        {
            var compare = Expression.Call(typeof(string), "Compare", null, left, right);
            return Expression.GreaterThan(compare, TypeZeroExpression);
        }

        if (Nullable.GetUnderlyingType(left.Type) == null) return Expression.GreaterThan(left, right);

        return Expression.AndAlso(IsNotNull(left), Expression.GreaterThan(left, right));
    }

    /// <summary>
    ///     Determines whether the left expression is greater than or equals to right expression. (EN)<br />
    ///     Xác định xem biểu thức bên trái có lớn hơn hoặc bằng biểu thức bên phải hay không. (VI)
    /// </summary>
    /// <param name="left">The left expression. (EN)<br />Biểu thức bên trái. (VI)</param>
    /// <param name="right">The right expression. (EN)<br />Biểu thức bên phải. (VI)</param>
    /// <returns>
    ///     An Expression that evaluates to true if the left expression is greater than or equal to the right expression;
    ///     otherwise, false.
    /// </returns>
    public static Expression IsGreaterThanOrEqual(Expression left, Expression right)
    {
        if (TypeList.IsString(left.Type))
        {
            var compare = Expression.Call(typeof(string), "Compare", null, left, right);
            return Expression.GreaterThanOrEqual(compare, TypeZeroExpression);
        }

        if (Nullable.GetUnderlyingType(left.Type) == null) return Expression.GreaterThanOrEqual(left, right);

        return Expression.AndAlso(IsNotNull(left), Expression.GreaterThanOrEqual(left, right));
    }

    /// <summary>
    ///     Determines whether the left expression is less than right expression. (EN)<br />
    ///     Xác định xem biểu thức bên trái có nhỏ hơn biểu thức bên phải hay không. (VI)
    /// </summary>
    /// <param name="left">The left expression. (EN)<br />Biểu thức bên trái. (VI)</param>
    /// <param name="right">The right expression. (EN)<br />Biểu thức bên phải. (VI)</param>
    /// <returns>
    ///     An Expression that evaluates to true if the left expression is less than the right expression; otherwise,
    ///     false.
    /// </returns>
    public static Expression IsLessThan(Expression left, Expression right)
    {
        if (TypeList.IsString(left.Type))
        {
            var compare = Expression.Call(typeof(string), "Compare", null, left, right);
            return Expression.LessThan(compare, TypeZeroExpression);
        }

        if (Nullable.GetUnderlyingType(left.Type) == null) return Expression.LessThan(left, right);

        return Expression.AndAlso(IsNotNull(left), Expression.LessThan(left, right));
    }

    /// <summary>
    ///     Determines whether the left expression is less than or equals to right expression. (EN)<br />
    ///     Xác định xem biểu thức bên trái có nhỏ hơn hoặc bằng biểu thức bên phải hay không. (VI)
    /// </summary>
    /// <param name="left">The left expression. (EN)<br />Biểu thức bên trái. (VI)</param>
    /// <param name="right">The right expression. (EN)<br />Biểu thức bên phải. (VI)</param>
    /// <returns>
    ///     An Expression that evaluates to true if the left expression is less than or equal to the right expression;
    ///     otherwise, false.
    /// </returns>
    public static Expression IsLessThanOrEqual(Expression left, Expression right)
    {
        if (TypeList.IsString(left.Type))
        {
            var compare = Expression.Call(typeof(string), "Compare", null, left, right);
            return Expression.LessThanOrEqual(compare, TypeZeroExpression);
        }

        if (Nullable.GetUnderlyingType(left.Type) == null) return Expression.LessThanOrEqual(left, right);

        return Expression.AndAlso(IsNotNull(left), Expression.LessThanOrEqual(left, right));
    }

    /// <summary>
    ///     Determines whether the left expression contains the right expression. (EN)<br />
    ///     Xác định xem biểu thức bên trái có chứa biểu thức bên phải hay không (không phân biệt chữ hoa/thường đối với
    ///     chuỗi). (VI)
    /// </summary>
    /// <param name="left">The left expression. (EN)<br />Biểu thức bên trái. (VI)</param>
    /// <param name="right">The right expression. (EN)<br />Biểu thức bên phải. (VI)</param>
    /// <returns>An Expression that evaluates to true if the left expression contains the right expression; otherwise, false.</returns>
    public static Expression IsLike(Expression left, Expression right)
    {
        var contains = typeof(string).GetMethod("Contains", [typeof(string)]);

        return Expression.Call(left, contains!, right);
    }

    /// <summary>
    ///     Determines whether the left expression does not contain the right expression (case-insensitive for strings). (EN)
    ///     <br />
    ///     Xác định xem biểu thức bên trái có không chứa biểu thức bên phải hay không (không phân biệt chữ hoa/thường đối với
    ///     chuỗi). (VI)
    /// </summary>
    /// <param name="left">The left expression. (EN)<br />Biểu thức bên trái. (VI)</param>
    /// <param name="right">The right expression. (EN)<br />Biểu thức bên phải. (VI)</param>
    /// <returns>
    ///     An Expression that evaluates to true if the left expression does not contain the right expression; otherwise,
    ///     false.
    /// </returns>
    public static Expression IsNotLike(Expression left, Expression right)
    {
        var indexof = Expression.Call(left, "Indexof", null, right,
            Expression.Constant(StringComparison.OrdinalIgnoreCase));
        return Expression.Equal(indexof, Expression.Constant(-1));
    }

    /// <summary>
    ///     (EN) Determines whether the left expression contains the right expression. Supports checking if an array contains a
    ///     value.<br />
    ///     (VI) Xác định xem biểu thức bên trái có chứa biểu thức bên phải hay không. Hỗ trợ kiểm tra xem một mảng có chứa một
    ///     giá trị hay không.
    /// </summary>
    /// <param name="left">The left expression. (EN)<br />Biểu thức bên trái. (VI)</param>
    /// <param name="right">The right expression (can be an array). (EN)<br />Biểu thức bên phải (có thể là một mảng). (VI)</param>
    /// <returns>An Expression that evaluates to true if the left expression contains the right expression; otherwise, false.</returns>
    public static Expression IsContains(Expression left, Expression right)
    {
        if (right.Type.IsArray)
        {
            var constant = right as ConstantExpression;
            left = Expression.Convert(left, typeof(object));
            return Expression.Call(constant,
                typeof(IList).GetRuntimeMethod("Contains", [constant!.Value!.GetType().GetElementType()!]!)!,
                left);
        }

        return Expression.Call(left, typeof(string).GetRuntimeMethod("Contains", [right.Type])!, right);
    }

    /// <summary>
    ///     Determines whether the left expression is not contained in the right expression. (EN)<br />
    ///     Xác định xem biểu thức bên trái có không nằm trong biểu thức bên phải hay không. (VI)
    /// </summary>
    /// <param name="left">The left expression. (EN)<br />Biểu thức bên trái. (VI)</param>
    /// <param name="right">The right expression (should be an array). (EN)<br />Biểu thức bên phải (nên là một mảng). (VI)</param>
    /// <returns>An Expression that evaluates to true if the left expression is not in the right expression; otherwise, false.</returns>
    public static Expression IsNotContains(Expression left, Expression right)
    {
        return Expression.Not(Expression.Call(left, typeof(string).GetRuntimeMethod("Contains", [right.Type])!,
            right));
    }

    /// <summary>
    ///     Determines whether the left expression is in the right expression. (EN)<br />
    ///     Xác định xem biểu thức bên trái có nằm trong biểu thức bên phải hay không. (VI)
    /// </summary>
    /// <param name="left">The left expression. (EN)<br />Biểu thức bên trái. (VI)</param>
    /// <param name="right">The right expression (should be an array). (EN)<br />Biểu thức bên phải (nên là một mảng). (VI)</param>
    /// <returns>An Expression that evaluates to true if the left expression is in the right expression; otherwise, false.</returns>
    public static Expression IsIn(Expression left, Expression right)
    {
        if (right.Type.IsArray)
        {
            var constant = (ConstantExpression)right;
            left = Expression.Convert(left, typeof(object));
            return Expression.Call(constant,
                typeof(IList).GetRuntimeMethod("Contains", [constant.Value!.GetType().GetElementType()!]!)!,
                left);
        }

        return Expression.Call(left, typeof(string).GetRuntimeMethod("Contains", [right.Type])!, right);
    }

    /// <summary>
    ///     Determines whether the left expression is not in the right expression. (EN)<br />
    ///     Xác định xem biểu thức bên trái có không nằm trong biểu thức bên phải hay không. (VI)
    /// </summary>
    /// <param name="left">The left expression. (EN)<br />Biểu thức bên trái. (VI)</param>
    /// <param name="right">The right expression (should be an array). (EN)<br />Biểu thức bên phải (nên là một mảng). (VI)</param>
    /// <returns>An Expression that evaluates to true if the left expression is not in the right expression; otherwise, false.</returns>
    public static Expression IsNotIn(Expression left, Expression right)
    {
        if (right.Type.IsArray)
        {
            var constant = right as ConstantExpression;
            left = Expression.Convert(left, typeof(object));
            return Expression.Not(Expression.Call(constant,
                typeof(IList).GetRuntimeMethod("Contains", [constant!.Value!.GetType().GetElementType()!]!)!,
                left));
        }

        return Expression.Not(Expression.Call(left, typeof(string).GetRuntimeMethod("Contains", [right.Type])!,
            right));
    }

    /// <summary>
    ///     Determines whether the expression's value is between value1 and value2 (inclusive). (EN)<br />
    ///     Xác định xem giá trị của biểu thức có nằm giữa value1 và value2 hay không (bao gồm cả hai đầu). (VI)
    /// </summary>
    /// <param name="expression">The expression to check. (EN)<br />Biểu thức cần kiểm tra. (VI)</param>
    /// <param name="value1">The lower bound expression. (EN)<br />Biểu thức giới hạn dưới. (VI)</param>
    /// <param name="value2">The upper bound expression. (EN)<br />Biểu thức giới hạn trên. (VI)</param>
    /// <returns>An Expression that evaluates to true if the expression's value is between value1 and value2; otherwise, false.</returns>
    public static Expression IsBetween(Expression expression, Expression value1, Expression value2)
    {
        if (Nullable.GetUnderlyingType(expression.Type) == null)
            return Expression.AndAlso(Expression.GreaterThanOrEqual(expression, value1),
                Expression.LessThanOrEqual(expression, value2));

        return Expression.AndAlso(IsNotNull(expression),
            Expression.AndAlso(Expression.GreaterThanOrEqual(expression, value1),
                Expression.LessThanOrEqual(expression, value2)));
    }

    /// <summary>
    ///     (EN) Gets the MemberExpression for a property name from a parameter expression.<br />
    ///     (VI) Lấy MemberExpression cho tên thuộc tính từ một biểu thức tham số.
    /// </summary>
    /// <param name="parameter">The parameter expression. (EN)<br />Biểu thức tham số. (VI)</param>
    /// <param name="propertyName">The name of the property. (EN)<br />Tên của thuộc tính. (VI)</param>
    /// <returns>The MemberExpression for the property, or null if not found.</returns>
    public static Expression GetMemberExpression(Expression parameter, string? propertyName)
    {
        ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));
        ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));
        const char dot = '.';
        while (true)
        {
            if (!propertyName.Contains(dot))
                return Expression.Property(parameter, propertyName);
            var dotIndex = propertyName.IndexOf(dot);
            parameter = Expression.Property(parameter, propertyName[..dotIndex]);
            propertyName = propertyName[(dotIndex + 1)..];
        }
    }

    /// <summary>
    ///     (EN) Gets the property name from a MemberExpression.<br />
    ///     (VI) Lấy tên thuộc tính từ một MemberExpression.
    /// </summary>
    /// <param name="expression">The expression to get the property name from. (EN)<br />Biểu thức để lấy tên thuộc tính. (VI)</param>
    /// <returns>The property name, or null if the expression is not a MemberExpression.</returns>
    public static string? GetPropertyName(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        IList<string?> propertyNames = new List<string?>();
        var exp = expression;
        while (true)
            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    exp = ((LambdaExpression)exp).Body; break;
                case ExpressionType.MemberAccess:
                    var memberExpression = exp as MemberExpression;
                    var propertyInfo = memberExpression!.Member as PropertyInfo;
                    propertyNames.Add(propertyInfo?.Name);
                    if (memberExpression.Expression?.NodeType == ExpressionType.Parameter)
                        return string.Join('.', propertyNames.Reverse());
                    var parameter = GetParameterExpression(memberExpression.Expression!);
                    if (parameter == null) return string.Join('.', propertyNames.Reverse());
                    exp = Expression.Lambda(memberExpression.Expression!, parameter);
                    break;

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.ArrayLength:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Call:
                case ExpressionType.Coalesce:
                case ExpressionType.Conditional:
                case ExpressionType.Constant:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Invoke:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.ListInit:
                case ExpressionType.MemberInit:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Negate:
                case ExpressionType.UnaryPlus:
                case ExpressionType.NegateChecked:
                case ExpressionType.New:
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                case ExpressionType.Not:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Parameter:
                case ExpressionType.Power:
                case ExpressionType.Quote:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.TypeAs:
                case ExpressionType.TypeIs:
                case ExpressionType.Assign:
                case ExpressionType.Block:
                case ExpressionType.DebugInfo:
                case ExpressionType.Decrement:
                case ExpressionType.Dynamic:
                case ExpressionType.Default:
                case ExpressionType.Extension:
                case ExpressionType.Goto:
                case ExpressionType.Increment:
                case ExpressionType.Index:
                case ExpressionType.Label:
                case ExpressionType.RuntimeVariables:
                case ExpressionType.Loop:
                case ExpressionType.Switch:
                case ExpressionType.Throw:
                case ExpressionType.Try:
                case ExpressionType.Unbox:
                case ExpressionType.AddAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.PostDecrementAssign:
                case ExpressionType.TypeEqual:
                case ExpressionType.OnesComplement:
                case ExpressionType.IsTrue:
                case ExpressionType.IsFalse:
                default:
                    return null;
            }
    }

    /// <summary>
    ///     (EN) Gets the ParameterExpression from an expression, if it exists.<br />
    ///     (VI) Lấy ParameterExpression từ một biểu thức, nếu tồn tại.
    /// </summary>
    /// <param name="expression">The expression to get the parameter from. (EN)<br />Biểu thức để lấy tham số. (VI)</param>
    /// <returns>The ParameterExpression, or null if not found.</returns>
    public static ParameterExpression? GetParameterExpression(Expression? expression)
    {
        while (expression?.NodeType == ExpressionType.MemberAccess)
            expression = (expression as MemberExpression)?.Expression;

        return expression?.NodeType == ExpressionType.Parameter ? expression as ParameterExpression : null;
    }
}