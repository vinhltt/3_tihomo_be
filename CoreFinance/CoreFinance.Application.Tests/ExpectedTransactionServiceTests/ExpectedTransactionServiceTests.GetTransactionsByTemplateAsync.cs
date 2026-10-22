using CoreFinance.Domain.Enums;
using FluentAssertions;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Template history includes all statuses and past/future dates, ordered stably by date then ID.<br />
    ///     (VI) Lịch sử mẫu gồm mọi trạng thái và ngày quá khứ/tương lai, sắp ổn định theo ngày rồi ID.
    /// </summary>
    [Fact]
    public async Task GetTransactionsByTemplateAsync_ShouldReturnStableHistory_WhenTemplateHasMixedPeriods()
    {
        // Arrange
        var overdue = Expected(1, -1);
        var confirmed = Expected(2, status: ExpectedTransactionStatus.Confirmed);
        var cancelled = Expected(3, status: ExpectedTransactionStatus.Cancelled);
        var completed = Expected(4, 7, ExpectedTransactionStatus.Completed);
        var otherTemplate = Expected(5);
        otherTemplate.RecurringTransactionTemplateId = Id(50);
        var withoutTemplate = Expected(6);
        withoutTemplate.RecurringTransactionTemplateId = null;
        var fixture = CreateFixture(completed, cancelled, otherTemplate, confirmed, withoutTemplate, overdue);

        // Act
        var result = (await fixture.Service.GetTransactionsByTemplateAsync(TemplateId)).ToList();

        // Assert
        result.Select(t => t.Id).Should().Equal(Id(1), Id(2), Id(3), Id(4));
        result.Select(t => t.Status).Should().Equal(ExpectedTransactionStatus.Pending,
            ExpectedTransactionStatus.Confirmed, ExpectedTransactionStatus.Cancelled, ExpectedTransactionStatus.Completed);
        result.Select(t => t.DisplayStatus).Should().Equal(4, 1, 2, 3);
        result.Select(t => t.DaysUntilDue).Should().Equal(-1, 0, 0, 7);
    }

    /// <summary>
    ///     (EN) An empty source or a source without the requested template returns empty history.<br />
    ///     (VI) Nguồn rỗng hoặc không có mẫu yêu cầu trả lịch sử rỗng.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetTransactionsByTemplateAsync_ShouldReturnEmpty_WhenNoPeriodMatchesTemplate(bool otherPeriod)
    {
        // Arrange
        var fixture = CreateFixture(otherPeriod ? [Expected(1)] : []);

        // Act
        var result = await fixture.Service.GetTransactionsByTemplateAsync(Id(50));

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Nullable account references and derived due data stay meaningful for unbound template periods.<br />
    ///     (VI) Tham chiếu tài khoản null và dữ liệu hạn dẫn xuất giữ ý nghĩa với kỳ mẫu chưa gắn tài khoản.
    /// </summary>
    [Fact]
    public async Task GetTransactionsByTemplateAsync_ShouldKeepNullableAccountAndDueData_WhenPeriodIsUnbound()
    {
        // Arrange
        var expected = Expected(1, 7, amount: 5000m, type: RecurringTransactionType.Income);
        expected.AccountId = null;
        expected.ExpectedDate = expected.ExpectedDate.AddHours(23);
        var fixture = CreateFixture(expected);

        // Act
        var result = (await fixture.Service.GetTransactionsByTemplateAsync(TemplateId)).Single();

        // Assert
        result.AccountId.Should().BeNull();
        result.AccountName.Should().BeNullOrEmpty();
        result.AccountType.Should().BeNull();
        result.TransactionType.Should().Be(RecurringTransactionType.Income);
        result.ExpectedAmount.Should().Be(5000m);
        result.DisplayStatus.Should().Be(0);
        result.DaysUntilDue.Should().Be(7);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ProcessedAt.Should().BeNull();
    }
}
