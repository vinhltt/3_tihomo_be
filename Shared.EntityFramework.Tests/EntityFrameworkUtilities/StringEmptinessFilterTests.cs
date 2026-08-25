using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.Enums;
using Shared.EntityFramework.EntityFrameworkUtilities;

namespace Shared.EntityFramework.Tests.EntityFrameworkUtilities;

/// <summary>
///     Regression coverage for the four string-emptiness filter operators.
///
///     These four routed through <c>ExpressionUtils.TypeStringEmptyExpression</c>, which was declared as
///     <c>Expression.Constant(0)</c> — an int — while its doc comment said "empty string value". Every one of
///     them built <c>Expression.Equal(stringExpression, Constant(0))</c>, and Expression.Equal validates
///     operand types while the tree is being constructed, so each threw
///     InvalidOperationException ("The binary operator Equal is not defined for the types 'System.String'
///     and 'System.Int32'") on first use rather than returning a wrong result.
///
///     Nothing in the repository exercised these paths, which is why the defect survived.
///
///     Coverage limit: these run against List&lt;T&gt;.AsQueryable(), i.e. LINQ to Objects. They prove the
///     expression tree is well formed and evaluates correctly in memory. They do NOT prove EF Core can
///     translate Expression.Call(x, Trim) into SQL, nor that OrElse/AndAlso short-circuiting survives the
///     translation to OR/AND, where operand evaluation order is not guaranteed. That gap needs a database
///     integration test; it is deliberately out of scope here because the defect being guarded is a
///     constant-value defect, which in-memory evaluation catches exactly.
/// </summary>
public class StringEmptinessFilterTests
{
    private sealed class Row
    {
        public string? Name { get; init; }
    }

    /// <summary>Four rows covering the cases the operators discriminate on: text, empty, whitespace, null.</summary>
    private static IQueryable<Row> Rows() => new List<Row>
    {
        new() { Name = "abc" },
        new() { Name = "" },
        new() { Name = "   " },
        new() { Name = null }
    }.AsQueryable();

    private static FilterRequest Filter(FilterType type) => new()
    {
        LogicalOperator = FilterLogicalOperator.And,
        Details = new List<FilterDetailsRequest>
        {
            new() { AttributeName = nameof(Row.Name), Value = "", FilterType = type }
        }
    };

    private static string?[] Apply(FilterType type) =>
        Rows().Filter(Filter(type)).Select(r => r.Name).ToArray();

    [Fact]
    public void IsEmpty_matches_only_the_empty_string()
    {
        Apply(FilterType.IsEmpty).Should().Equal("");
    }

    [Fact]
    public void IsNotEmpty_matches_everything_that_is_not_the_empty_string()
    {
        // null is reported as "not empty": the operator is Expression.NotEqual(value, ""), which is true
        // for null. Whether that is the intended semantic is a product question, not a defect this test
        // asserts away — it records current behaviour so a future change to it is visible.
        Apply(FilterType.IsNotEmpty).Should().Equal("abc", "   ", null);
    }

    [Fact]
    public void IsNullOrWhiteSpace_matches_null_empty_and_whitespace()
    {
        Apply(FilterType.IsNullOrWhiteSpace).Should().Equal("", "   ", null);
    }

    [Fact]
    public void IsNotNullOrWhiteSpace_matches_only_rows_with_visible_text()
    {
        Apply(FilterType.IsNotNullOrWhiteSpace).Should().Equal("abc");
    }

    [Theory]
    [InlineData(FilterType.IsEmpty)]
    [InlineData(FilterType.IsNotEmpty)]
    [InlineData(FilterType.IsNullOrWhiteSpace)]
    [InlineData(FilterType.IsNotNullOrWhiteSpace)]
    public void Building_the_filter_does_not_throw(FilterType type)
    {
        // The original defect surfaced here, at tree construction, before any row was evaluated.
        // This asserts the failure mode directly rather than inferring it from a result count.
        var act = () => Rows().Filter(Filter(type)).ToArray();

        act.Should().NotThrow();
    }
}
