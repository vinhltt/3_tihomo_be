using CoreFinance.Application.Enums;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

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
}
