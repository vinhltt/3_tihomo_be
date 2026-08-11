using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace CoreFinance.Application.Tests.TransactionServiceTests;

/// <summary>
///     Contains test cases for the GetByIdAsync method of TransactionService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức GetByIdAsync của TransactionService. (VI)
/// </summary>
public partial class TransactionServiceTests
{
    /// <summary>
    ///     Verifies that GetByIdAsync returns the correct transaction ViewModel when the transaction exists. (EN)<br />
    ///     Xác minh rằng GetByIdAsync trả về đúng ViewModel của giao dịch khi giao dịch tồn tại. (VI)
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ShouldReturnTransaction_WhenTransactionExists()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<TransactionService>>();
        var repositoryMock = new Mock<IBaseRepository<Transaction, Guid>>();

        var transactionId = Guid.CreateVersion7();
        var transaction = new Transaction { Id = transactionId, Description = "Test Transaction" };

        unitOfWorkMock.Setup(uow => uow.Repository<Transaction, Guid>()).Returns(repositoryMock.Object);
        repositoryMock.Setup(repo => repo.GetByIdNoTrackingAsync(transactionId,
            It.IsAny<Expression<Func<Transaction, object>>[]>())).ReturnsAsync(transaction);

        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetByIdAsync(transactionId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(transactionId);
        result.Description.Should().Be("Test Transaction");
    }

    /// <summary>
    ///     Verifies that GetByIdAsync returns null when the transaction does not exist in the repository. (EN)<br />
    ///     Xác minh rằng GetByIdAsync trả về null khi giao dịch không tồn tại trong repository. (VI)
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTransactionDoesNotExist()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<TransactionService>>();
        var repositoryMock = new Mock<IBaseRepository<Transaction, Guid>>();

        var transactionId = Guid.CreateVersion7();

        unitOfWorkMock.Setup(uow => uow.Repository<Transaction, Guid>()).Returns(repositoryMock.Object);
        repositoryMock
            .Setup(repo => repo.GetByIdNoTrackingAsync(transactionId,
                It.IsAny<Expression<Func<Transaction, object>>[]>()))
            .ReturnsAsync((Transaction?)null);

        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetByIdAsync(transactionId);

        // Assert
        result.Should().BeNull();
    }
}