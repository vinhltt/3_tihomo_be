using CoreFinance.Application.DTOs.Transaction;
using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;

namespace CoreFinance.Application.Tests.TransactionServiceTests;

/// <summary>
///     Contains test cases for the GetAllDtoAsync method of TransactionService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức GetAllDtoAsync của TransactionService. (VI)
/// </summary>
public partial class TransactionServiceTests
{
    /// <summary>
    ///     Verifies that GetAllDtoAsync returns all transactions correctly when transactions exist. (EN)<br />
    ///     Xác minh rằng GetAllDtoAsync trả về tất cả các giao dịch một cách chính xác khi có giao dịch tồn tại. (VI)
    /// </summary>
    [Fact]
    public async Task GetAllDtoAsync_ShouldReturnAllTransactions_WhenTransactionsExist()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new() { Id = Guid.CreateVersion7(), Description = "Salary", RevenueAmount = 1000, SpentAmount = 0 },
            new() { Id = Guid.CreateVersion7(), Description = "Groceries", RevenueAmount = 0, SpentAmount = 200 },
            new() { Id = Guid.CreateVersion7(), Description = "Transfer", RevenueAmount = 0, SpentAmount = 500 }
        };

        var transactionsMock = transactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities()).Returns(transactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<TransactionService>>();

        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetAllDtoAsync();

        // Assert
        var transactionViewModels = result!.ToList();
        transactionViewModels.Should().NotBeNull();
        transactionViewModels.Should().HaveCount(transactions.Count);
        var expectedViewModels = transactions.Select(_mapper.Map<TransactionViewModel>).ToList();
        transactionViewModels.Should().BeEquivalentTo(expectedViewModels);
        repoMock.Verify(r => r.GetNoTrackingEntities(), Times.Once);
    }

    /// <summary>
    ///     Verifies that GetAllDtoAsync returns an empty list when no transactions exist in the repository. (EN)<br />
    ///     Xác minh rằng GetAllDtoAsync trả về danh sách rỗng khi không có giao dịch nào tồn tại trong repository. (VI)
    /// </summary>
    [Fact]
    public async Task GetAllDtoAsync_ShouldReturnEmptyList_WhenNoTransactionsExist()
    {
        // Arrange
        var emptyTransactions = new List<Transaction>().AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities()).Returns(emptyTransactions);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<TransactionService>>();

        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetAllDtoAsync();

        // Assert
        var transactionViewModels = result!.ToList();
        transactionViewModels.Should().NotBeNull();
        transactionViewModels.Should().BeEmpty();
        repoMock.Verify(r => r.GetNoTrackingEntities(), Times.Once);
    }
}