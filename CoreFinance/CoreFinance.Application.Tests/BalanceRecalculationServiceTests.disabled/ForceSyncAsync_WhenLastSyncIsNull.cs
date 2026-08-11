using CoreFinance.Application.Services;
using CoreFinance.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CoreFinance.Application.Tests.BalanceRecalculationServiceTests;

/// <summary>
///     Tests for ForceSyncAsync when LastSync is null (EN)<br/>
///     Kiểm thử ForceSyncAsync khi LastSync là null (VI)
/// </summary>
public partial class BalanceRecalculationServiceTests
{
    /// <summary>
    ///     T008: Verifies that ForceSyncAsync recalculates all transactions when LastSync is null (EN)<br/>
    ///     T008: Xác minh ForceSyncAsync tính toán lại tất cả giao dịch khi LastSync là null (VI)
    /// </summary>
    [Fact]
    public async Task ForceSyncAsync_WhenLastSyncIsNull_RecalculatesAllTransactions()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var accountFaker = CreateAccountFaker(userId);
        var account = accountFaker.Generate();
        account.LastSync = null;
        account.InitialBalance = 1000m;

        var transactionFaker = CreateTransactionFaker(account.Id, userId);
        var transactions = new List<Transaction>
        {
            transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => DateTime.UtcNow.AddDays(-3))
                .RuleFor(t => t.RevenueAmount, _ => 500m)
                .RuleFor(t => t.SpentAmount, _ => 0m)
                .Generate(),
            transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => DateTime.UtcNow.AddDays(-2))
                .RuleFor(t => t.RevenueAmount, _ => 0m)
                .RuleFor(t => t.SpentAmount, _ => 200m)
                .Generate(),
            transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => DateTime.UtcNow.AddDays(-1))
                .RuleFor(t => t.RevenueAmount, _ => 300m)
                .RuleFor(t => t.SpentAmount, _ => 0m)
                .Generate()
        };

        var transactionIds = transactions.Select(t => t.Id).ToList();

        var (accountRepoMock, transactionRepoMock, loggerMock) = CreateMocks();

        // Setup mocks
        accountRepoMock.Setup(r => r.GetByIdAsync(account.Id))
            .ReturnsAsync(account);

        transactionRepoMock.Setup(r => r.GetTransactionIdsAsync(account.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionIds);

        transactionRepoMock.Setup(r => r.LoadBatchAsync(transactionIds, 0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        transactionRepoMock.Setup(r => r.UpdateAsync(It.IsAny<IEnumerable<Transaction>>()))
            .ReturnsAsync(transactions.Count);

        transactionRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(new List<Transaction>().AsQueryable());

        var service = new BalanceRecalculationService(
            accountRepoMock.Object,
            transactionRepoMock.Object,
            loggerMock.Object);

        // Act
        var result = await service.ForceSyncAsync(account.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.AccountId.Should().Be(account.Id);
        result.TransactionsRecalculated.Should().Be(3);

        // Verify balance calculations
        transactions[0].Balance.Should().Be(1500m); // 1000 + 500 (revenue)
        transactions[1].Balance.Should().Be(1300m); // 1500 - 200 (spent)
        transactions[2].Balance.Should().Be(1600m); // 1300 + 300 (revenue)

        // Verify repository calls
        accountRepoMock.Verify(r => r.GetByIdAsync(account.Id), Times.Once);
        transactionRepoMock.Verify(r => r.GetTransactionIdsAsync(account.Id, null, It.IsAny<CancellationToken>()), Times.Once);
        transactionRepoMock.Verify(r => r.LoadBatchAsync(transactionIds, 0, 100, It.IsAny<CancellationToken>()), Times.Once);
        transactionRepoMock.Verify(r => r.UpdateAsync(It.IsAny<IEnumerable<Transaction>>()), Times.Once);
    }
}
