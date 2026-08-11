using CoreFinance.Application.Services;
using CoreFinance.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CoreFinance.Application.Tests.BalanceRecalculationServiceTests;

/// <summary>
///     Tests for ForceSyncAsync with orphaned transactions (EN)<br/>
///     Kiểm thử ForceSyncAsync với giao dịch mồ côi (VI)
/// </summary>
public partial class BalanceRecalculationServiceTests
{
    /// <summary>
    ///     T011: Verifies that ForceSyncAsync throws exception when encountering orphaned transactions (EN)<br/>
    ///     T011: Xác minh ForceSyncAsync ném exception khi gặp giao dịch mồ côi (VI)
    /// </summary>
    [Fact]
    public async Task ForceSyncAsync_WithOrphanedTransactions_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var accountFaker = CreateAccountFaker(userId);
        var account = accountFaker.Generate();
        account.LastSync = null;
        account.InitialBalance = 1000m;

        var transactionFaker = CreateTransactionFaker(account.Id, userId);

        // Create transactions with one having NULL AccountId (orphaned)
        var transactions = new List<Transaction>
        {
            transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => DateTime.UtcNow.AddDays(-3))
                .RuleFor(t => t.RevenueAmount, _ => 500m)
                .RuleFor(t => t.SpentAmount, _ => 0m)
                .Generate(),
            transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => DateTime.UtcNow.AddDays(-2))
                .RuleFor(t => t.AccountId, _ => null) // ORPHANED TRANSACTION
                .RuleFor(t => t.RevenueAmount, _ => 0m)
                .RuleFor(t => t.SpentAmount, _ => 200m)
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

        transactionRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(new List<Transaction>().AsQueryable());

        var service = new BalanceRecalculationService(
            accountRepoMock.Object,
            transactionRepoMock.Object,
            loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.ForceSyncAsync(account.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot force sync: Found transactions with null AccountId");

        // Verify UpdateAsync was NOT called (because exception was thrown before update)
        transactionRepoMock.Verify(r => r.UpdateAsync(It.IsAny<IEnumerable<Transaction>>()), Times.Never);
    }

    /// <summary>
    ///     T011b: Verifies that no transactions are updated when orphaned transaction is found (EN)<br/>
    ///     T011b: Xác minh không có giao dịch nào được cập nhật khi tìm thấy giao dịch mồ côi (VI)
    /// </summary>
    [Fact]
    public async Task ForceSyncAsync_WithOrphanedTransactions_DoesNotUpdateAnyTransactions()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var accountFaker = CreateAccountFaker(userId);
        var account = accountFaker.Generate();

        var transactionFaker = CreateTransactionFaker(account.Id, userId);
        var validTransaction = transactionFaker.Clone()
            .RuleFor(t => t.TransactionDate, _ => DateTime.UtcNow.AddDays(-3))
            .RuleFor(t => t.RevenueAmount, _ => 500m)
            .Generate();

        var orphanedTransaction = transactionFaker.Clone()
            .RuleFor(t => t.TransactionDate, _ => DateTime.UtcNow.AddDays(-2))
            .RuleFor(t => t.AccountId, _ => null) // Orphaned
            .Generate();

        var transactions = new List<Transaction> { validTransaction, orphanedTransaction };
        var transactionIds = transactions.Select(t => t.Id).ToList();

        var (accountRepoMock, transactionRepoMock, loggerMock) = CreateMocks();

        accountRepoMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
        transactionRepoMock.Setup(r => r.GetTransactionIdsAsync(account.Id, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionIds);
        transactionRepoMock.Setup(r => r.LoadBatchAsync(transactionIds, 0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);
        transactionRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(new List<Transaction>().AsQueryable());

        var service = new BalanceRecalculationService(
            accountRepoMock.Object,
            transactionRepoMock.Object,
            loggerMock.Object);

        // Store original balance
        var originalBalance = validTransaction.Balance;

        // Act
        try
        {
            await service.ForceSyncAsync(account.Id, CancellationToken.None);
        }
        catch (InvalidOperationException)
        {
            // Expected exception
        }

        // Assert - valid transaction should NOT have been updated
        validTransaction.Balance.Should().Be(originalBalance);
        transactionRepoMock.Verify(r => r.UpdateAsync(It.IsAny<IEnumerable<Transaction>>()), Times.Never);
    }
}
