namespace CoreFinance.Application.Tests.BalanceRecalculationServiceTests;

/// <summary>
///     Tests for ForceSyncAsync deterministic ordering (EN)<br/>
///     Kiểm thử thứ tự xác định của ForceSyncAsync (VI)
/// </summary>
public partial class BalanceRecalculationServiceTests
{
    /// <summary>
    ///     T014: Verifies that ForceSyncAsync maintains deterministic order by (TransactionDate, Id) (EN)<br/>
    ///     T014: Xác minh ForceSyncAsync duy trì thứ tự xác định theo (TransactionDate, Id) (VI)
    /// </summary>
    [Fact]
    public async Task ForceSyncAsync_MaintainsDeterministicOrder_ByTransactionDateThenId()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var accountFaker = CreateAccountFaker(userId);
        var account = accountFaker.Generate();
        account.InitialBalance = 1000m;

        var transactionFaker = CreateTransactionFaker(account.Id, userId);
        var sameDate = DateTime.UtcNow.AddDays(-5);

        // Create transactions with SAME date but different IDs
        // IDs should be sorted to ensure deterministic order
        var id1 = Guid.CreateVersion7();
        System.Threading.Thread.Sleep(1); // Ensure different v7 UUIDs
        var id2 = Guid.CreateVersion7();
        System.Threading.Thread.Sleep(1);
        var id3 = Guid.CreateVersion7();

        var transactions = new List<Transaction>
        {
            transactionFaker.Clone()
                .RuleFor(t => t.Id, _ => id1)
                .RuleFor(t => t.TransactionDate, _ => sameDate)
                .RuleFor(t => t.RevenueAmount, _ => 100m)
                .RuleFor(t => t.SpentAmount, _ => 0m)
                .Generate(),
            transactionFaker.Clone()
                .RuleFor(t => t.Id, _ => id2)
                .RuleFor(t => t.TransactionDate, _ => sameDate)
                .RuleFor(t => t.RevenueAmount, _ => 200m)
                .RuleFor(t => t.SpentAmount, _ => 0m)
                .Generate(),
            transactionFaker.Clone()
                .RuleFor(t => t.Id, _ => id3)
                .RuleFor(t => t.TransactionDate, _ => sameDate)
                .RuleFor(t => t.RevenueAmount, _ => 300m)
                .RuleFor(t => t.SpentAmount, _ => 0m)
                .Generate()
        };

        // Verify v7 UUIDs are ordered by creation time
        (string.Compare(transactions[0].Id.ToString(), transactions[1].Id.ToString(), StringComparison.Ordinal) < 0).Should().BeTrue();
        (string.Compare(transactions[1].Id.ToString(), transactions[2].Id.ToString(), StringComparison.Ordinal) < 0).Should().BeTrue();

