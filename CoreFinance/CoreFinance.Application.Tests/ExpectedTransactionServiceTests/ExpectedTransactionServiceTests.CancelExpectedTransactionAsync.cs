using CoreFinance.Application.Enums;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Cancelling a pending or overdue period exposes its reason and processed state without an actual.<br />
    ///     (VI) Hủy kỳ đang chờ hoặc quá hạn trả lý do và trạng thái đã xử lý mà không tạo giao dịch thực.
    /// </summary>
    [Theory]
    [InlineData(-30)]
    [InlineData(0)]
    [InlineData(7)]
    public async Task CancelExpectedTransactionAsync_ShouldExposeCancelledPeriodAndReason_WhenPeriodIsPending(int days)
    {
        // Arrange
        var expected = Expected(1, days);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.CancelExpectedTransactionAsync(expected.Id, "No bill this month");
        var view = await fixture.Service.GetByIdAsync(expected.Id);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Success);
        view.Should().NotBeNull();
        view!.Status.Should().Be(ExpectedTransactionStatus.Cancelled);
        view.DisplayStatus.Should().Be(2);
        view.AdjustmentReason.Should().Be("No bill this month");
        view.ExpectedAmount.Should().Be(100m);
        view.ProcessedAt.Should().NotBeNull();
        view.ActualTransactionId.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) A cancellation reason is required and invalid requests do not change lifecycle state.<br />
    ///     (VI) Bắt buộc lý do hủy; yêu cầu không hợp lệ không thay trạng thái vòng đời.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public async Task CancelExpectedTransactionAsync_ShouldReturnInvalidRequest_WhenReasonIsBlank(string? reason)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.CancelExpectedTransactionAsync(expected.Id, reason!);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.InvalidRequest);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.AdjustmentReason.Should().BeNull();
        expected.ProcessedAt.Should().BeNull();
    }

    /// <summary>
    ///     (EN) An unavailable expected reference is reported as not found.<br />
    ///     (VI) Tham chiếu kỳ không được trả về được báo không tìm thấy.
    /// </summary>
    [Fact]
    public async Task CancelExpectedTransactionAsync_ShouldReturnNotFound_WhenExpectedReferenceIsUnavailable()
    {
        // Arrange
        var fixture = CreateFixture();

        // Act
        var result = await fixture.Service.CancelExpectedTransactionAsync(Id(1), "Duplicate bill");

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.NotFound);
    }

    /// <summary>
    ///     (EN) Terminal periods keep their original reason, status, and processed timestamp.<br />
    ///     (VI) Kỳ kết thúc giữ nguyên lý do, trạng thái và thời điểm đã xử lý.
    /// </summary>
    [Theory]
    [InlineData(ExpectedTransactionStatus.Confirmed)]
    [InlineData(ExpectedTransactionStatus.Cancelled)]
    [InlineData(ExpectedTransactionStatus.Completed)]
    public async Task CancelExpectedTransactionAsync_ShouldReturnConflict_WhenPeriodIsNotPending(
        ExpectedTransactionStatus status)
    {
        // Arrange
        var expected = Expected(1, status: status);
        expected.AdjustmentReason = "Existing reason";
        expected.ProcessedAt = Today.AddDays(-1);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.CancelExpectedTransactionAsync(expected.Id, "Replacement reason");

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Conflict);
        expected.Status.Should().Be(status);
        expected.AdjustmentReason.Should().Be("Existing reason");
        expected.ProcessedAt.Should().Be(Today.AddDays(-1));
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Write and commit failures must not be reported as a successful cancellation.<br />
    ///     (VI) Lỗi ghi hoặc commit không được báo hủy thành công.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CancelExpectedTransactionAsync_ShouldReturnPersistenceError_WhenWriteOrCommitFails(bool commit)
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
        var result = await fixture.Service.CancelExpectedTransactionAsync(expected.Id, "No bill");

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.PersistenceError);
    }

    /// <summary>
    ///     (EN) Cancellation accepts exactly 500 reason characters but rejects 501 without changing lifecycle state.<br />
    ///     (VI) Hủy chấp nhận đúng 500 ký tự lý do nhưng từ chối 501 mà không đổi trạng thái vòng đời.
    /// </summary>
    [Theory]
    [InlineData(500, ExpectedTransactionActionOutcome.Success)]
    [InlineData(501, ExpectedTransactionActionOutcome.InvalidRequest)]
    public async Task CancelExpectedTransactionAsync_ShouldEnforceReasonLengthBoundary_WhenReasonHas500Or501Characters(
        int length, ExpectedTransactionActionOutcome outcome)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        var reason = new string('r', length);
        var accepted = outcome == ExpectedTransactionActionOutcome.Success;

        // Act
        var result = await fixture.Service.CancelExpectedTransactionAsync(expected.Id, reason);
        var view = await fixture.Service.GetByIdAsync(expected.Id);

        // Assert
        result.Outcome.Should().Be(outcome);
        view!.Status.Should().Be(accepted ? ExpectedTransactionStatus.Cancelled : ExpectedTransactionStatus.Pending);
        view.AdjustmentReason.Should().Be(accepted ? reason : null);
        view.ProcessedAt.HasValue.Should().Be(accepted);
        view.ExpectedAmount.Should().Be(100m);
        view.ActualTransactionId.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }
}
