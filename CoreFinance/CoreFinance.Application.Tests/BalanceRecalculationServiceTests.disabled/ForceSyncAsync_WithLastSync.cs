using CoreFinance.Application.Services;
using CoreFinance.Domain.Entities;
using FluentAssertions;
using MockQueryable;
using Moq;

namespace CoreFinance.Application.Tests.BalanceRecalculationServiceTests;

/// <summary>
///     Tests for ForceSyncAsync with LastSync checkpoint (EN)<br/>
///     Kiểm thử ForceSyncAsync với checkpoint LastSync (VI)
/// </summary>
public partial class BalanceRecalculationServiceTests
{
    /// <summary>
    ///     T009: Verifies that ForceSyncAsync only recalculates transactions after LastSync (EN)<br/>
    ///     T009: Xác minh ForceSyncAsync chỉ tính toán lại giao dịch sau LastSync (VI)
    /// </summary>
    [Fact]
    public async Task ForceSyncAsync_WithLastSync_OnlyRecalculatesTransactionsAfterCheckpoint()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var accountFaker = CreateAccountFaker(userId);
        var account = accountFaker.Generate();

        var checkpointDate = DateTime.UtcNow.AddDays(-5);
        account.LastSync = checkpointDate;
        account.InitialBalance = 1000m;

        var transactionFaker = CreateTransactionFaker(account.Id, userId);

        // Transactions BEFORE LastSync (should NOT be recalculated)
        var oldTransactions = new List<Transaction>
        {
            transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => checkpointDate.AddDays(-2))
                .RuleFor(t => t.RevenueAmount, _ => 500m)
                .RuleFor(t => t.Balance, _ => 1500m) // Already calculated
                .Generate()
        };

        // Transactions AFTER LastSync (should be recalculated)
        var newTransactions = new List<Transaction>
        {
            transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => checkpointDate.AddDays(1))
                .RuleFor(t => t.RevenueAmount, _ => 0m)
                .RuleFor(t => t.SpentAmount, _ => 200m)
                .Generate(),
            transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => checkpointDate.AddDays(2))
                .RuleFor(t => t.RevenueAmount, _ => 300m)
                .RuleFor(t => t.SpentAmount, _ => 0m)
                .Generate()
        };

        var newTransactionIds = newTransactions.Select(t => t.Id).ToList();

        var (accountRepoMock, transactionRepoMock, loggerMock) = CreateMocks();

        // Setup mocks
        accountRepoMock.Setup(r => r.GetByIdAsync(account.Id))
            .ReturnsAsync(account);

        // Only return transactions AFTER LastSync
        transactionRepoMock.Setup(r => r.GetTransactionIdsAsync(account.Id, checkpointDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newTransactionIds);

        transactionRepoMock.Setup(r => r.LoadBatchAsync(newTransactionIds, 0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newTransactions);

        transactionRepoMock.Setup(r => r.UpdateAsync(It.IsAny<IEnumerable<Transaction>>()))
            .ReturnsAsync(newTransactions.Count);

        // Setup GetNoTrackingEntities to return the last synced transaction
        var lastSyncedTransaction = oldTransactions.Last();
        var queryableTransactions = new List<Transaction> { lastSyncedTransaction }.AsQueryable().BuildMock();
        transactionRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(queryableTransactions);

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
        result.TransactionsRecalculated.Should().Be(2); // Only 2 new transactions

        // Verify balance calculations starting from last synced balance (1500m)
        newTransactions[0].Balance.Should().Be(1300m); // 1500 - 200
        newTransactions[1].Balance.Should().Be(1600m); // 1300 + 300

        // Verify only NEW transactions were queried and updated
        transactionRepoMock.Verify(r => r.GetTransactionIdsAsync(account.Id, checkpointDate, It.IsAny<CancellationToken>()), Times.Once);
        transactionRepoMock.Verify(r => r.UpdateAsync(It.Is<IEnumerable<Transaction>>(t => t.Count() == 2)), Times.Once);
    }
}
