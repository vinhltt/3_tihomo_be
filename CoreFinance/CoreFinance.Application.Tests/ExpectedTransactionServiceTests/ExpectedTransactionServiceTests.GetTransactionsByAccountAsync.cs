using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using FluentAssertions;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Account history includes all lifecycle states and dates, with a stable date/ID order.<br />
    ///     (VI) Lịch sử tài khoản gồm mọi trạng thái và ngày, sắp ổn định theo ngày/ID.
    /// </summary>
    [Fact]
    public async Task GetTransactionsByAccountAsync_ShouldReturnStableHistory_WhenAccountHasMixedPeriods()
    {
        // Arrange
        var pending = Expected(1, -1);
        var confirmed = Expected(2, status: ExpectedTransactionStatus.Confirmed);
        var cancelled = Expected(3, status: ExpectedTransactionStatus.Cancelled);
        var completed = Expected(4, 7, ExpectedTransactionStatus.Completed);
        var otherAccount = Expected(5);
        otherAccount.AccountId = Id(50);
        var unbound = Expected(6);
        unbound.AccountId = null;
        var fixture = CreateFixture(completed, cancelled, otherAccount, confirmed, unbound, pending);

        // Act
        var result = (await fixture.Service.GetTransactionsByAccountAsync(AccountId)).ToList();

        // Assert
        result.Select(t => t.Id).Should().Equal(Id(1), Id(2), Id(3), Id(4));
        result.Select(t => t.Status).Should().Equal(ExpectedTransactionStatus.Pending,
            ExpectedTransactionStatus.Confirmed, ExpectedTransactionStatus.Cancelled, ExpectedTransactionStatus.Completed);
        result.Select(t => t.DisplayStatus).Should().Equal(4, 1, 2, 3);
        result.Select(t => t.DaysUntilDue).Should().Equal(-1, 0, 0, 7);
        pending.Status.Should().Be(ExpectedTransactionStatus.Pending);
        pending.ProcessedAt.Should().BeNull();
    }

    /// <summary>
    ///     (EN) Empty account history is returned for both an empty source and a source with no matching account.<br />
    ///     (VI) Trả lịch sử rỗng khi nguồn rỗng hoặc không có tài khoản khớp.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetTransactionsByAccountAsync_ShouldReturnEmpty_WhenNoPeriodMatchesAccount(bool otherPeriod)
    {
        // Arrange
        var fixture = CreateFixture(otherPeriod ? [Expected(1)] : []);

        // Act
        var result = await fixture.Service.GetTransactionsByAccountAsync(Id(50));

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Account history exposes adjustment history and linked state without hiding income or expense periods.<br />
    ///     (VI) Lịch sử tài khoản trả lịch sử điều chỉnh và liên kết, không ẩn kỳ thu hoặc chi.
    /// </summary>
    [Fact]
    public async Task GetTransactionsByAccountAsync_ShouldExposeAdjustmentAndLinkState_WhenHistoryContainsBothTypes()
    {
        // Arrange
        var income = Expected(1, amount: 5000m, type: RecurringTransactionType.Income);
        var expense = Expected(2, status: ExpectedTransactionStatus.Confirmed, amount: 120m);
        expense.IsAdjusted = true;
        expense.OriginalAmount = 100m;
        expense.AdjustmentReason = "New meter reading";
        expense.ActualTransactionId = ActualId;
        var fixture = CreateFixture(expense, income);

        // Act
        var result = (await fixture.Service.GetTransactionsByAccountAsync(AccountId)).ToList();

        // Assert
        result[0].TransactionType.Should().Be(RecurringTransactionType.Income);
        result[0].ExpectedAmount.Should().Be(5000m);
        result[0].HasActualTransaction.Should().BeFalse();
        result[1].TransactionType.Should().Be(RecurringTransactionType.Expense);
        result[1].ExpectedAmount.Should().Be(120m);
        result[1].OriginalAmount.Should().Be(100m);
        result[1].IsAdjusted.Should().BeTrue();
        result[1].AdjustmentReason.Should().Be("New meter reading");
        result[1].HasActualTransaction.Should().BeTrue();
        result[1].ActualTransactionId.Should().Be(ActualId);
    }

    /// <summary>
    ///     (EN) Unavailable or foreign navigation metadata is masked in the consumer projection, not SQL/isolation proof.<br />
    ///     (VI) Projection che metadata navigation thiếu hoặc khác chủ; không chứng minh SQL hay cách ly repository.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetTransactionsByAccountAsync_ShouldMaskUnavailableNavigationMetadata_WhenNavigationIsNotOwned(
        bool foreign)
    {
        // Arrange
        var expected = Expected(1, -1);
        if (foreign)
        {
            expected.Account = new Account { Id = AccountId, UserId = OtherOwnerId, Name = "Private account" };
            expected.RecurringTransactionTemplate = new RecurringTransactionTemplate
            {
                Id = TemplateId, UserId = OtherOwnerId, Name = "Private template"
            };
        }
        var fixture = CreateFixture(expected);

        // Act
        var result = (await fixture.Service.GetTransactionsByAccountAsync(AccountId)).Single();

        // Assert
        result.AccountName.Should().BeNullOrEmpty();
        result.AccountType.Should().BeNull();
        result.TemplateName.Should().BeNullOrEmpty();
        result.DisplayStatus.Should().Be(4);
        result.AccountId.Should().Be(AccountId);
        result.RecurringTransactionTemplateId.Should().Be(TemplateId);
    }
}
