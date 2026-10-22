using CoreFinance.Application.Enums;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Deleting a pending or overdue period marks it deleted without manufacturing an actual transaction.<br />
    ///     (VI) Xóa kỳ chờ hoặc quá hạn đánh dấu đã xóa mà không tạo giao dịch thực.
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    public async Task DeletePendingAsync_ShouldMarkPeriodDeleted_WhenPeriodIsPending(int days)
    {
        // Arrange
        var expected = Expected(1, days);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.DeletePendingAsync(expected.Id);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Success);
        expected.IsDeleted.Should().NotBeNullOrWhiteSpace();
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ExpectedAmount.Should().Be(100m);
        expected.ActualTransactionId.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Terminal periods cannot be deleted through the pending-only command.<br />
    ///     (VI) Kỳ kết thúc không được xóa qua command chỉ dành cho Pending.
    /// </summary>
    [Theory]
    [InlineData(ExpectedTransactionStatus.Confirmed)]
    [InlineData(ExpectedTransactionStatus.Cancelled)]
    [InlineData(ExpectedTransactionStatus.Completed)]
    public async Task DeletePendingAsync_ShouldReturnConflict_WhenPeriodIsTerminal(ExpectedTransactionStatus status)
    {
        // Arrange
        var expected = Expected(1, status: status);
        expected.ActualTransactionId = ActualId;
        expected.ProcessedAt = Today.AddDays(-1);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.DeletePendingAsync(expected.Id);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Conflict);
        expected.IsDeleted.Should().BeNull();
        expected.Status.Should().Be(status);
        expected.ActualTransactionId.Should().Be(ActualId);
        expected.ProcessedAt.Should().Be(Today.AddDays(-1));
    }

    /// <summary>
    ///     (EN) An unavailable expected reference is not a successful deletion.<br />
    ///     (VI) Tham chiếu kỳ không được trả về không phải xóa thành công.
    /// </summary>
    [Fact]
    public async Task DeletePendingAsync_ShouldReturnNotFound_WhenExpectedReferenceIsUnavailable()
    {
        // Arrange
        var fixture = CreateFixture();

        // Act
        var result = await fixture.Service.DeletePendingAsync(Id(1));

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.NotFound);
        result.Code.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    ///     (EN) Write and commit failures return persistence errors; this does not prove database rollback.<br />
    ///     (VI) Lỗi ghi hoặc commit trả lỗi lưu trữ; không chứng minh rollback cơ sở dữ liệu.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DeletePendingAsync_ShouldReturnPersistenceError_WhenWriteOrCommitFails(bool commit)
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
        var result = await fixture.Service.DeletePendingAsync(expected.Id);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.PersistenceError);
        result.Code.Should().NotBeNullOrWhiteSpace();
    }
}
