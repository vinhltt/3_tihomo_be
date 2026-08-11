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

// Tests for the GetCategoryForecastAsync method of ExpectedTransactionService
public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Verifies that GetCategoryForecastAsync returns the correct category breakdown with aggregated amounts.<br />
    ///     (VI) Xác minh rằng GetCategoryForecastAsync trả về phân tích danh mục chính xác với tổng số tiền.
    /// </summary>
    [Fact]
    public async Task GetCategoryForecastAsync_ShouldReturnCorrectCategoryBreakdown()
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
                Category = "Salary",
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(10),
                ExpectedAmount = 2000m,
                TransactionType = RecurringTransactionType.Expense,
                Category = "Food",
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(15),
                ExpectedAmount = 1500m,
                TransactionType = RecurringTransactionType.Expense,
                Category = "Transport",
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(20),
                ExpectedAmount = 800m,
                TransactionType = RecurringTransactionType.Expense,
                Category = "Food", // Same category as another expense
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
        var result = await service.GetCategoryForecastAsync(userId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result["Salary"].Should().Be(5000m); // Income is positive
        result["Food"].Should().Be(-2800m); // Expenses are negative (2000 + 800)
        result["Transport"].Should().Be(-1500m); // Expense is negative
    }

    /// <summary>
    ///     (EN) Verifies that GetCategoryForecastAsync ignores transactions with null or empty category.<br />
    ///     (VI) Xác minh rằng GetCategoryForecastAsync bỏ qua các giao dịch có danh mục null hoặc rỗng.
    /// </summary>
    [Fact]
    public async Task GetCategoryForecastAsync_ShouldIgnoreTransactionsWithNullOrEmptyCategory()
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
                Category = "Salary",
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(10),
                ExpectedAmount = 2000m,
                TransactionType = RecurringTransactionType.Expense,
                Category = null, // Should be ignored
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(15),
                ExpectedAmount = 1500m,
                TransactionType = RecurringTransactionType.Expense,
                Category = "", // Should be ignored
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(20),
                ExpectedAmount = 800m,
                TransactionType = RecurringTransactionType.Expense,
                Category = "Food",
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
        var result = await service.GetCategoryForecastAsync(userId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Only Salary and Food categories
        result["Salary"].Should().Be(5000m);
        result["Food"].Should().Be(-800m);
    }

    /// <summary>
    ///     (EN) Verifies that GetCategoryForecastAsync ignores non-pending transactions.<br />
    ///     (VI) Xác minh rằng GetCategoryForecastAsync bỏ qua các giao dịch không ở trạng thái chờ xử lý.
    /// </summary>
    [Fact]
    public async Task GetCategoryForecastAsync_ShouldIgnoreNonPendingTransactions()
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
                Category = "Salary",
                Status = ExpectedTransactionStatus.Pending // Should be included
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(10),
                ExpectedAmount = 2000m,
                TransactionType = RecurringTransactionType.Expense,
                Category = "Food",
                Status = ExpectedTransactionStatus.Confirmed // Should be ignored
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(15),
                ExpectedAmount = 1500m,
                TransactionType = RecurringTransactionType.Expense,
                Category = "Transport",
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

        // Act
        var result = await service.GetCategoryForecastAsync(userId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1); // Only Salary category (pending)
        result["Salary"].Should().Be(5000m);
    }

    /// <summary>
    ///     (EN) Verifies that GetCategoryForecastAsync filters transactions by User ID.<br />
    ///     (VI) Xác minh rằng GetCategoryForecastAsync lọc các giao dịch theo ID người dùng.
    /// </summary>
    [Fact]
    public async Task GetCategoryForecastAsync_ShouldFilterByUserId()
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
                Category = "Salary",
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = otherUserId, // Different user
                ExpectedDate = startDate.AddDays(10),
                ExpectedAmount = 10000m,
                TransactionType = RecurringTransactionType.Income,
                Category = "Salary",
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
        var result = await service.GetCategoryForecastAsync(userId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result["Salary"].Should().Be(5000m); // Only the transaction for the specified user
    }

    /// <summary>
    ///     Verifies that GetCategoryForecastAsync returns an empty dictionary when there are no valid transactions (within
    ///     date range, with category, pending). (EN)<br />
    ///     Xác minh rằng GetCategoryForecastAsync trả về một dictionary rỗng khi không có giao dịch hợp lệ nào (trong phạm vi
    ///     ngày, có danh mục, đang chờ xử lý). (VI)
    /// </summary>
    [Fact]
    public async Task GetCategoryForecastAsync_ShouldReturnEmptyDictionary_WhenNoValidTransactions()
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
                Category = "Salary",
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(5),
                ExpectedAmount = 2000m,
                TransactionType = RecurringTransactionType.Expense,
                Category = null, // No category
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
        var result = await service.GetCategoryForecastAsync(userId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies that GetCategoryForecastAsync correctly handles mixed income and expense transactions within the same
    ///     category. (EN)<br />
    ///     Xác minh rằng GetCategoryForecastAsync xử lý đúng các giao dịch thu nhập và chi phí hỗn hợp trong cùng một danh
    ///     mục. (VI)
    /// </summary>
    [Fact]
    public async Task GetCategoryForecastAsync_ShouldHandleMixedIncomeAndExpenseInSameCategory()
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
                Category = "Business",
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(10),
                ExpectedAmount = 1000m,
                TransactionType = RecurringTransactionType.Expense,
                Category = "Business", // Same category as income
                Status = ExpectedTransactionStatus.Pending
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                ExpectedDate = startDate.AddDays(15),
                ExpectedAmount = 500m,
                TransactionType = RecurringTransactionType.Income,
                Category = "Business", // Another income in same category
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
        var result = await service.GetCategoryForecastAsync(userId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result["Business"].Should().Be(2500m); // 3000 + 500 - 1000 = 2500
    }
}