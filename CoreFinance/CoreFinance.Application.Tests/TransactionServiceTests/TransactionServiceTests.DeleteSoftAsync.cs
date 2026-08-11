using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CoreFinance.Application.Tests.TransactionServiceTests;

/// <summary>
///     Contains test cases for the soft deletion methods of TransactionService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho các phương thức xóa mềm của TransactionService. (VI)
/// </summary>
public partial class TransactionServiceTests
{
    /// <summary>
    ///     Verifies that DeleteSoftAsync returns one when a transaction is successfully soft deleted. (EN)<br />
    ///     Xác minh rằng DeleteSoftAsync trả về một khi một giao dịch được xóa mềm thành công. (VI)
    /// </summary>
    [Fact]
    public async Task DeleteSoftAsync_ShouldReturnOne_WhenTransactionIsDeleted()
    {
        // Arrange
        var transactionId = Guid.CreateVersion7();
        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.DeleteSoftAsync(transactionId)).ReturnsAsync(1);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.DeleteSoftAsync(transactionId);

        // Assert
        result.Should().Be(1);
    }

    /// <summary>
    ///     Verifies that DeleteSoftAsync returns the correct affected count when soft deletion is successful. (EN)<br />
    ///     Xác minh rằng DeleteSoftAsync trả về số bản ghi bị ảnh hưởng chính xác khi xóa mềm thành công. (VI)
    /// </summary>
    [Fact]
    public async Task DeleteSoftAsync_ShouldReturnAffectedCount_WhenDeletionIsSuccessful()
    {
        // Arrange
        var transactionId = Guid.CreateVersion7();
        var expectedAffectedCount = 1;
        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.DeleteSoftAsync(transactionId)).ReturnsAsync(expectedAffectedCount);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.DeleteSoftAsync(transactionId);

        // Assert
        result.Should().Be(expectedAffectedCount);
        repoMock.Verify(r => r.DeleteSoftAsync(transactionId), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    /// <summary>
    ///     Verifies that DeleteSoftAsync returns zero when the entity to soft delete does not exist. (EN)<br />
    ///     Xác minh rằng DeleteSoftAsync trả về không khi thực thể cần xóa mềm không tồn tại. (VI)
    /// </summary>
    [Fact]
    public async Task DeleteSoftAsync_ShouldReturnZero_WhenEntityDoesNotExist()
    {
        // Arrange
        var transactionId = Guid.CreateVersion7();
        var expectedAffectedCount = 0;
        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.DeleteSoftAsync(transactionId)).ReturnsAsync(expectedAffectedCount);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.DeleteSoftAsync(transactionId);

        // Assert
        result.Should().Be(expectedAffectedCount);
        repoMock.Verify(r => r.DeleteSoftAsync(transactionId), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    /// <summary>
    ///     Verifies that DeleteSoftAsync throws an exception when the repository's soft delete operation throws an exception.
    ///     (EN)<br />
    ///     Xác minh rằng DeleteSoftAsync ném ra một ngoại lệ khi thao tác xóa mềm của repository ném ra một ngoại lệ. (VI)
    /// </summary>
    [Fact]
    public async Task DeleteSoftAsync_ShouldThrowException_WhenRepositoryThrowsException()
    {
        // Arrange
        var transactionId = Guid.CreateVersion7();
        var expectedException = new InvalidOperationException("Database error during soft delete");
        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.DeleteSoftAsync(transactionId)).ThrowsAsync(expectedException);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.DeleteSoftAsync(transactionId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Database error during soft delete");
        repoMock.Verify(r => r.DeleteSoftAsync(transactionId), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    /// <summary>
    ///     Verifies that DeleteSoftAsync handles different affected counts returned by the repository. (EN)<br />
    ///     Xác minh rằng DeleteSoftAsync xử lý các số lượng bản ghi bị ảnh hưởng khác nhau được trả về bởi repository. (VI)
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteSoftAsync_ShouldHandleDifferentAffectedCounts(int affectedCount)
    {
        // Arrange
        var transactionId = Guid.CreateVersion7();
        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.DeleteSoftAsync(transactionId)).ReturnsAsync(affectedCount);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.DeleteSoftAsync(transactionId);

        // Assert
        result.Should().Be(affectedCount);
        repoMock.Verify(r => r.DeleteSoftAsync(transactionId), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}