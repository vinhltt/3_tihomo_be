using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Shared.Contracts.Utilities;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.Enums;

namespace Shared.EntityFramework.EntityFrameworkUtilities;
// ReSharper disable All
#pragma warning disable CS8604 // Possible null reference argument.

internal class Criteria
{
    public string? Field { get; set; }
    public object? Value { get; set; }
    public FilterType Operator { get; set; }
    public FilterLogicalOperator LogicalOperator { get; set; }
}

public static class ExpressionBuilder
{
    /// <summary>
    ///     (EN) Builds a filter expression from a single filter descriptor.<br />
    ///     (VI) Xây dựng biểu thức lọc từ một mô tả lọc duy nhất.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to filter. (EN)<br />Kiểu dữ liệu của model cần lọc. (VI)</typeparam>
    /// <param name="filterDescriptor">The filter descriptor. (EN)<br />Mô tả lọc. (VI)</param>
    /// <param name="parameterName">
    ///     The name of the parameter in the lambda expression. (EN)<br />Tên của tham số trong biểu
    ///     thức lambda. (VI)
    /// </param>
    /// <returns>A lambda expression that can be used for filtering.</returns>
    public static Expression<Func<TModel, bool>> Build<TModel>(FilterDescriptor? filterDescriptor,
        string parameterName = "x")
        where TModel : class
    {
        var criteria = GetCriteria(filterDescriptor);

        var parameter = Expression.Parameter(typeof(TModel), parameterName);
        Expression? expression = null;

        if (filterDescriptor != null) expression = CreateExpression(parameter, criteria);

        return Expression.Lambda<Func<TModel, bool>>(expression ?? ExpressionUtils.TypeTrueExpression,
            parameter);
    }

    /// <summary>
    ///     (EN) Builds a filter expression from a collection of filter descriptors.<br />
    ///     (VI) Xây dựng biểu thức lọc từ một tập hợp các mô tả lọc.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to filter. (EN)<br />Kiểu dữ liệu của model cần lọc. (VI)</typeparam>
    /// <param name="filterDescriptors">The collection of filter descriptors. (EN)<br />Tập hợp các mô tả lọc. (VI)</param>
    /// <param name="parameterName">
    ///     The name of the parameter in the lambda expression. (EN)<br />Tên của tham số trong biểu
    ///     thức lambda. (VI)
    /// </param>
    /// <returns>A lambda expression that can be used for filtering.</returns>
    /// <exception cref="ArgumentNullException">Thrown if filterDescriptors is null.</exception>
    public static Expression<Func<TModel, bool>> Build<TModel>(
        IEnumerable<FilterDescriptor>? filterDescriptors, string parameterName = "x")
        where TModel : class
    {
        var parameter = Expression.Parameter(typeof(TModel), parameterName);

        Expression? expression = null;
        if (!(filterDescriptors ?? throw new ArgumentNullException(nameof(filterDescriptors)))
            .Any())
            return Expression.Lambda<Func<TModel, bool>>(
                expression ?? ExpressionUtils.TypeTrueExpression, parameter);

        foreach (var filterDescriptor in filterDescriptors)
        {
            var criteria = GetCriteria(filterDescriptor);

            var innerExpression = CreateExpression(parameter, criteria);
            if (expression == null)
                expression = innerExpression;
            else
                expression = filterDescriptor.LogicalOperator == FilterLogicalOperator.And
                    ? Expression.AndAlso(expression, innerExpression)
                    : Expression.OrElse(expression, innerExpression);
        }

        return Expression.Lambda<Func<TModel, bool>>(expression ?? ExpressionUtils.TypeTrueExpression,
            parameter);
    }

