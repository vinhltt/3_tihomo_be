using CoreFinance.Application.Enums;
using CoreFinance.Domain.Constants;
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

    /// <summary>
    ///     (EN) Characterization: a successful adjustment reports the success code and message.<br />
    ///     (VI) Đặc tả hiện trạng: điều chỉnh thành công trả mã và message thành công.
    /// </summary>
    [Fact]
    public async Task AdjustExpectedTransactionAsync_ShouldReturnSuccessCodeAndMessage_WhenAmountAndReasonAreValid()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.AdjustExpectedTransactionAsync(expected.Id, 120m, "Reading correction");

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Success);
        result.Code.Should().Be(CoreFinanceErrorCodes.Success);
        result.Message.Should().Be(CoreFinanceMessages.ExpectedTransactionAdjusted);
        result.ExpectedTransactionId.Should().Be(expected.Id);
    }

    /// <summary>
    ///     (EN) Characterization: when both amount and reason are invalid, the amount error is reported first.<br />
    ///     (VI) Đặc tả hiện trạng: khi cả số tiền và lý do đều sai, lỗi số tiền được trả trước.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AdjustExpectedTransactionAsync_ShouldReturnInvalidAmountFirst_WhenAmountAndReasonAreInvalid(
        string reason)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.AdjustExpectedTransactionAsync(expected.Id, 0m, reason);

        // Assert
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.InvalidRequest,
            CoreFinanceErrorCodes.InvalidAmount, CoreFinanceMessages.ExpectedAmountMustBePositive);
    }

    /// <summary>
    ///     (EN) A non-positive amount on an otherwise adjustable pending period reports the invalid-amount code and message, writes nothing and keeps all amount history untouched.<br />
    ///     (VI) Số tiền không dương trên kỳ chờ có thể điều chỉnh trả mã và message invalid_amount, không ghi gì và giữ nguyên lịch sử số tiền.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AdjustExpectedTransactionAsync_ShouldNotWriteAnything_WhenAmountIsNotPositive(int amount)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.AdjustExpectedTransactionAsync(expected.Id, amount, "Reading correction");

        // Assert
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.InvalidRequest,
            CoreFinanceErrorCodes.InvalidAmount, CoreFinanceMessages.ExpectedAmountMustBePositive);
        AssertNothingWritten(fixture);
        AssertPeriodUnchanged(expected);
    }

    /// <summary>
    ///     (EN) A null, empty, whitespace or 501-character reason on an otherwise adjustable pending period reports the invalid-reason code and message, writes nothing and keeps all amount history untouched.<br />
    ///     (VI) Lý do null, rỗng, khoảng trắng hoặc 501 ký tự trên kỳ chờ có thể điều chỉnh trả mã và message invalid_reason, không ghi gì và giữ nguyên lịch sử số tiền.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" \t ")]
    [InlineData("501")]
    public async Task AdjustExpectedTransactionAsync_ShouldNotWriteAnything_WhenReasonIsInvalid(string? reason)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        var input = reason == "501" ? new string('r', 501) : reason;

        // Act
        var result = await fixture.Service.AdjustExpectedTransactionAsync(expected.Id, 120m, input!);

        // Assert
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.InvalidRequest,
            CoreFinanceErrorCodes.InvalidReason, CoreFinanceMessages.ReasonRequired);
        AssertNothingWritten(fixture);
        AssertPeriodUnchanged(expected);
    }

    /// <summary>
    ///     (EN) Characterization: an unknown or soft-deleted period reports not-found even when amount and reason are invalid.<br />
    ///     (VI) Đặc tả hiện trạng: kỳ không tồn tại hoặc xóa mềm trả not_found kể cả khi số tiền và lý do đều sai.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AdjustExpectedTransactionAsync_ShouldReturnNotFoundCodeAndMessage_WhenPeriodIsMissingAndInputIsInvalid(
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
        var result = await fixture.Service.AdjustExpectedTransactionAsync(Id(1), 0m, "");

        // Assert
        AssertActionFailure(result, Id(1), ExpectedTransactionActionOutcome.NotFound,
            CoreFinanceErrorCodes.ExpectedTransactionNotFound, CoreFinanceMessages.ExpectedTransactionNotFound);
    }

    /// <summary>
    ///     (EN) Characterization: a non-pending or linked period reports the state-conflict code and message, even when the input is invalid.<br />
    ///     (VI) Đặc tả hiện trạng: kỳ không ở trạng thái chờ hoặc đã liên kết trả mã và message state_conflict, kể cả khi đầu vào sai.
    /// </summary>
    [Theory]
    [InlineData(ExpectedTransactionStatus.Confirmed, 120, "Reading correction")]
    [InlineData(ExpectedTransactionStatus.Cancelled, 0, "Reading correction")]
    [InlineData(ExpectedTransactionStatus.Completed, 120, "")]
    [InlineData(ExpectedTransactionStatus.Pending, -1, "")]
    public async Task AdjustExpectedTransactionAsync_ShouldReturnStateConflictCodeAndMessage_WhenPeriodIsNotAvailable(
        ExpectedTransactionStatus status, int amount, string reason)
    {
        // Arrange
        var expected = Expected(1, status: status);
        if (status == ExpectedTransactionStatus.Pending) expected.ActualTransactionId = ActualId;
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.AdjustExpectedTransactionAsync(expected.Id, amount, reason);

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
    public async Task AdjustExpectedTransactionAsync_ShouldReturnPersistenceCodeAndMessage_WhenWriteOrCommitFails(
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
        var result = await fixture.Service.AdjustExpectedTransactionAsync(expected.Id, 120m, "Reading correction");

        // Assert
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.PersistenceError,
            CoreFinanceErrorCodes.PersistenceError, CoreFinanceMessages.OperationCouldNotBeSaved);
        fixture.Transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
