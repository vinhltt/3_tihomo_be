using CoreFinance.Domain.Enums;
using FluentAssertions;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Categories aggregate repeated expenses and mixed income/expense while preserving their signs.<br />
    ///     (VI) Danh mục gộp chi lặp và thu/chi hỗn hợp, giữ dấu của số tiền.
    /// </summary>
    [Fact]
    public async Task GetCategoryForecastAsync_ShouldAggregateSignedAmounts_WhenCategoriesContainMixedPeriods()
    {
        // Arrange
        var salary = Expected(1, amount: 5000m, type: RecurringTransactionType.Income);
        salary.Category = "Salary";
        var food = Expected(2, amount: 2000m);
        food.Category = "Food";
        var moreFood = Expected(3, amount: 800m);
        moreFood.Category = "Food";
        var transport = Expected(4, amount: 1500m);
        transport.Category = "Transport";
        var businessIncome = Expected(5, amount: 3000m, type: RecurringTransactionType.Income);
        businessIncome.Category = "Business";
        var businessExpense = Expected(6, amount: 1000m);
        businessExpense.Category = "Business";
        var moreBusinessIncome = Expected(7, amount: 500m, type: RecurringTransactionType.Income);
        moreBusinessIncome.Category = "Business";
        var fixture = CreateFixture(salary, food, moreFood, transport, businessIncome, businessExpense, moreBusinessIncome);

        // Act
        var result = await fixture.Service.GetCategoryForecastAsync(OwnerId, Today, Today.AddDays(7));

        // Assert
        result.Should().BeEquivalentTo(new Dictionary<string, decimal>
        {
            ["Salary"] = 5000m, ["Food"] = -2800m, ["Transport"] = -1500m, ["Business"] = 2500m
        });
    }

    /// <summary>
    ///     (EN) Categories count overdue Pending periods through the last date, excluding terminal and uncategorized rows.<br />
    ///     (VI) Danh mục tính kỳ Pending quá hạn đến trọn ngày cuối, loại kỳ kết thúc và không có danh mục.
    /// </summary>
    [Fact]
    public async Task GetCategoryForecastAsync_ShouldCountOnlyCategorizedPendingPeriods_WhenDatesAndStatusesAreMixed()
    {
        // Arrange
        var overdue = Expected(1, -1, amount: 300m);
        var lastDay = Expected(2, 7, amount: 120m, type: RecurringTransactionType.Income);
        lastDay.ExpectedDate = lastDay.ExpectedDate.AddHours(23).AddMinutes(59);
        var noCategory = Expected(3, amount: 1000m);
        noCategory.Category = null;
        var emptyCategory = Expected(4, amount: 2000m);
        emptyCategory.Category = "";
        var fixture = CreateFixture(overdue, lastDay, noCategory, emptyCategory,
            Expected(5, -2, amount: 4000m), Expected(6, 8, amount: 5000m),
            Expected(7, status: ExpectedTransactionStatus.Confirmed, amount: 6000m),
            Expected(8, status: ExpectedTransactionStatus.Cancelled, amount: 7000m),
            Expected(9, status: ExpectedTransactionStatus.Completed, amount: 8000m));

        // Act
        var result = await fixture.Service.GetCategoryForecastAsync(OwnerId,
            Today.AddDays(-1).AddHours(12), Today.AddDays(7).AddHours(12));

        // Assert
        result.Should().BeEquivalentTo(new Dictionary<string, decimal> { ["Utilities"] = -180m });
        overdue.Status.Should().Be(ExpectedTransactionStatus.Pending);
        overdue.ProcessedAt.Should().BeNull();
    }

    /// <summary>
    ///     (EN) No categorized pending period yields an empty dictionary rather than invented category values.<br />
    ///     (VI) Không có kỳ Pending có danh mục trả dictionary rỗng, không tạo giá trị danh mục giả.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetCategoryForecastAsync_ShouldReturnEmpty_WhenNoCategorizedPendingPeriodMatches(bool otherPeriods)
    {
        // Arrange
        var noCategory = Expected(1);
        noCategory.Category = null;
        var fixture = CreateFixture(otherPeriods ? [noCategory, Expected(2, -10), Expected(3, 50)] : []);

        // Act
        var result = await fixture.Service.GetCategoryForecastAsync(OwnerId, Today, Today.AddDays(7));

        // Assert
        result.Should().BeEmpty();
    }
}
