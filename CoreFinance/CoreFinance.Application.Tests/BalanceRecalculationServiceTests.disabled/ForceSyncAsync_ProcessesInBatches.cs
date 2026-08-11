using CoreFinance.Application.Services;
using CoreFinance.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CoreFinance.Application.Tests.BalanceRecalculationServiceTests;

/// <summary>
///     Tests for ForceSyncAsync batch processing (EN)<br/>
///     Kiểm thử xử lý batch của ForceSyncAsync (VI)
/// </summary>
public partial class BalanceRecalculationServiceTests
{
    /// <summary>
    ///     T013: Verifies that ForceSyncAsync processes transactions in batches of 100 (EN)<br/>
    ///     T013: Xác minh ForceSyncAsync xử lý giao dịch theo batch 100 (VI)
    /// </summary>
    [Fact]
    public async Task ForceSyncAsync_ProcessesInBatches_WhenMoreThan100Transactions()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var accountFaker = CreateAccountFaker(userId);
        var account = accountFaker.Generate();
        account.LastSync = null;
        account.InitialBalance = 1000m;

        // Generate 250 transactions to test multiple batches
        var transactionFaker = CreateTransactionFaker(account.Id, userId);
        var transactions = new List<Transaction>();

        for (int i = 0; i < 250; i++)
        {
            var transaction = transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => DateTime.UtcNow.AddDays(-250 + i))
                .RuleFor(t => t.RevenueAmount, _ => 10m)
                .RuleFor(t => t.SpentAmount, _ => 0m)
                .Generate();
            transactions.Add(transaction);
        }

        var transactionIds = transactions.Select(t => t.Id).ToList();

        var (accountRepoMock, transactionRepoMock, loggerMock) = CreateMocks();

        // Setup mocks
        accountRepoMock.Setup(r => r.GetByIdAsync(account.Id))
            .ReturnsAsync(account);

        transactionRepoMock.Setup(r => r.GetTransactionIdsAsync(account.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionIds);

        // Setup batch loading - should be called 3 times (100, 100, 50)
        transactionRepoMock.Setup(r => r.LoadBatchAsync(transactionIds, 0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions.GetRange(0, 100));

        transactionRepoMock.Setup(r => r.LoadBatchAsync(transactionIds, 100, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions.GetRange(100, 100));

        transactionRepoMock.Setup(r => r.LoadBatchAsync(transactionIds, 200, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions.GetRange(200, 50));

        transactionRepoMock.Setup(r => r.UpdateAsync(It.IsAny<IEnumerable<Transaction>>()))
            .ReturnsAsync((IEnumerable<Transaction> txns) => txns.Count());

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
        result.TransactionsRecalculated.Should().Be(250);

        // Verify LoadBatchAsync was called 3 times (for 3 batches)
        transactionRepoMock.Verify(r => r.LoadBatchAsync(transactionIds, 0, 100, It.IsAny<CancellationToken>()), Times.Once);
        transactionRepoMock.Verify(r => r.LoadBatchAsync(transactionIds, 100, 100, It.IsAny<CancellationToken>()), Times.Once);
        transactionRepoMock.Verify(r => r.LoadBatchAsync(transactionIds, 200, 100, It.IsAny<CancellationToken>()), Times.Once);

        // Verify UpdateAsync was called 3 times (once per batch)
        transactionRepoMock.Verify(r => r.UpdateAsync(It.IsAny<IEnumerable<Transaction>>()), Times.Exactly(3));

        // Verify balance progression is correct
        // First transaction: 1000 + 10 = 1010
        transactions[0].Balance.Should().Be(1010m);

        // 100th transaction: 1000 + (10 * 100) = 2000
        transactions[99].Balance.Should().Be(2000m);

        // Last transaction: 1000 + (10 * 250) = 3500
        transactions[249].Balance.Should().Be(3500m);
    }

    /// <summary>
    ///     T013b: Verifies correct batch size (exactly 100 per batch except last) (EN)<br/>
    ///     T013b: Xác minh kích thước batch đúng (chính xác 100 mỗi batch trừ batch cuối) (VI)
    /// </summary>
    [Fact]
    public async Task ForceSyncAsync_ProcessesExactly100PerBatch_ExceptLastBatch()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var accountFaker = CreateAccountFaker(userId);
        var account = accountFaker.Generate();
        account.InitialBalance = 0m;

        // Generate exactly 150 transactions
        var transactionFaker = CreateTransactionFaker(account.Id, userId);
        var transactions = Enumerable.Range(0, 150)
            .Select(i => transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => DateTime.UtcNow.AddDays(-150 + i))
                .RuleFor(t => t.RevenueAmount, _ => 1m)
                .Generate())
            .ToList();

        var transactionIds = transactions.Select(t => t.Id).ToList();

        var (accountRepoMock, transactionRepoMock, loggerMock) = CreateMocks();

        accountRepoMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
        transactionRepoMock.Setup(r => r.GetTransactionIdsAsync(account.Id, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionIds);

        // Batch 1: 100 transactions
        transactionRepoMock.Setup(r => r.LoadBatchAsync(transactionIds, 0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions.GetRange(0, 100));

        // Batch 2: 50 transactions (remaining)
        transactionRepoMock.Setup(r => r.LoadBatchAsync(transactionIds, 100, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions.GetRange(100, 50));

        transactionRepoMock.Setup(r => r.UpdateAsync(It.IsAny<IEnumerable<Transaction>>()))
            .ReturnsAsync((IEnumerable<Transaction> txns) => txns.Count());

        transactionRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(new List<Transaction>().AsQueryable());

        var service = new BalanceRecalculationService(
            accountRepoMock.Object,
            transactionRepoMock.Object,
            loggerMock.Object);

        // Act
        var result = await service.ForceSyncAsync(account.Id, CancellationToken.None);

        // Assert
        result.TransactionsRecalculated.Should().Be(150);

        // Verify exactly 2 batch calls
        transactionRepoMock.Verify(r => r.LoadBatchAsync(It.IsAny<List<Guid>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

        // Verify 2 update calls
        transactionRepoMock.Verify(r => r.UpdateAsync(It.IsAny<IEnumerable<Transaction>>()), Times.Exactly(2));
    }
}
