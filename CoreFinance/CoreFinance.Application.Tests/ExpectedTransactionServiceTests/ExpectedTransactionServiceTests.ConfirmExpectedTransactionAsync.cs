using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Enums;
using CoreFinance.Domain.Constants;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Linking an available same-owner transaction confirms even a transfer without creating a new actual.<br />
    ///     (VI) Liên kết giao dịch cùng chủ hiện có xác nhận cả kỳ chuyển khoản mà không tạo giao dịch thực mới.
    /// </summary>
    [Theory]
    [InlineData(RecurringTransactionType.Income)]
    [InlineData(RecurringTransactionType.Expense)]
    [InlineData(RecurringTransactionType.Transfer)]
    public async Task ConfirmExpectedTransactionAsync_ShouldConfirmWithExistingActual_WhenReferenceIsAvailable(
        RecurringTransactionType type)
    {
        // Arrange
        var expected = Expected(1, -1, type: type);
        var fixture = CreateFixture(expected);
        fixture.ActualTransactions.Add(new Transaction { Id = ActualId, UserId = OwnerId, AccountId = AccountId });

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id, ActualId);
        var view = await fixture.Service.GetByIdAsync(expected.Id);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Success);
        result.ExpectedTransactionId.Should().Be(expected.Id);
        result.ActualTransactionId.Should().Be(ActualId);
        view.Should().NotBeNull();
        view!.Status.Should().Be(ExpectedTransactionStatus.Confirmed);
        view.DisplayStatus.Should().Be(1);
        view.ActualTransactionId.Should().Be(ActualId);
        view.HasActualTransaction.Should().BeTrue();
        view.ProcessedAt.Should().NotBeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) An unavailable expected reference is reported as not found, not a successful link.<br />
    ///     (VI) Kỳ không được repository trả về được báo không tìm thấy, không phải liên kết thành công.
    /// </summary>
    [Fact]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnNotFound_WhenExpectedReferenceIsUnavailable()
    {
        // Arrange
        var fixture = CreateFixture();

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(Id(1), ActualId);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.NotFound);
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Missing and mismatched-owner actual references are rejected by service guards only.<br />
    ///     (VI) Tham chiếu thực thiếu hoặc khác chủ bị guard dịch vụ từ chối; không chứng minh cách ly repository.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnNotFound_WhenActualReferenceIsMissingOrForeign(
        bool foreign)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        if (foreign)
            fixture.ActualTransactions.Add(new Transaction { Id = ActualId, UserId = OtherOwnerId });

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id, ActualId);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.NotFound);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ActualTransactionId.Should().BeNull();
        expected.ProcessedAt.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Empty actual IDs are invalid input and leave the period pending.<br />
    ///     (VI) ID giao dịch thực rỗng là đầu vào không hợp lệ và giữ kỳ chờ xử lý.
    /// </summary>
    [Fact]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnInvalidRequest_WhenActualIdIsEmpty()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id, Guid.Empty);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.InvalidRequest);
        expected.ActualTransactionId.Should().BeNull();
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) A visible existing link, including a tombstone, prevents reusing an actual reference.<br />
    ///     (VI) Liên kết hiện có được trả về, kể cả kỳ đã xóa mềm, chặn dùng lại giao dịch thực.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnConflict_WhenActualIsAlreadyLinked(bool deleted)
    {
        // Arrange
        var expected = Expected(1);
        var linked = Expected(2, status: ExpectedTransactionStatus.Confirmed);
        linked.ActualTransactionId = ActualId;
        linked.IsDeleted = deleted ? "20261008090000" : null;
        var fixture = CreateFixture(expected, linked);
        fixture.ActualTransactions.Add(new Transaction { Id = ActualId, UserId = OwnerId });

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id, ActualId);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Conflict);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ActualTransactionId.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Every terminal state rejects linking without replacing its existing lifecycle data.<br />
    ///     (VI) Mọi trạng thái kết thúc từ chối liên kết mà không thay dữ liệu vòng đời hiện có.
    /// </summary>
    [Theory]
    [InlineData(ExpectedTransactionStatus.Confirmed)]
    [InlineData(ExpectedTransactionStatus.Cancelled)]
    [InlineData(ExpectedTransactionStatus.Completed)]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnConflict_WhenPeriodIsNotPending(
        ExpectedTransactionStatus status)
    {
        // Arrange
        var expected = Expected(1, status: status);
        expected.ActualTransactionId = Id(20);
        expected.ProcessedAt = Today.AddDays(-1);
        var fixture = CreateFixture(expected);
        fixture.ActualTransactions.Add(new Transaction { Id = ActualId, UserId = OwnerId });

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id, ActualId);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Conflict);
        expected.Status.Should().Be(status);
        expected.ActualTransactionId.Should().Be(Id(20));
        expected.ProcessedAt.Should().Be(Today.AddDays(-1));
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Write and commit failures return persistence errors; mocks do not prove rollback.<br />
    ///     (VI) Lỗi ghi hoặc commit trả lỗi lưu trữ; mock không chứng minh rollback.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnPersistenceError_WhenWriteOrCommitFails(bool commit)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.ActualTransactions.Add(new Transaction { Id = ActualId, UserId = OwnerId });
        if (commit)
            fixture.Transaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Commit failed"));
        else
            fixture.UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Write failed"));

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id, ActualId);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.PersistenceError);
        result.ActualTransaction.Should().BeNull();
    }

    /// <summary>
    ///     (EN) A same-owner actual tombstone cannot be linked and leaves the expected period unprocessed.<br />
    ///     (VI) Giao dịch thực xóa mềm cùng chủ không được liên kết và giữ kỳ chưa xử lý.
    /// </summary>
    [Fact]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnNotFound_WhenActualIsSoftDeleted()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.ActualTransactions.Add(new Transaction
        {
            Id = ActualId, UserId = OwnerId, AccountId = AccountId, IsDeleted = "20261008120000"
        });

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id, ActualId);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.NotFound);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ActualTransactionId.Should().BeNull();
        expected.ProcessedAt.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    private static void AssertActionFailure(ExpectedTransactionActionResult result, Guid expectedId,
        ExpectedTransactionActionOutcome outcome, string code, string message)
    {
        result.Outcome.Should().Be(outcome);
        result.Code.Should().Be(code);
        result.Message.Should().Be(message);
        result.ExpectedTransactionId.Should().Be(expectedId);
        result.ActualTransactionId.Should().BeNull();
        result.ActualTransaction.Should().BeNull();
    }

    private static void AssertNothingWritten(ServiceFixture fixture)
    {
        fixture.ExpectedRepository.Verify(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>(),
            It.IsAny<CancellationToken>()), Times.Never);
        fixture.ExpectedRepository.Verify(r => r.UpdateAsync(It.IsAny<IEnumerable<ExpectedTransaction>>()),
            Times.Never);
        fixture.ExpectedRepository.Verify(r => r.CreateAsync(It.IsAny<ExpectedTransaction>(),
            It.IsAny<CancellationToken>()), Times.Never);
        fixture.ActualRepository.Verify(r => r.CreateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);
        fixture.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.Transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    private static void AssertPeriodUnchanged(ExpectedTransaction expected)
    {
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ExpectedAmount.Should().Be(100m);
        expected.OriginalAmount.Should().BeNull();
        expected.IsAdjusted.Should().BeFalse();
        expected.AdjustmentReason.Should().BeNull();
        expected.ActualTransactionId.Should().BeNull();
        expected.ProcessedAt.Should().BeNull();
    }

    /// <summary>
    ///     (EN) An invalid actual ID on an otherwise confirmable pending period writes nothing and leaves it unchanged.<br />
    ///     (VI) ID giao dịch thực không hợp lệ trên kỳ chờ có thể xác nhận không ghi gì và giữ nguyên kỳ.
    /// </summary>
    [Fact]
    public async Task ConfirmExpectedTransactionAsync_ShouldNotWriteAnything_WhenActualIdIsInvalid()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.ActualTransactions.Add(new Transaction { Id = ActualId, UserId = OwnerId, AccountId = AccountId });

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id, Guid.Empty);

        // Assert
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.InvalidRequest,
            CoreFinanceErrorCodes.InvalidActualTransaction, CoreFinanceMessages.ActualTransactionIdRequired);
        AssertNothingWritten(fixture);
        fixture.Transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        AssertPeriodUnchanged(expected);
    }

    /// <summary>
    ///     (EN) Characterization: a successful link reports the success code and message with the linked actual ID.<br />
    ///     (VI) Đặc tả hiện trạng: liên kết thành công trả mã và message thành công cùng ID giao dịch thực.
    /// </summary>
    [Fact]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnSuccessCodeAndMessage_WhenActualIsLinked()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.ActualTransactions.Add(new Transaction { Id = ActualId, UserId = OwnerId });

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id, ActualId);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Success);
        result.Code.Should().Be(CoreFinanceErrorCodes.Success);
        result.Message.Should().Be(CoreFinanceMessages.ExpectedTransactionConfirmed);
        result.ExpectedTransactionId.Should().Be(expected.Id);
        result.ActualTransactionId.Should().Be(ActualId);
    }

    /// <summary>
    ///     (EN) Characterization: a missing, foreign or soft-deleted actual reports the actual-not-found code and message.<br />
    ///     (VI) Đặc tả hiện trạng: giao dịch thực thiếu, khác chủ hoặc xóa mềm trả mã và message không tìm thấy giao dịch thực.
    /// </summary>
    [Theory]
    [InlineData("missing")]
    [InlineData("foreign")]
    [InlineData("deleted")]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnActualNotFoundCodeAndMessage_WhenActualIsUnavailable(
        string kind)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        if (kind == "foreign")
            fixture.ActualTransactions.Add(new Transaction { Id = ActualId, UserId = OtherOwnerId });
        if (kind == "deleted")
            fixture.ActualTransactions.Add(new Transaction
            {
                Id = ActualId, UserId = OwnerId, IsDeleted = "20261008120000"
            });

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id, ActualId);

        // Assert
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.NotFound,
            CoreFinanceErrorCodes.ActualTransactionNotFound, CoreFinanceMessages.ActualTransactionNotFound);
    }

    /// <summary>
    ///     (EN) Characterization: an actual already linked elsewhere, tombstone included, reports the already-linked code and message.<br />
    ///     (VI) Đặc tả hiện trạng: giao dịch thực đã liên kết nơi khác, kể cả kỳ xóa mềm, trả mã và message already_linked.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnAlreadyLinkedCodeAndMessage_WhenActualIsLinkedElsewhere(
        bool deleted)
    {
        // Arrange
        var expected = Expected(1);
        var linked = Expected(2, status: ExpectedTransactionStatus.Confirmed);
        linked.ActualTransactionId = ActualId;
        linked.IsDeleted = deleted ? "20261008090000" : null;
        var fixture = CreateFixture(expected, linked);
        fixture.ActualTransactions.Add(new Transaction { Id = ActualId, UserId = OwnerId });

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id, ActualId);

        // Assert
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.Conflict,
            CoreFinanceErrorCodes.AlreadyLinked, CoreFinanceMessages.ActualTransactionAlreadyLinked);
    }

    /// <summary>
    ///     (EN) Characterization: a non-pending or already linked period reports the state-conflict code and message, even when the input is also invalid.<br />
    ///     (VI) Đặc tả hiện trạng: kỳ không ở trạng thái chờ hoặc đã liên kết trả mã và message state_conflict, kể cả khi đầu vào cũng sai.
    /// </summary>
    [Theory]
    [InlineData(ExpectedTransactionStatus.Confirmed, false)]
    [InlineData(ExpectedTransactionStatus.Cancelled, true)]
    [InlineData(ExpectedTransactionStatus.Completed, false)]
    [InlineData(ExpectedTransactionStatus.Pending, true)]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnStateConflictCodeAndMessage_WhenPeriodIsNotAvailable(
        ExpectedTransactionStatus status, bool emptyActualId)
    {
        // Arrange
        var expected = Expected(1, status: status);
        if (status == ExpectedTransactionStatus.Pending) expected.ActualTransactionId = ActualId;
        var fixture = CreateFixture(expected);
        fixture.ActualTransactions.Add(new Transaction { Id = Id(904), UserId = OwnerId });

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id,
            emptyActualId ? Guid.Empty : Id(904));

        // Assert
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.Conflict,
            CoreFinanceErrorCodes.StateConflict, CoreFinanceMessages.PendingTransactionRequired);
    }

    /// <summary>
    ///     (EN) Characterization: an unknown or soft-deleted period reports not-found even when the actual ID is also invalid.<br />
    ///     (VI) Đặc tả hiện trạng: kỳ không tồn tại hoặc đã xóa mềm trả not_found kể cả khi ID giao dịch thực cũng sai.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnNotFoundCodeAndMessage_WhenPeriodIsMissingAndInputIsInvalid(
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
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(Id(1), Guid.Empty);

        // Assert
        AssertActionFailure(result, Id(1), ExpectedTransactionActionOutcome.NotFound,
            CoreFinanceErrorCodes.ExpectedTransactionNotFound, CoreFinanceMessages.ExpectedTransactionNotFound);
    }

    /// <summary>
    ///     (EN) Characterization: write or commit failures report the persistence code and message.<br />
    ///     (VI) Đặc tả hiện trạng: lỗi ghi hoặc commit trả mã và message lỗi lưu trữ.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnPersistenceCodeAndMessage_WhenWriteOrCommitFails(
        bool commit)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.ActualTransactions.Add(new Transaction { Id = ActualId, UserId = OwnerId });
        if (commit)
            fixture.Transaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Commit failed"));
        else
            fixture.UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Write failed"));

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id, ActualId);

        // Assert
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.PersistenceError,
            CoreFinanceErrorCodes.PersistenceError, CoreFinanceMessages.OperationCouldNotBeSaved);
        fixture.Transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     (EN) Characterization: a stale write reports the concurrency code and the period-changed message.<br />
    ///     (VI) Đặc tả hiện trạng: ghi trên bản cũ trả mã concurrency_conflict và message kỳ đã thay đổi.
    /// </summary>
    [Fact]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnConcurrencyCodeAndMessage_WhenWriteIsStale()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.ActualTransactions.Add(new Transaction { Id = ActualId, UserId = OwnerId });
        fixture.ExpectedRepository.Setup(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Stale period"));

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id, ActualId);

        // Assert
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.Conflict,
            CoreFinanceErrorCodes.ConcurrencyConflict, CoreFinanceMessages.PeriodChanged);
    }

    /// <summary>
    ///     (EN) Characterization: the actual-link unique violation reports the already-linked code and message.<br />
    ///     (VI) Đặc tả hiện trạng: vi phạm unique liên kết thực trả mã và message already_linked.
    /// </summary>
    [Fact]
    public async Task ConfirmExpectedTransactionAsync_ShouldReturnAlreadyLinkedCodeAndMessage_WhenUniqueIndexRejectsLink()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.ActualTransactions.Add(new Transaction { Id = ActualId, UserId = OwnerId });
        var postgresError = new PostgresException("Write rejected", "ERROR", "ERROR",
            PostgresErrorCodes.UniqueViolation, constraintName: "ux_expected_actual_transaction");
        fixture.ExpectedRepository.Setup(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Write failed", postgresError));

        // Act
        var result = await fixture.Service.ConfirmExpectedTransactionAsync(expected.Id, ActualId);

        // Assert
        AssertActionFailure(result, expected.Id, ExpectedTransactionActionOutcome.Conflict,
            CoreFinanceErrorCodes.AlreadyLinked, CoreFinanceMessages.ActualTransactionAlreadyLinked);
    }
}
