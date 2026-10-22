using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using FluentAssertions;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Cash-flow forecasts preserve positive, negative, income-only, expense-only, and balanced totals.<br />
    ///     (VI) Dự báo dòng tiền giữ kết quả dương, âm, chỉ thu, chỉ chi và cân bằng.
    /// </summary>
    [Theory]
    [InlineData(5000, 3500, 1500)]
    [InlineData(2000, 4500, -2500)]
    [InlineData(5000, 0, 5000)]
    [InlineData(0, 3500, -3500)]
    [InlineData(100, 100, 0)]
    public async Task GetCashFlowForecastAsync_ShouldReturnNetPendingAmount_WhenIncomeAndExpenseTotalsDiffer(
        int income, int expense, int net)
    {
        // Arrange
        var rows = new List<ExpectedTransaction>();
        if (income > 0)
        {
            rows.Add(Expected(1, amount: income * 0.4m, type: RecurringTransactionType.Income));
            rows.Add(Expected(2, 7, amount: income * 0.6m, type: RecurringTransactionType.Income));
        }
        if (expense > 0)
        {
            rows.Add(Expected(3, amount: expense * 0.25m));
            rows.Add(Expected(4, 7, amount: expense * 0.75m));
        }
        var fixture = CreateFixture(rows.ToArray());

        // Act
        var result = await fixture.Service.GetCashFlowForecastAsync(OwnerId, Today, Today.AddDays(7));

        // Assert
        result.Should().Be(net);
    }

    /// <summary>
    ///     (EN) Forecasts retain overdue Pending amounts and whole date boundaries, excluding terminal and transfer rows.<br />
    ///     (VI) Dự báo giữ số tiền Pending quá hạn và trọn ngày biên, loại kỳ kết thúc và chuyển khoản.
    /// </summary>
    [Fact]
    public async Task GetCashFlowForecastAsync_ShouldCountOverdueWithinInclusiveDates_WhenStatusesAndDatesAreMixed()
    {
        // Arrange
        var lastDay = Expected(2, 7, amount: 1500m);
        lastDay.ExpectedDate = lastDay.ExpectedDate.AddHours(23).AddMinutes(59);
        var overdue = Expected(1, -1, amount: 5000m, type: RecurringTransactionType.Income);
        var fixture = CreateFixture(overdue, lastDay,
            Expected(3, -2, amount: 8000m, type: RecurringTransactionType.Income),
            Expected(4, 8, amount: 9000m),
            Expected(5, status: ExpectedTransactionStatus.Confirmed, amount: 2000m),
            Expected(6, status: ExpectedTransactionStatus.Cancelled, amount: 3000m),
            Expected(7, status: ExpectedTransactionStatus.Completed, amount: 4000m),
            Expected(8, amount: 7000m, type: RecurringTransactionType.Transfer));

        // Act
        var result = await fixture.Service.GetCashFlowForecastAsync(OwnerId,
            Today.AddDays(-1).AddHours(12), Today.AddDays(7).AddHours(12));

        // Assert
        result.Should().Be(3500m);
        overdue.Status.Should().Be(ExpectedTransactionStatus.Pending);
        overdue.ProcessedAt.Should().BeNull();
    }

    /// <summary>
    ///     (EN) Empty or entirely out-of-range data produces zero cash flow.<br />
    ///     (VI) Dữ liệu rỗng hoặc hoàn toàn ngoài phạm vi trả dòng tiền bằng không.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetCashFlowForecastAsync_ShouldReturnZero_WhenNoPeriodIsInRange(bool outside)
    {
        // Arrange
        var fixture = CreateFixture(outside ? [Expected(1, -10), Expected(2, 50)] : []);

        // Act
        var result = await fixture.Service.GetCashFlowForecastAsync(OwnerId, Today, Today.AddDays(7));

        // Assert
        result.Should().Be(0m);
    }
}
