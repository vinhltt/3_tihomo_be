using CoreFinance.Application.Enums;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Repeated adjustments expose the current amount while retaining only the first original amount.<br />
    ///     (VI) Điều chỉnh nhiều lần trả số tiền hiện hành nhưng chỉ giữ số gốc của lần đầu.
    /// </summary>
    [Fact]
    public async Task AdjustExpectedTransactionAsync_ShouldKeepFirstOriginalAmount_WhenAdjustedRepeatedly()
    {
        // Arrange
        var expected = Expected(1, -1, amount: 100m);
        var fixture = CreateFixture(expected);

        // Act
        var first = await fixture.Service.AdjustExpectedTransactionAsync(expected.Id, 150.75m, "Meter reading");
        var firstView = await fixture.Service.GetByIdAsync(expected.Id);
        var second = await fixture.Service.AdjustExpectedTransactionAsync(expected.Id, 80.25m, "Corrected reading");
        var secondView = await fixture.Service.GetByIdAsync(expected.Id);

        // Assert
        first.Outcome.Should().Be(ExpectedTransactionActionOutcome.Success);
        firstView.Should().NotBeNull();
        firstView!.ExpectedAmount.Should().Be(150.75m);
        firstView.OriginalAmount.Should().Be(100m);
        firstView.IsAdjusted.Should().BeTrue();
        firstView.AdjustmentReason.Should().Be("Meter reading");
        second.Outcome.Should().Be(ExpectedTransactionActionOutcome.Success);
        secondView.Should().NotBeNull();
        secondView!.ExpectedAmount.Should().Be(80.25m);
        secondView.OriginalAmount.Should().Be(100m);
        secondView.IsAdjusted.Should().BeTrue();
        secondView.AdjustmentReason.Should().Be("Corrected reading");
        secondView.Status.Should().Be(ExpectedTransactionStatus.Pending);
        secondView.DisplayStatus.Should().Be(4);
        secondView.ProcessedAt.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) An already adjusted period retains its original amount even for a same-amount adjustment.<br />
    ///     (VI) Kỳ đã điều chỉnh giữ số gốc kể cả khi điều chỉnh cùng số tiền hiện hành.
    /// </summary>
    [Fact]
    public async Task AdjustExpectedTransactionAsync_ShouldKeepOriginalAndReplaceReason_WhenAmountIsUnchanged()
    {
        // Arrange
        var expected = Expected(1, amount: 120m);
        expected.IsAdjusted = true;
        expected.OriginalAmount = 100m;
        expected.AdjustmentReason = "Old reason";
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.AdjustExpectedTransactionAsync(expected.Id, 120m, "Confirmed reading");
        var view = await fixture.Service.GetByIdAsync(expected.Id);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Success);
        view!.OriginalAmount.Should().Be(100m);
        view.ExpectedAmount.Should().Be(120m);
        view.AdjustmentReason.Should().Be("Confirmed reading");
        view.IsAdjusted.Should().BeTrue();
    }

    /// <summary>
    ///     (EN) Non-positive amounts are rejected before changing the original or current amount.<br />
    ///     (VI) Số tiền không dương bị từ chối trước khi thay số gốc hoặc số hiện hành.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AdjustExpectedTransactionAsync_ShouldReturnInvalidRequest_WhenAmountIsNotPositive(int amount)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.AdjustExpectedTransactionAsync(expected.Id, amount, "Reading correction");

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.InvalidRequest);
        expected.ExpectedAmount.Should().Be(100m);
        expected.OriginalAmount.Should().BeNull();
        expected.IsAdjusted.Should().BeFalse();
        expected.AdjustmentReason.Should().BeNull();
    }

    /// <summary>
    ///     (EN) A reason is mandatory for amount adjustments.<br />
    ///     (VI) Điều chỉnh số tiền bắt buộc có lý do.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public async Task AdjustExpectedTransactionAsync_ShouldReturnInvalidRequest_WhenReasonIsBlank(string? reason)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.AdjustExpectedTransactionAsync(expected.Id, 120m, reason!);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.InvalidRequest);
        expected.ExpectedAmount.Should().Be(100m);
        expected.OriginalAmount.Should().BeNull();
        expected.IsAdjusted.Should().BeFalse();
    }

    /// <summary>
    ///     (EN) An unavailable expected reference returns not found.<br />
    ///     (VI) Tham chiếu kỳ không được trả về dẫn đến kết quả không tìm thấy.
    /// </summary>
    [Fact]
    public async Task AdjustExpectedTransactionAsync_ShouldReturnNotFound_WhenExpectedReferenceIsUnavailable()
    {
        // Arrange
        var fixture = CreateFixture();

        // Act
        var result = await fixture.Service.AdjustExpectedTransactionAsync(Id(1), 120m, "New reading");

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.NotFound);
    }

    /// <summary>
    ///     (EN) Terminal periods reject adjustments and retain all amount history.<br />
    ///     (VI) Kỳ kết thúc từ chối điều chỉnh và giữ toàn bộ lịch sử số tiền.
    /// </summary>
    [Theory]
    [InlineData(ExpectedTransactionStatus.Confirmed)]
    [InlineData(ExpectedTransactionStatus.Cancelled)]
    [InlineData(ExpectedTransactionStatus.Completed)]
    public async Task AdjustExpectedTransactionAsync_ShouldReturnConflict_WhenPeriodIsNotPending(
        ExpectedTransactionStatus status)
    {
        // Arrange
        var expected = Expected(1, status: status, amount: 120m);
        expected.IsAdjusted = true;
        expected.OriginalAmount = 100m;
        expected.AdjustmentReason = "Existing reason";
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.AdjustExpectedTransactionAsync(expected.Id, 80m, "New reason");

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Conflict);
        expected.Status.Should().Be(status);
        expected.ExpectedAmount.Should().Be(120m);
        expected.OriginalAmount.Should().Be(100m);
        expected.AdjustmentReason.Should().Be("Existing reason");
    }

    /// <summary>
    ///     (EN) Write and commit failures are returned as persistence errors, not successful adjustments.<br />
    ///     (VI) Lỗi ghi hoặc commit được trả là lỗi lưu trữ, không phải điều chỉnh thành công.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AdjustExpectedTransactionAsync_ShouldReturnPersistenceError_WhenWriteOrCommitFails(bool commit)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        if (commit)
            fixture.Transaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Commit failed"));
        else
            fixture.UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Write failed"));

        // Act
        var result = await fixture.Service.AdjustExpectedTransactionAsync(expected.Id, 120m, "New reading");

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.PersistenceError);
    }

    /// <summary>
    ///     (EN) The reason length boundary accepts 500 characters and preserves untouched amount history for 501.<br />
    ///     (VI) Biên độ dài lý do chấp nhận 500 ký tự và giữ lịch sử tiền chưa thay đổi khi có 501 ký tự.
    /// </summary>
    [Theory]
    [InlineData(500, ExpectedTransactionActionOutcome.Success)]
    [InlineData(501, ExpectedTransactionActionOutcome.InvalidRequest)]
    public async Task AdjustExpectedTransactionAsync_ShouldEnforceReasonLengthBoundary_WhenReasonHas500Or501Characters(
        int length, ExpectedTransactionActionOutcome outcome)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        var reason = new string('r', length);
        var accepted = outcome == ExpectedTransactionActionOutcome.Success;

        // Act
        var result = await fixture.Service.AdjustExpectedTransactionAsync(expected.Id, 150.75m, reason);
        var view = await fixture.Service.GetByIdAsync(expected.Id);

        // Assert
        result.Outcome.Should().Be(outcome);
        view!.ExpectedAmount.Should().Be(accepted ? 150.75m : 100m);
        view.OriginalAmount.Should().Be(accepted ? 100m : (decimal?)null);
        view.IsAdjusted.Should().Be(accepted);
        view.AdjustmentReason.Should().Be(accepted ? reason : null);
        view.Status.Should().Be(ExpectedTransactionStatus.Pending);
        view.ActualTransactionId.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }
}
