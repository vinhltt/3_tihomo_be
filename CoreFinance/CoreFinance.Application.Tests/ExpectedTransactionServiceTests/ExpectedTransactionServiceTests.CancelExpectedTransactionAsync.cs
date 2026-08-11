using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

/// <summary>
///     Contains test cases for the CancelExpectedTransactionAsync method of ExpectedTransactionService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức CancelExpectedTransactionAsync của ExpectedTransactionService. (VI)
/// </summary>
public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     Verifies that CancelExpectedTransactionAsync returns true and updates the transaction status and reason when
    ///     cancellation is successful. (EN)<br />
    ///     Xác minh rằng CancelExpectedTransactionAsync trả về true và cập nhật trạng thái giao dịch cùng lý do khi hủy bỏ
    ///     thành công. (VI)
    /// </summary>
    [Fact]
    public async Task CancelExpectedTransactionAsync_ShouldReturnTrue_WhenTransactionIsCancelledSuccessfully()
    {
        // Arrange
        var expectedTransactionId = Guid.CreateVersion7();
        var reason = "User requested cancellation";
        var expectedTransaction = new ExpectedTransaction
        {
            Id = expectedTransactionId,
            Status = ExpectedTransactionStatus.Pending,
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            ExpectedAmount = 100m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetByIdAsync(expectedTransactionId))
            .ReturnsAsync(expectedTransaction);
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>()))
            .ReturnsAsync(1);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CancelExpectedTransactionAsync(expectedTransactionId, reason);

        // Assert
        result.Should().BeTrue();
        expectedTransaction.Status.Should().Be(ExpectedTransactionStatus.Cancelled);
        expectedTransaction.AdjustmentReason.Should().Be(reason);
        expectedTransaction.ProcessedAt.Should().NotBeNull();
        expectedTransaction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        repoMock.Verify(r => r.GetByIdAsync(expectedTransactionId), Times.Once);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>()), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     Verifies that CancelExpectedTransactionAsync returns false when the expected transaction to cancel is not found.
    ///     (EN)<br />
    ///     Xác minh rằng CancelExpectedTransactionAsync trả về false khi không tìm thấy giao dịch dự kiến cần hủy bỏ. (VI)
    /// </summary>
    [Fact]
    public async Task CancelExpectedTransactionAsync_ShouldReturnFalse_WhenTransactionNotFound()
    {
        // Arrange
        var expectedTransactionId = Guid.CreateVersion7();
        var reason = "User requested cancellation";

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetByIdAsync(expectedTransactionId))
            .ReturnsAsync((ExpectedTransaction?)null);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CancelExpectedTransactionAsync(expectedTransactionId, reason);

        // Assert
        result.Should().BeFalse();
        repoMock.Verify(r => r.GetByIdAsync(expectedTransactionId), Times.Once);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    /// <summary>
    ///     Verifies that CancelExpectedTransactionAsync returns false when the expected transaction is not in the Pending
    ///     status. (EN)<br />
    ///     Xác minh rằng CancelExpectedTransactionAsync trả về false khi giao dịch dự kiến không ở trạng thái Đang chờ xử lý.
    ///     (VI)
    /// </summary>
    [Fact]
    public async Task CancelExpectedTransactionAsync_ShouldReturnFalse_WhenTransactionIsNotPending()
    {
        // Arrange
        var expectedTransactionId = Guid.CreateVersion7();
        var reason = "User requested cancellation";
        var expectedTransaction = new ExpectedTransaction
        {
            Id = expectedTransactionId,
            Status = ExpectedTransactionStatus.Confirmed, // Already confirmed
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            ExpectedAmount = 100m
        };

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetByIdAsync(expectedTransactionId))
            .ReturnsAsync(expectedTransaction);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CancelExpectedTransactionAsync(expectedTransactionId, reason);

        // Assert
        result.Should().BeFalse();
        repoMock.Verify(r => r.GetByIdAsync(expectedTransactionId), Times.Once);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    /// <summary>
    ///     Verifies that CancelExpectedTransactionAsync returns false and logs an error when an exception occurs during the
    ///     cancellation process. (EN)<br />
    ///     Xác minh rằng CancelExpectedTransactionAsync trả về false và ghi log lỗi khi có ngoại lệ xảy ra trong quá trình hủy
    ///     bỏ. (VI)
    /// </summary>
    [Fact]
    public async Task CancelExpectedTransactionAsync_ShouldReturnFalse_WhenExceptionOccurs()
    {
        // Arrange
        var expectedTransactionId = Guid.CreateVersion7();
        var reason = "User requested cancellation";

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetByIdAsync(expectedTransactionId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CancelExpectedTransactionAsync(expectedTransactionId, reason);

        // Assert
        result.Should().BeFalse();
        repoMock.Verify(r => r.GetByIdAsync(expectedTransactionId), Times.Once);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify that error was logged
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error cancelling expected transaction")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}