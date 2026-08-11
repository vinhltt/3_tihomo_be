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
///     Contains test cases for the AdjustExpectedTransactionAsync method of ExpectedTransactionService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức AdjustExpectedTransactionAsync của ExpectedTransactionService. (VI)
/// </summary>
public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     Verifies that AdjustExpectedTransactionAsync returns true and updates the transaction when the adjustment is
    ///     successful. (EN)<br />
    ///     Xác minh rằng AdjustExpectedTransactionAsync trả về true và cập nhật giao dịch khi việc điều chỉnh thành công. (VI)
    /// </summary>
    [Fact]
    public async Task AdjustExpectedTransactionAsync_ShouldReturnTrue_WhenTransactionIsAdjustedSuccessfully()
    {
        // Arrange
        var expectedTransactionId = Guid.CreateVersion7();
        var newAmount = 150.75m;
        var reason = "Price increase adjustment";
        var expectedTransaction = new ExpectedTransaction
        {
            Id = expectedTransactionId,
            Status = ExpectedTransactionStatus.Pending,
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            ExpectedAmount = 100m,
            IsAdjusted = false,
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
        var result = await service.AdjustExpectedTransactionAsync(expectedTransactionId, newAmount, reason);

        // Assert
        result.Should().BeTrue();
        expectedTransaction.ExpectedAmount.Should().Be(newAmount);
        expectedTransaction.OriginalAmount.Should().Be(100m); // Should be set to original amount
        expectedTransaction.IsAdjusted.Should().BeTrue();
        expectedTransaction.AdjustmentReason.Should().Be(reason);
        expectedTransaction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        repoMock.Verify(r => r.GetByIdAsync(expectedTransactionId), Times.Once);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>()), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     Verifies that AdjustExpectedTransactionAsync does not override the original amount if the transaction has already
    ///     been adjusted. (EN)<br />
    ///     Xác minh rằng AdjustExpectedTransactionAsync không ghi đè số tiền gốc nếu giao dịch đã được điều chỉnh. (VI)
    /// </summary>
    [Fact]
    public async Task AdjustExpectedTransactionAsync_ShouldNotOverrideOriginalAmount_WhenAlreadyAdjusted()
    {
        // Arrange
        var expectedTransactionId = Guid.CreateVersion7();
        var newAmount = 150.75m;
        var reason = "Second adjustment";
        var expectedTransaction = new ExpectedTransaction
        {
            Id = expectedTransactionId,
            Status = ExpectedTransactionStatus.Pending,
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            ExpectedAmount = 120m, // Already adjusted from original 100
            OriginalAmount = 100m, // Original amount already set
            IsAdjusted = true,
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
        var result = await service.AdjustExpectedTransactionAsync(expectedTransactionId, newAmount, reason);

        // Assert
        result.Should().BeTrue();
        expectedTransaction.ExpectedAmount.Should().Be(newAmount);
        expectedTransaction.OriginalAmount.Should().Be(100m); // Should remain the original amount, not overridden
        expectedTransaction.IsAdjusted.Should().BeTrue();
        expectedTransaction.AdjustmentReason.Should().Be(reason);

        repoMock.Verify(r => r.GetByIdAsync(expectedTransactionId), Times.Once);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>()), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     Verifies that AdjustExpectedTransactionAsync returns false when the transaction to adjust is not found. (EN)<br />
    ///     Xác minh rằng AdjustExpectedTransactionAsync trả về false khi không tìm thấy giao dịch cần điều chỉnh. (VI)
    /// </summary>
    [Fact]
    public async Task AdjustExpectedTransactionAsync_ShouldReturnFalse_WhenTransactionNotFound()
    {
        // Arrange
        var expectedTransactionId = Guid.CreateVersion7();
        var newAmount = 150.75m;
        var reason = "Adjustment reason";

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
        var result = await service.AdjustExpectedTransactionAsync(expectedTransactionId, newAmount, reason);

        // Assert
        result.Should().BeFalse();
        repoMock.Verify(r => r.GetByIdAsync(expectedTransactionId), Times.Once);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    /// <summary>
    ///     Verifies that AdjustExpectedTransactionAsync returns false when the transaction is not in the Pending status. (EN)
    ///     <br />
    ///     Xác minh rằng AdjustExpectedTransactionAsync trả về false khi giao dịch không ở trạng thái Đang chờ xử lý. (VI)
    /// </summary>
    [Fact]
    public async Task AdjustExpectedTransactionAsync_ShouldReturnFalse_WhenTransactionIsNotPending()
    {
        // Arrange
        var expectedTransactionId = Guid.CreateVersion7();
        var newAmount = 150.75m;
        var reason = "Adjustment reason";
        var expectedTransaction = new ExpectedTransaction
        {
            Id = expectedTransactionId,
            Status = ExpectedTransactionStatus.Confirmed, // Not pending
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
        var result = await service.AdjustExpectedTransactionAsync(expectedTransactionId, newAmount, reason);

        // Assert
        result.Should().BeFalse();
        repoMock.Verify(r => r.GetByIdAsync(expectedTransactionId), Times.Once);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    /// <summary>
    ///     Verifies that AdjustExpectedTransactionAsync returns false and logs an error when an exception occurs during the
    ///     process. (EN)<br />
    ///     Xác minh rằng AdjustExpectedTransactionAsync trả về false và ghi log lỗi khi có ngoại lệ xảy ra trong quá trình.
    ///     (VI)
    /// </summary>
    [Fact]
    public async Task AdjustExpectedTransactionAsync_ShouldReturnFalse_WhenExceptionOccurs()
    {
        // Arrange
        var expectedTransactionId = Guid.CreateVersion7();
        var newAmount = 150.75m;
        var reason = "Adjustment reason";

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
        var result = await service.AdjustExpectedTransactionAsync(expectedTransactionId, newAmount, reason);

        // Assert
        result.Should().BeFalse();
        repoMock.Verify(r => r.GetByIdAsync(expectedTransactionId), Times.Once);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify that error was logged
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error adjusting expected transaction")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}