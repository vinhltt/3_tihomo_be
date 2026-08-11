using CoreFinance.Application.Services;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using MockQueryable;
using Moq;

namespace CoreFinance.Application.Tests.BalanceRecalculationServiceTests;

/// <summary>
///     Tests for ForceSyncAsync with credit card accounts (EN)<br/>
///     Kiểm thử ForceSyncAsync với tài khoản thẻ tín dụng (VI)
/// </summary>
public partial class BalanceRecalculationServiceTests
{
    /// <summary>
    ///     T012: Verifies that ForceSyncAsync calculates AvailableLimit for credit card accounts (EN)<br/>
    ///     T012: Xác minh ForceSyncAsync tính toán AvailableLimit cho tài khoản thẻ tín dụng (VI)
    /// </summary>
    [Fact]
    public async Task ForceSyncAsync_WithCreditCardAccount_CalculatesAvailableLimit()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var accountFaker = CreateAccountFaker(userId);
        var account = accountFaker.Generate();
        account.Type = AccountType.CreditCard;
        account.CreditLimit = 10000m;
        account.LastSync = null;
        account.InitialBalance = 0m; // Credit cards typically start at 0

        var transactionFaker = CreateTransactionFaker(account.Id, userId);
        var transactions = new List<Transaction>
        {
            // Spent 500 (reduces available limit)
            transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => DateTime.UtcNow.AddDays(-3))
                .RuleFor(t => t.RevenueAmount, _ => 0m)
                .RuleFor(t => t.SpentAmount, _ => 500m)
                .Generate(),
            // Spent 300 more
            transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => DateTime.UtcNow.AddDays(-2))
                .RuleFor(t => t.RevenueAmount, _ => 0m)
                .RuleFor(t => t.SpentAmount, _ => 300m)
                .Generate(),
            // Payment of 200 (increases available limit)
            transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => DateTime.UtcNow.AddDays(-1))
                .RuleFor(t => t.RevenueAmount, _ => 200m)
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
            .Returns(new List<Transaction>().AsQueryable().BuildMock());

        transactionRepoMock.Setup(r => r.GetNetSpentUpToAsync(account.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m); // No previous net spent

        var service = new BalanceRecalculationService(
            accountRepoMock.Object,
            transactionRepoMock.Object,
            loggerMock.Object);

        // Act
        var result = await service.ForceSyncAsync(account.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.TransactionsRecalculated.Should().Be(3);

        // Verify AvailableLimit calculations
        // Transaction 1: Spent 500 → AvailableLimit = 10000 - 500 = 9500
        transactions[0].AvailableLimit.Should().Be(9500m);

        // Transaction 2: Spent 300 more → AvailableLimit = 10000 - (500 + 300) = 9200
        transactions[1].AvailableLimit.Should().Be(9200m);

        // Transaction 3: Payment 200 → AvailableLimit = 10000 - (500 + 300 - 200) = 9400
        transactions[2].AvailableLimit.Should().Be(9400m);

        // Verify balance calculations (for credit cards, negative balance = owed amount)
        transactions[0].Balance.Should().Be(-500m);  // 0 - 500
        transactions[1].Balance.Should().Be(-800m);  // -500 - 300
        transactions[2].Balance.Should().Be(-600m);  // -800 + 200
    }

    /// <summary>
    ///     T012b: Verifies AvailableLimit calculation with existing LastSync (EN)<br/>
    ///     T012b: Xác minh tính toán AvailableLimit với LastSync đã tồn tại (VI)
    /// </summary>
    [Fact]
    public async Task ForceSyncAsync_WithCreditCardAndLastSync_CalculatesAvailableLimitFromCheckpoint()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var accountFaker = CreateAccountFaker(userId);
        var account = accountFaker.Generate();
        account.Type = AccountType.CreditCard;
        account.CreditLimit = 10000m;

        var checkpointDate = DateTime.UtcNow.AddDays(-5);
        account.LastSync = checkpointDate;

        var transactionFaker = CreateTransactionFaker(account.Id, userId);

        // New transactions after LastSync
        var newTransactions = new List<Transaction>
        {
            transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => checkpointDate.AddDays(1))
                .RuleFor(t => t.RevenueAmount, _ => 0m)
                .RuleFor(t => t.SpentAmount, _ => 100m)
                .Generate()
        };

        var newTransactionIds = newTransactions.Select(t => t.Id).ToList();

        var (accountRepoMock, transactionRepoMock, loggerMock) = CreateMocks();

        accountRepoMock.Setup(r => r.GetByIdAsync(account.Id))
            .ReturnsAsync(account);

        transactionRepoMock.Setup(r => r.GetTransactionIdsAsync(account.Id, checkpointDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newTransactionIds);

        transactionRepoMock.Setup(r => r.LoadBatchAsync(newTransactionIds, 0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newTransactions);

        transactionRepoMock.Setup(r => r.UpdateAsync(It.IsAny<IEnumerable<Transaction>>()))
            .ReturnsAsync(newTransactions.Count);

        transactionRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(new List<Transaction>().AsQueryable().BuildMock());

        // Baseline: Already spent 500 before LastSync
        transactionRepoMock.Setup(r => r.GetNetSpentUpToAsync(account.Id, checkpointDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(500m);

        var service = new BalanceRecalculationService(
            accountRepoMock.Object,
            transactionRepoMock.Object,
            loggerMock.Object);

        // Act
        var result = await service.ForceSyncAsync(account.Id, CancellationToken.None);

        // Assert
        result.TransactionsRecalculated.Should().Be(1);

        // AvailableLimit = CreditLimit - (baseline + new spent)
        // = 10000 - (500 + 100) = 9400
        newTransactions[0].AvailableLimit.Should().Be(9400m);

        // Verify GetNetSpentUpToAsync was called for baseline
        transactionRepoMock.Verify(r => r.GetNetSpentUpToAsync(account.Id, checkpointDate, It.IsAny<CancellationToken>()), Times.Once);
    }
}
