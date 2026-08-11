using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

/// <summary>
///     Contains test cases for the GetCashFlowForecastAsync method of ExpectedTransactionService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức GetCashFlowForecastAsync của ExpectedTransactionService. (VI)
/// </summary>
// Tests for the GetCashFlowForecastAsync method of ExpectedTransactionService
public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     Verifies that GetCashFlowForecastAsync returns a positive cash flow when income exceeds expenses within the date
    ///     range. (EN)<br />
    ///     Xác minh rằng GetCashFlowForecastAsync trả về dòng tiền dương khi thu nhập vượt quá chi phí trong phạm vi ngày.
    ///     (VI)
    /// </summary>
    [Fact]
    public async Task GetCashFlowForecastAsync_ShouldReturnPositiveCashFlow_WhenIncomeExceedsExpenses()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(30);

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(5),
                ExpectedAmount = 5000m,
                TransactionType = RecurringTransactionType.Income,
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(10),
                ExpectedAmount = 2000m,
                TransactionType = RecurringTransactionType.Expense,
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(15),
                ExpectedAmount = 1500m,
                TransactionType = RecurringTransactionType.Expense,
                Status = ExpectedTransactionStatus.Pending
            }
        };

        var expectedTransactionsMock = expectedTransactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var expectedCashFlow = 5000m - 2000m - 1500m; // Income - Expenses = 1500

        // Act
        var result = await service.GetCashFlowForecastAsync(userId, startDate, endDate);

        // Assert
        result.Should().Be(expectedCashFlow);
    }

    /// <summary>
    ///     Verifies that GetCashFlowForecastAsync returns a negative cash flow when expenses exceed income within the date
    ///     range. (EN)<br />
    ///     Xác minh rằng GetCashFlowForecastAsync trả về dòng tiền âm khi chi phí vượt quá thu nhập trong phạm vi ngày. (VI)
    /// </summary>
    [Fact]
    public async Task GetCashFlowForecastAsync_ShouldReturnNegativeCashFlow_WhenExpensesExceedIncome()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(30);

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(5),
                ExpectedAmount = 2000m,
                TransactionType = RecurringTransactionType.Income,
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(10),
                ExpectedAmount = 3000m,
                TransactionType = RecurringTransactionType.Expense,
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(15),
                ExpectedAmount = 1500m,
                TransactionType = RecurringTransactionType.Expense,
                Status = ExpectedTransactionStatus.Pending
            }
        };

        var expectedTransactionsMock = expectedTransactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var expectedCashFlow = 2000m - 3000m - 1500m; // Income - Expenses = -2500

        // Act
        var result = await service.GetCashFlowForecastAsync(userId, startDate, endDate);

        // Assert
        result.Should().Be(expectedCashFlow);
    }

    /// <summary>
    ///     Verifies that GetCashFlowForecastAsync returns zero when there are no transactions within the specified date range.
    ///     (EN)<br />
    ///     Xác minh rằng GetCashFlowForecastAsync trả về không khi không có giao dịch nào trong phạm vi ngày được chỉ định.
    ///     (VI)
    /// </summary>
    [Fact]
    public async Task GetCashFlowForecastAsync_ShouldReturnZero_WhenNoTransactionsInDateRange()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(30);

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(-10), // Outside date range
                ExpectedAmount = 5000m,
                TransactionType = RecurringTransactionType.Income,
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = endDate.AddDays(10), // Outside date range
                ExpectedAmount = 2000m,
                TransactionType = RecurringTransactionType.Expense,
                Status = ExpectedTransactionStatus.Pending
            }
        };

        var expectedTransactionsMock = expectedTransactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetCashFlowForecastAsync(userId, startDate, endDate);

        // Assert
        result.Should().Be(0m);
    }

    /// <summary>
    ///     Verifies that GetCashFlowForecastAsync ignores non-pending transactions when calculating cash flow. (EN)<br />
    ///     Xác minh rằng GetCashFlowForecastAsync bỏ qua các giao dịch không ở trạng thái đang chờ xử lý khi tính toán dòng
    ///     tiền. (VI)
    /// </summary>
    [Fact]
    public async Task GetCashFlowForecastAsync_ShouldIgnoreNonPendingTransactions()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(30);

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(5),
                ExpectedAmount = 5000m,
                TransactionType = RecurringTransactionType.Income,
                Status = ExpectedTransactionStatus.Pending // Should be included
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(10),
                ExpectedAmount = 2000m,
                TransactionType = RecurringTransactionType.Expense,
                Status = ExpectedTransactionStatus.Confirmed // Should be ignored
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(15),
                ExpectedAmount = 1500m,
                TransactionType = RecurringTransactionType.Expense,
                Status = ExpectedTransactionStatus.Cancelled // Should be ignored
            }
        };

        var expectedTransactionsMock = expectedTransactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var expectedCashFlow = 5000m; // Only pending income transaction

        // Act
        var result = await service.GetCashFlowForecastAsync(userId, startDate, endDate);

        // Assert
        result.Should().Be(expectedCashFlow);
    }

    /// <summary>
    ///     Verifies that GetCashFlowForecastAsync filters transactions by User ID when calculating cash flow. (EN)<br />
    ///     Xác minh rằng GetCashFlowForecastAsync lọc các giao dịch theo ID người dùng khi tính toán dòng tiền. (VI)
    /// </summary>
    [Fact]
    public async Task GetCashFlowForecastAsync_ShouldFilterByUserId()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(30);

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(5),
                ExpectedAmount = 5000m,
                TransactionType = RecurringTransactionType.Income,
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = otherUserId, // Different user
                ExpectedDate = startDate.AddDays(10),
                ExpectedAmount = 10000m,
                TransactionType = RecurringTransactionType.Income,
                Status = ExpectedTransactionStatus.Pending
            }
        };

        var expectedTransactionsMock = expectedTransactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var expectedCashFlow = 5000m; // Only the transaction for the specified user

        // Act
        var result = await service.GetCashFlowForecastAsync(userId, startDate, endDate);

        // Assert
        result.Should().Be(expectedCashFlow);
    }

    /// <summary>
    ///     Verifies that GetCashFlowForecastAsync returns the correct sum when only income transactions exist within the date
    ///     range. (EN)<br />
    ///     Xác minh rằng GetCashFlowForecastAsync trả về tổng đúng khi chỉ có các giao dịch thu nhập tồn tại trong phạm vi
    ///     ngày. (VI)
    /// </summary>
    [Fact]
    public async Task GetCashFlowForecastAsync_ShouldReturnZero_WhenOnlyIncomeTransactionsExist()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(30);

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(5),
                ExpectedAmount = 3000m,
                TransactionType = RecurringTransactionType.Income,
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(15),
                ExpectedAmount = 2000m,
                TransactionType = RecurringTransactionType.Income,
                Status = ExpectedTransactionStatus.Pending
            }
        };

        var expectedTransactionsMock = expectedTransactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var expectedCashFlow = 3000m + 2000m; // Total income = 5000

        // Act
        var result = await service.GetCashFlowForecastAsync(userId, startDate, endDate);

        // Assert
        result.Should().Be(expectedCashFlow);
    }
}