    /// <summary>
    ///     (EN) Creates an expression for a specific filter criterion.<br />
    ///     (VI) Tạo biểu thức cho một tiêu chí lọc cụ thể.
    /// </summary>
    /// <param name="parameter">The parameter expression. (EN)<br />Biểu thức tham số. (VI)</param>
    /// <param name="filterDescriptor">The criteria for filtering. (EN)<br />Tiêu chí lọc. (VI)</param>
    /// <returns>An Expression representing the filter criterion.</returns>
    /// <exception cref="ArgumentNullException">Thrown if parameter or filterDescriptor is null.</exception>
    /// <exception cref="NotSupportedException">Thrown if the filter operator is not supported for the value type.</exception>
    private static Expression? CreateExpression(Expression parameter, Criteria filterDescriptor)
    {
        ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));
        ArgumentNullException.ThrowIfNull(filterDescriptor, nameof(filterDescriptor));

        var member = ExpressionUtils.GetMemberExpression(parameter, filterDescriptor.Field);
        var value = Expression.Constant(filterDescriptor.Value) as Expression;
        var valueType = Nullable.GetUnderlyingType(value.Type) != null ? value.Type : member.Type;
        if (member.Type.IsEnum) return CreateExpressionForEnumType(member, value, filterDescriptor);
        switch (filterDescriptor.Operator)
        {
            case FilterType.IsNull:
                return ExpressionUtils.IsNull(member);

            case FilterType.IsEmpty:
                return ExpressionUtils.IsEmpty(member);

            case FilterType.IsNotNull:
                return ExpressionUtils.IsNotNull(member);

            case FilterType.IsNotEmpty:
                return ExpressionUtils.IsNotEmpty(member);

            case FilterType.IsNullOrWhiteSpace:
                return ExpressionUtils.IsNullOrWhiteSpace(member);

            case FilterType.IsNotNullOrWhiteSpace:
                return ExpressionUtils.IsNotNullOrWhiteSpace(member);

            case FilterType.Equal:
                value = ExpressionUtils.CreateConstantExpression(
                    filterDescriptor.Value?.ToString() ?? string.Empty, valueType);
                return ExpressionUtils.IsEqual(member, value);

            case FilterType.NotEqual:
                value = ExpressionUtils.CreateConstantExpression(
                    filterDescriptor.Value?.ToString() ?? string.Empty, valueType);
                return ExpressionUtils.IsNotEqual(member, value);

            case FilterType.StartsWith:
                value = ExpressionUtils.CreateConstantExpression(
                    filterDescriptor.Value?.ToString() ?? string.Empty, valueType);
                return ExpressionUtils.IsStartsWith(member, value);

            case FilterType.EndsWith:
                value = ExpressionUtils.CreateConstantExpression(
                    filterDescriptor.Value?.ToString() ?? string.Empty, valueType);
                return ExpressionUtils.IsEndsWith(member, value);

            case FilterType.GreaterThan:
                value = ExpressionUtils.CreateConstantExpression(
                    filterDescriptor.Value?.ToString() ?? string.Empty, valueType);
                return ExpressionUtils.IsGreaterThan(member, value);

            case FilterType.GreaterThanOrEqual:
                value = ExpressionUtils.CreateConstantExpression(
                    filterDescriptor.Value?.ToString() ?? string.Empty, valueType);
                return ExpressionUtils.IsGreaterThanOrEqual(member, value);

            case FilterType.LessThan:
                value = ExpressionUtils.CreateConstantExpression(
                    filterDescriptor.Value?.ToString() ?? string.Empty, valueType);
                return ExpressionUtils.IsLessThan(member, value);

            case FilterType.LessThanOrEqual:
                value = ExpressionUtils.CreateConstantExpression(
                    filterDescriptor.Value?.ToString() ?? string.Empty, valueType);
                return ExpressionUtils.IsLessThanOrEqual(member, value);

            case FilterType.Between:
                var betweenValues = filterDescriptor.Value as string[];
                var from = ExpressionUtils.CreateConstantExpression(betweenValues![0], valueType);
                var to = ExpressionUtils.CreateConstantExpression(betweenValues[1], valueType);
                return ExpressionUtils.IsBetween(member, from, to);

            case FilterType.Contains:
                var containsValues = filterDescriptor.Value as string[];
                if (containsValues != null && containsValues.Any())
                {
                    value = ExpressionUtils.CreateConstantExpression(containsValues[0], valueType);
                    return ExpressionUtils.IsLike(member, value);
                }

                value = ExpressionUtils.CreateConstantExpression(containsValues!, valueType);
                return ExpressionUtils.IsContains(member, value);

            case FilterType.NotContains:
                var notContainsValues = filterDescriptor.Value as string[];
                if (notContainsValues!.Length == 1)
                {
                    value = ExpressionUtils.CreateConstantExpression(notContainsValues[0],
                        valueType);
                    return ExpressionUtils.IsNotLike(member, value);
                }

                value = ExpressionUtils.CreateConstantExpression(notContainsValues, valueType);
                return ExpressionUtils.IsNotContains(member, value);

            case FilterType.In:
                if (filterDescriptor.Value is not string[] inValues)
                    throw new ArgumentNullException(nameof(filterDescriptor.Value));
                if (member.Type.GetInterfaces().All(t => t != typeof(IList)))
                {
                    value = ExpressionUtils.CreateConstantExpression(inValues, valueType);
                    return ExpressionUtils.IsIn(member, value);
                }

                var inElementType = member.Type.GenericTypeArguments[0];
                var containsMethod =
                    member.Type.GetRuntimeMethod("Contains", [inElementType]);
                if (containsMethod is null) throw new ArgumentNullException(nameof(member.Type));
                Expression? inExpression = null;
                foreach (var inValue in inValues)
                {
                    var containValue =
                        ExpressionUtils.CreateConstantExpression(inValue, inElementType);
                    var innerExpression = Expression.Call(member, containsMethod, containValue);

                    if (inExpression == null)
                        inExpression = innerExpression;
                    else
                        inExpression = Expression.OrElse(inExpression, innerExpression);
                }

                return inExpression;

            case FilterType.NotIn:
                var notInValues = filterDescriptor.Value as string[];
                if (member.Type.GetInterfaces().All(t => t != typeof(IList)))
                {
                    if (notInValues != null)
                        value = ExpressionUtils.CreateConstantExpression(notInValues, valueType);
                    return ExpressionUtils.IsNotIn(member, value);
                }

                var notInElementType = member.Type.GenericTypeArguments[0];
                var notInContainsMethod =
                    member.Type.GetRuntimeMethod("Contains", new[] { notInElementType });

                Expression? notInExpression = null;
                foreach (var notInValue in notInValues!)
                {
                    var notContainValue =
                        ExpressionUtils.CreateConstantExpression(notInValue, notInElementType);
                    var innerExpression =
                        Expression.Call(member, notInContainsMethod, notContainValue);

                    if (notInExpression == null)
                        notInExpression = innerExpression;
                    else
                        notInExpression = Expression.AndAlso(notInExpression, innerExpression);
                }

                return Expression.Not(notInExpression);
        }

        throw new NotSupportedException(
            $"{filterDescriptor.Operator.ToString()} is not supported by {value.Type} ");
    }

    /// <summary>
    ///     (EN) Creates an expression for filtering enum types.<br />
    ///     (VI) Tạo biểu thức để lọc các kiểu enum.
    /// </summary>
    /// <param name="member">
    ///     The member expression representing the enum property. (EN)<br />Biểu thức thành viên biểu thị
    ///     thuộc tính enum. (VI)
    /// </param>
    /// <param name="value">The value expression to filter by. (EN)<br />Biểu thức giá trị để lọc theo. (VI)</param>
    /// <param name="filterDescriptor">The criteria for filtering. (EN)<br />Tiêu chí lọc. (VI)</param>
    /// <returns>An Expression representing the filter criterion for enum types.</returns>
    /// <exception cref="NotSupportedException">Thrown if the filter operator is not supported for enum types.</exception>
    private static Expression CreateExpressionForEnumType(Expression member, Expression value,
        Criteria filterDescriptor)
    {
        // Get actual enum type (handle nullable enums)
        // Lấy kiểu enum thực tế (xử lý nullable enums)
        var actualEnumType = Nullable.GetUnderlyingType(member.Type) ?? member.Type;
        if (!actualEnumType.IsEnum)
            throw new ArgumentException($"Member type {member.Type} is not an enum or nullable enum");

        switch (filterDescriptor.Operator)
        {
            case FilterType.Equal:
                return CreateEnumEqualExpression(member, actualEnumType, filterDescriptor.Value, false);

            case FilterType.NotEqual:
                return CreateEnumEqualExpression(member, actualEnumType, filterDescriptor.Value, true);

            case FilterType.In:
            case FilterType.Contains:
                return CreateEnumInExpression(member, actualEnumType, filterDescriptor.Value, false);

            case FilterType.NotIn:
            case FilterType.NotContains:
                return CreateEnumInExpression(member, actualEnumType, filterDescriptor.Value, true);

            default:
                throw new NotSupportedException($"Operator {filterDescriptor.Operator} is not supported for enum types");
        }
    }

    /// <summary>
    ///     (EN) Creates an equal/not equal expression for enum filtering.<br />
    ///     (VI) Tạo biểu thức so sánh bằng/khác cho việc lọc enum.
    /// </summary>
    private static Expression CreateEnumEqualExpression(Expression member, Type actualEnumType, object? value, bool negate)
    {
        var enumValue = ParseEnumValue(actualEnumType, value);
        var constantValue = Expression.Constant(enumValue, actualEnumType);

        Expression comparison;
        if (member.Type != actualEnumType) // Nullable enum case
        {
            var hasValue = Expression.Property(member, "HasValue");
            var getValue = Expression.Property(member, "Value");
            var equalComparison = Expression.Equal(getValue, constantValue);
            comparison = Expression.AndAlso(hasValue, equalComparison);
        }
        else // Direct enum case
        {
            comparison = Expression.Equal(member, constantValue);
        }

        return negate ? Expression.Not(comparison) : comparison;
    }

    /// <summary>
    ///     (EN) Creates an In/NotIn expression for enum filtering.<br />
    ///     (VI) Tạo biểu thức In/NotIn cho việc lọc enum.
    /// </summary>
    private static Expression CreateEnumInExpression(Expression member, Type actualEnumType, object? values, bool negate)
    {
        var stringValues = values as string[] ?? new[] { values?.ToString() ?? string.Empty };
        if (stringValues.Length == 0)
            return negate ? ExpressionUtils.TypeTrueExpression : ExpressionUtils.TypeFalseExpression;

        Expression? inExpression = null;
        foreach (var stringValue in stringValues)
        {
            var enumValue = ParseEnumValue(actualEnumType, stringValue);
            var constantValue = Expression.Constant(enumValue, actualEnumType);

            Expression comparison;
            if (member.Type != actualEnumType) // Nullable enum case
            {
                var hasValue = Expression.Property(member, "HasValue");
                var getValue = Expression.Property(member, "Value");
                comparison = Expression.AndAlso(hasValue, Expression.Equal(getValue, constantValue));
            }
            else // Direct enum case
            {
                comparison = Expression.Equal(member, constantValue);
            }

            inExpression = inExpression == null ? comparison : Expression.OrElse(inExpression, comparison);
        }

        return negate ? Expression.Not(inExpression!) : inExpression!;
    }

    /// <summary>
    ///     (EN) Parses a string value to enum.<br />
    ///     (VI) Chuyển đổi giá trị string thành enum.
    /// </summary>
    private static object ParseEnumValue(Type enumType, object? value)
    {
        var stringValue = value?.ToString();
        if (string.IsNullOrWhiteSpace(stringValue))
            throw new ArgumentException($"Cannot parse empty value to enum type {enumType.Name}");

        try
        {
            // Try parse as string name first
            // Thử parse như tên string trước
            if (Enum.TryParse(enumType, stringValue, true, out var enumValue))
                return enumValue;

            // Try parse as numeric value
            // Thử parse như giá trị số
            if (int.TryParse(stringValue, out var intValue))
                return Enum.ToObject(enumType, intValue);

            throw new ArgumentException($"Cannot parse '{stringValue}' to enum type {enumType.Name}");
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Cannot parse '{stringValue}' to enum type {enumType.Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    ///     (EN) Converts a FilterDescriptor object into a Criteria object for internal use.<br />
    ///     (VI) Chuyển đổi đối tượng FilterDescriptor thành đối tượng Criteria để sử dụng nội bộ.
    /// </summary>
    /// <param name="filter">The FilterDescriptor object. (EN)<br />Đối tượng FilterDescriptor. (VI)</param>
    /// <returns>A Criteria object representing the filter.</returns>
    /// <exception cref="ArgumentNullException">Thrown if filter is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the filter descriptor is invalid for the specified operator.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the filter operator is not recognized.</exception>
    private static Criteria GetCriteria(FilterDescriptor? filter)
    {
        ArgumentNullException.ThrowIfNull(filter, nameof(filter));

        var values = new List<string>();
        if (filter.Values is { Length: > 0 })
        {
            var a = filter.Values.Select(item => item.Nullify());
            var b = a.Where(value => !string.IsNullOrEmpty(value)).ToList();
            values.AddRange(b);
        }

        filter.Values = values.ToArray();

        switch (filter.Operator)
        {
            case FilterType.Equal:
            case FilterType.NotEqual:
            case FilterType.StartsWith:
            case FilterType.EndsWith:
            case FilterType.GreaterThan:
            case FilterType.GreaterThanOrEqual:
            case FilterType.LessThan:
            case FilterType.LessThanOrEqual:
                if (filter.Values.Length != 1) throw new ArgumentException();

                return new Criteria
                {
                    Field = filter.Field,
                    LogicalOperator = filter.LogicalOperator,
                    Operator = filter.Operator,
                    Value = filter.Values[0]
                };

            case FilterType.Contains:
            case FilterType.NotContains:
                if (filter.Values.Length == 0) throw new ArgumentException();

                return new Criteria
                {
                    Field = filter.Field,
                    LogicalOperator = filter.LogicalOperator,
                    Operator = filter.Operator,
                    Value = filter.Values
                };

            case FilterType.Between:
                if (filter.Values.Length != 2) throw new ArgumentException();

                return new Criteria
                {
                    Field = filter.Field,
                    LogicalOperator = filter.LogicalOperator,
                    Operator = filter.Operator,
                    Value = new[] { filter.Values[0], filter.Values[1] }
                };

            case FilterType.IsNull:
            case FilterType.IsEmpty:
            case FilterType.IsNotNull:
            case FilterType.IsNotEmpty:
            case FilterType.IsNullOrWhiteSpace:
            case FilterType.IsNotNullOrWhiteSpace:
                if (filter.Values.Length != 0) throw new ArgumentException();

                return new Criteria
                {
                    Field = filter.Field,
                    LogicalOperator = filter.LogicalOperator,
                    Operator = filter.Operator,
                    Value = null
                };

            case FilterType.In:
            case FilterType.NotIn:
                if (filter.Values.Length == 0) throw new ArgumentException();

                return new Criteria
                {
                    Field = filter.Field,
                    LogicalOperator = filter.LogicalOperator,
                    Operator = filter.Operator,
                    Value = filter.Values
                };
            case FilterType.NotBetween:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        throw new ArgumentException();
    }
}