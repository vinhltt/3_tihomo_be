using CoreFinance.Domain.Enums;
using FluentAssertions;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Pending reads include overdue periods but exclude every terminal state from an already-scoped source.<br />
    ///     (VI) Đọc Pending gồm kỳ quá hạn và loại mọi trạng thái kết thúc từ nguồn đã giả định scoped.
    /// </summary>
    [Fact]
    public async Task GetPendingTransactionsAsync_ShouldIncludeOverdueAndExcludeTerminalStates_WhenPeriodsAreMixed()
    {
        // Arrange
        var overdue = Expected(1, -30);
        var today = Expected(2);
        var future = Expected(3, 50);
        var fixture = CreateFixture(future, Expected(4, status: ExpectedTransactionStatus.Confirmed), today,
            Expected(5, status: ExpectedTransactionStatus.Cancelled), overdue,
            Expected(6, status: ExpectedTransactionStatus.Completed));

        // Act
        var result = (await fixture.Service.GetPendingTransactionsAsync(OwnerId)).ToList();

        // Assert
        result.Select(t => t.Id).Should().Equal(Id(1), Id(2), Id(3));
        result.Select(t => t.DisplayStatus).Should().Equal(4, 0, 0);
        result.Select(t => t.DaysUntilDue).Should().Equal(-30, 0, 50);
        result.Should().OnlyContain(t => t.Status == ExpectedTransactionStatus.Pending);
        overdue.Status.Should().Be(ExpectedTransactionStatus.Pending);
        overdue.ProcessedAt.Should().BeNull();
    }

    /// <summary>
    ///     (EN) No pending period produces an empty result, for either empty or terminal-only input.<br />
    ///     (VI) Không có kỳ Pending trả kết quả rỗng với nguồn rỗng hoặc chỉ có kỳ kết thúc.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetPendingTransactionsAsync_ShouldReturnEmpty_WhenNoPendingPeriodExists(bool terminal)
    {
        // Arrange
        var fixture = CreateFixture(terminal
            ? [Expected(1, status: ExpectedTransactionStatus.Confirmed), Expected(2, status: ExpectedTransactionStatus.Cancelled)]
            : []);

        // Act
        var result = await fixture.Service.GetPendingTransactionsAsync(OwnerId);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Reads preserve null references and show adjusted amounts without claiming an actual payment exists.<br />
    ///     (VI) Đọc giữ tham chiếu null và hiển thị số đã chỉnh mà không báo đã có thanh toán thực.
    /// </summary>
    [Fact]
    public async Task GetPendingTransactionsAsync_ShouldExposeAdjustmentAndNullableReferences_WhenPeriodIsUnbound()
    {
        // Arrange
        var expected = Expected(1, amount: 120m);
        expected.AccountId = null;
        expected.RecurringTransactionTemplateId = null;
        expected.IsAdjusted = true;
        expected.OriginalAmount = 100m;
        expected.AdjustmentReason = "Meter reading";
        var fixture = CreateFixture(expected);

        // Act
        var result = (await fixture.Service.GetPendingTransactionsAsync(OwnerId)).Single();

        // Assert
        result.AccountId.Should().BeNull();
        result.RecurringTransactionTemplateId.Should().BeNull();
        result.ExpectedAmount.Should().Be(120m);
        result.OriginalAmount.Should().Be(100m);
        result.IsAdjusted.Should().BeTrue();
        result.AdjustmentReason.Should().Be("Meter reading");
        result.HasActualTransaction.Should().BeFalse();
        result.ActualTransactionId.Should().BeNull();
    }
}