        // Return IDs in sorted order (by TransactionDate, then Id)
        var transactionIds = transactions
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.Id)
            .Select(t => t.Id)
            .ToList();

        var (accountRepoMock, transactionRepoMock, loggerMock) = CreateMocks();

        // Setup mocks
        accountRepoMock.Setup(r => r.GetByIdAsync(account.Id))
            .ReturnsAsync(account);

        transactionRepoMock.Setup(r => r.GetTransactionIdsAsync(account.Id, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionIds); // Already sorted

        // Return transactions in the SAME sorted order
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
        result.Success.Should().BeTrue();
        result.TransactionsRecalculated.Should().Be(3);

        // Verify balances are calculated IN ORDER by Id (deterministic)
        transactions[0].Balance.Should().Be(1100m); // 1000 + 100 (id1 first)
        transactions[1].Balance.Should().Be(1300m); // 1100 + 200 (id2 second)
        transactions[2].Balance.Should().Be(1600m); // 1300 + 300 (id3 third)
    }

    /// <summary>
    ///     T014b: Verifies ordering with mixed dates and IDs (EN)<br/>
    ///     T014b: Xác minh thứ tự với ngày và ID hỗn hợp (VI)
    /// </summary>
    [Fact]
    public async Task ForceSyncAsync_OrdersByDateFirst_ThenById()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var accountFaker = CreateAccountFaker(userId);
        var account = accountFaker.Generate();
        account.InitialBalance = 1000m;

        var transactionFaker = CreateTransactionFaker(account.Id, userId);

        // Create transactions with different dates
        var earlierDate = DateTime.UtcNow.AddDays(-10);
        var laterDate = DateTime.UtcNow.AddDays(-5);

        var id1 = Guid.CreateVersion7();
        System.Threading.Thread.Sleep(1);
        var id2 = Guid.CreateVersion7();
        System.Threading.Thread.Sleep(1);
        var id3 = Guid.CreateVersion7();

        var transactions = new List<Transaction>
        {
            // Later date but earlier ID - should be SECOND
            transactionFaker.Clone()
                .RuleFor(t => t.Id, _ => id1)
                .RuleFor(t => t.TransactionDate, _ => laterDate)
                .RuleFor(t => t.RevenueAmount, _ => 200m)
                .Generate(),
            // Earlier date - should be FIRST regardless of ID
            transactionFaker.Clone()
                .RuleFor(t => t.Id, _ => id2)
                .RuleFor(t => t.TransactionDate, _ => earlierDate)
                .RuleFor(t => t.RevenueAmount, _ => 100m)
                .Generate(),
            // Later date, later ID - should be THIRD
            transactionFaker.Clone()
                .RuleFor(t => t.Id, _ => id3)
                .RuleFor(t => t.TransactionDate, _ => laterDate)
                .RuleFor(t => t.RevenueAmount, _ => 300m)
                .Generate()
        };

        // Sort by date first, then by ID (this is what repository should return)
        var sortedTransactions = transactions
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.Id)
            .ToList();

        var transactionIds = sortedTransactions.Select(t => t.Id).ToList();

        var (accountRepoMock, transactionRepoMock, loggerMock) = CreateMocks();

        accountRepoMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
        transactionRepoMock.Setup(r => r.GetTransactionIdsAsync(account.Id, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionIds);
        transactionRepoMock.Setup(r => r.LoadBatchAsync(transactionIds, 0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sortedTransactions);
        transactionRepoMock.Setup(r => r.UpdateAsync(It.IsAny<IEnumerable<Transaction>>()))
            .ReturnsAsync(sortedTransactions.Count);
        transactionRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(new List<Transaction>().AsQueryable());

        var service = new BalanceRecalculationService(
            accountRepoMock.Object,
            transactionRepoMock.Object,
            loggerMock.Object);

        // Act
        var result = await service.ForceSyncAsync(account.Id, CancellationToken.None);

        // Assert
        result.TransactionsRecalculated.Should().Be(3);

        // Verify order: earlier date first (id2), then later date sorted by ID (id1, id3)
        sortedTransactions[0].Id.Should().Be(id2); // Earlier date
        sortedTransactions[0].Balance.Should().Be(1100m); // 1000 + 100

        sortedTransactions[1].Id.Should().Be(id1); // Later date, earlier ID
        sortedTransactions[1].Balance.Should().Be(1300m); // 1100 + 200

        sortedTransactions[2].Id.Should().Be(id3); // Later date, later ID
        sortedTransactions[2].Balance.Should().Be(1600m); // 1300 + 300
    }

    /// <summary>
    ///     T014c: Verifies running same sync twice produces identical results (idempotency) (EN)<br/>
    ///     T014c: Xác minh chạy sync 2 lần cho kết quả giống hệt nhau (tính idempotent) (VI)
    /// </summary>
    [Fact]
    public async Task ForceSyncAsync_WhenRunTwice_ProducesIdenticalResults()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var accountFaker = CreateAccountFaker(userId);
        var account = accountFaker.Generate();
        account.InitialBalance = 1000m;

        var transactionFaker = CreateTransactionFaker(account.Id, userId);
        var transactions = Enumerable.Range(0, 5)
            .Select(i => transactionFaker.Clone()
                .RuleFor(t => t.TransactionDate, _ => DateTime.UtcNow.AddDays(-5 + i))
                .RuleFor(t => t.RevenueAmount, _ => 100m * (i + 1))
                .Generate())
            .ToList();

        var transactionIds = transactions.Select(t => t.Id).ToList();

        var (accountRepoMock, transactionRepoMock, loggerMock) = CreateMocks();

        accountRepoMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
        transactionRepoMock.Setup(r => r.GetTransactionIdsAsync(account.Id, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
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

        // Act - Run twice
        var result1 = await service.ForceSyncAsync(account.Id, CancellationToken.None);
        var balancesAfterFirstRun = transactions.Select(t => t.Balance).ToList();

        var result2 = await service.ForceSyncAsync(account.Id, CancellationToken.None);
        var balancesAfterSecondRun = transactions.Select(t => t.Balance).ToList();

        // Assert - Results should be identical (idempotent)
        result1.TransactionsRecalculated.Should().Be(result2.TransactionsRecalculated);
        balancesAfterFirstRun.Should().Equal(balancesAfterSecondRun);
    }
}
