using CoreFinance.Application.Enums;
using CoreFinance.Domain.Constants;
using CoreFinance.Domain.Entities;
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

    /// <summary>
    ///     (EN) Characterization: a successful cancellation reports the success code and message.<br />
    ///     (VI) Đặc tả hiện trạng: hủy thành công trả mã và message thành công.
    /// </summary>
    [Fact]
    public async Task CancelExpectedTransactionAsync_ShouldReturnSuccessCodeAndMessage_WhenReasonIsValid()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.CancelExpectedTransactionAsync(expected.Id, "No bill this month");

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Success);
        result.Code.Should().Be(CoreFinanceErrorCodes.Success);
        result.Message.Should().Be(CoreFinanceMessages.ExpectedTransactionCancelled);
        result.ExpectedTransactionId.Should().Be(expected.Id);
    }

    /// <summary>
    ///     (EN) Characterization: null, empty, whitespace and 501-character reasons report the invalid-reason code and message.<br />
    ///     (VI) Đặc tả hiện trạng: lý do null, rỗng, khoảng trắng và 501 ký tự trả mã và message invalid_reason.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" \t ")]
    [InlineData("501")]
    public async Task CancelExpectedTransactionAsync_ShouldReturnInvalidReasonCodeAndMessage_WhenReasonIsInvalid(
        string? reason)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        var input = reason == "501" ? new string('r', 501) : reason;

        // Act
        var result = await fixture.Service.CancelExpectedTransactionAsync(expected.Id, input!);

        // Assert
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.InvalidRequest,
            CoreFinanceErrorCodes.InvalidReason, CoreFinanceMessages.ReasonRequired);
        fixture.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.ExpectedRepository.Verify(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    ///     (EN) An invalid cancellation reason on an otherwise cancellable pending period writes nothing and leaves it pending.<br />
    ///     (VI) Lý do hủy không hợp lệ trên kỳ chờ có thể hủy không ghi gì và giữ kỳ ở trạng thái chờ.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    [InlineData("501")]
    public async Task CancelExpectedTransactionAsync_ShouldNotWriteAnything_WhenReasonIsInvalid(string? reason)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        var input = reason == "501" ? new string('r', 501) : reason;

        // Act
        var result = await fixture.Service.CancelExpectedTransactionAsync(expected.Id, input!);

        // Assert
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.InvalidRequest,
            CoreFinanceErrorCodes.InvalidReason, CoreFinanceMessages.ReasonRequired);
        AssertNothingWritten(fixture);
        AssertPeriodUnchanged(expected);
    }

    /// <summary>
    ///     (EN) Characterization: an unknown or soft-deleted period reports not-found even when the reason is also invalid.<br />
    ///     (VI) Đặc tả hiện trạng: kỳ không tồn tại hoặc xóa mềm trả not_found kể cả khi lý do cũng sai.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CancelExpectedTransactionAsync_ShouldReturnNotFoundCodeAndMessage_WhenPeriodIsMissingAndReasonIsInvalid(
        bool softDeleted)
    {
        // Arrange
        var fixture = CreateFixture();
        if (softDeleted)
        {
            var tombstone = Expected(1);
            tombstone.IsDeleted = "20261008090000";
            fixture.ExpectedTransactions.Add(tombstone);
        }

        // Act
        var result = await fixture.Service.CancelExpectedTransactionAsync(Id(1), "");

        // Assert
        AssertActionFailure(result, Id(1), ExpectedTransactionActionOutcome.NotFound,
            CoreFinanceErrorCodes.ExpectedTransactionNotFound, CoreFinanceMessages.ExpectedTransactionNotFound);
    }

    /// <summary>
    ///     (EN) Characterization: a non-pending or linked period reports the state-conflict code and message, even when the reason is invalid.<br />
    ///     (VI) Đặc tả hiện trạng: kỳ không ở trạng thái chờ hoặc đã liên kết trả mã và message state_conflict, kể cả khi lý do sai.
    /// </summary>
    [Theory]
    [InlineData(ExpectedTransactionStatus.Confirmed, "Replacement reason")]
    [InlineData(ExpectedTransactionStatus.Cancelled, "")]
    [InlineData(ExpectedTransactionStatus.Completed, null)]
    [InlineData(ExpectedTransactionStatus.Pending, "   ")]
    public async Task CancelExpectedTransactionAsync_ShouldReturnStateConflictCodeAndMessage_WhenPeriodIsNotAvailable(
        ExpectedTransactionStatus status, string? reason)
    {
        // Arrange
        var expected = Expected(1, status: status);
        if (status == ExpectedTransactionStatus.Pending) expected.ActualTransactionId = ActualId;
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.CancelExpectedTransactionAsync(expected.Id, reason!);

        // Assert
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.Conflict,
            CoreFinanceErrorCodes.StateConflict, CoreFinanceMessages.PendingTransactionRequired);
    }

    /// <summary>
    ///     (EN) Characterization: write or commit failures report the persistence code and message.<br />
    ///     (VI) Đặc tả hiện trạng: lỗi ghi hoặc commit trả mã và message lỗi lưu trữ.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CancelExpectedTransactionAsync_ShouldReturnPersistenceCodeAndMessage_WhenWriteOrCommitFails(
        bool commit)
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
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.PersistenceError,
            CoreFinanceErrorCodes.PersistenceError, CoreFinanceMessages.OperationCouldNotBeSaved);
        fixture.Transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
