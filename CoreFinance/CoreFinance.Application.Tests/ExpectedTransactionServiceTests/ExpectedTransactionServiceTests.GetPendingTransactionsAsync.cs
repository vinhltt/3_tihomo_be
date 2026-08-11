using CoreFinance.Application.DTOs.ExpectedTransaction;
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
///     Contains test cases for the GetPendingTransactionsAsync method of ExpectedTransactionService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức GetPendingTransactionsAsync của ExpectedTransactionService. (VI)
/// </summary>
public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     Verifies that GetPendingTransactionsAsync returns only pending transactions for a specific user. (EN)<br />
    ///     Xác minh rằng GetPendingTransactionsAsync chỉ trả về các giao dịch đang chờ xử lý cho một người dùng cụ thể. (VI)
    /// </summary>
    [Fact]
    public async Task GetPendingTransactionsAsync_ShouldReturnOnlyPendingTransactions_ForSpecificUser()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Description = "Pending Transaction 1",
                ExpectedAmount = 1000m,
                Status = ExpectedTransactionStatus.Pending,
                ExpectedDate = DateTime.UtcNow.AddDays(5)
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Description = "Confirmed Transaction",
                ExpectedAmount = 2000m,
                Status = ExpectedTransactionStatus.Confirmed, // Should be ignored
                ExpectedDate = DateTime.UtcNow.AddDays(10)
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Description = "Pending Transaction 2",
                ExpectedAmount = 1500m,
                Status = ExpectedTransactionStatus.Pending,
                ExpectedDate = DateTime.UtcNow.AddDays(15)
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = otherUserId,
                Description = "Other User Pending",
                ExpectedAmount = 3000m,
                Status = ExpectedTransactionStatus.Pending, // Should be ignored (different user)
                ExpectedDate = DateTime.UtcNow.AddDays(20)
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
        var result = await service.GetPendingTransactionsAsync(userId);

        // Assert
        var expectedTransactionViewModels = result.ToList();
        expectedTransactionViewModels.Should().NotBeNull();
        expectedTransactionViewModels.Should().HaveCount(2);
        expectedTransactionViewModels.Should().OnlyContain(t => t.UserId == userId);
        expectedTransactionViewModels.Should().OnlyContain(t => t.Status == ExpectedTransactionStatus.Pending);
        expectedTransactionViewModels.Select(t => t.Description).Should().Contain(new List<string>
            { "Pending Transaction 1", "Pending Transaction 2" }.AsReadOnly());
    }

    /// <summary>
    ///     Verifies that GetPendingTransactionsAsync returns an empty list when no pending transactions exist for the user.
    ///     (EN)<br />
    ///     Xác minh rằng GetPendingTransactionsAsync trả về danh sách rỗng khi không có giao dịch đang chờ xử lý nào tồn tại
    ///     cho người dùng. (VI)
    /// </summary>
    [Fact]
    public async Task GetPendingTransactionsAsync_ShouldReturnEmptyList_WhenNoPendingTransactionsExist()
    {
        // Arrange
        var userId = Guid.CreateVersion7();

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Description = "Confirmed Transaction",
                ExpectedAmount = 1000m,
                Status = ExpectedTransactionStatus.Confirmed,
                ExpectedDate = DateTime.UtcNow.AddDays(5)
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Description = "Cancelled Transaction",
                ExpectedAmount = 2000m,
                Status = ExpectedTransactionStatus.Cancelled,
                ExpectedDate = DateTime.UtcNow.AddDays(10)
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
        var result = await service.GetPendingTransactionsAsync(userId);

        // Assert
        var expectedTransactionViewModels = result.ToList();
        expectedTransactionViewModels.Should().NotBeNull();
        expectedTransactionViewModels.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies that GetPendingTransactionsAsync returns an empty list when the user has no transactions at all. (EN)
    ///     <br />
    ///     Xác minh rằng GetPendingTransactionsAsync trả về danh sách rỗng khi người dùng không có bất kỳ giao dịch nào. (VI)
    /// </summary>
    [Fact]
    public async Task GetPendingTransactionsAsync_ShouldReturnEmptyList_WhenUserHasNoTransactions()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = otherUserId,
                Description = "Other User Transaction",
                ExpectedAmount = 1000m,
                Status = ExpectedTransactionStatus.Pending,
                ExpectedDate = DateTime.UtcNow.AddDays(5)
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
        var result = await service.GetPendingTransactionsAsync(userId);

        // Assert
        var expectedTransactionViewModels = result as ExpectedTransactionViewModel[] ?? result.ToArray();
        expectedTransactionViewModels.Should().NotBeNull();
        expectedTransactionViewModels.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies that GetPendingTransactionsAsync returns an empty list when no transactions exist in the repository
    ///     globally. (EN)<br />
    ///     Xác minh rằng GetPendingTransactionsAsync trả về danh sách rỗng khi không có giao dịch nào tồn tại trong toàn bộ
    ///     repository. (VI)
    /// </summary>
    [Fact]
    public async Task GetPendingTransactionsAsync_ShouldReturnEmptyList_WhenNoTransactionsExist()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var expectedTransactionsMock = new List<ExpectedTransaction>().AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetPendingTransactionsAsync(userId);

        // Assert
        var expectedTransactionViewModels = result as ExpectedTransactionViewModel[] ?? result.ToArray();
        expectedTransactionViewModels.Should().NotBeNull();
        expectedTransactionViewModels.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies that GetPendingTransactionsAsync returns the correct transaction properties in the ViewModel. (EN)<br />
    ///     Xác minh rằng GetPendingTransactionsAsync trả về đúng các thuộc tính giao dịch trong ViewModel. (VI)
    /// </summary>
    [Fact]
    public async Task GetPendingTransactionsAsync_ShouldReturnCorrectTransactionProperties()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var templateId = Guid.CreateVersion7();

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                AccountId = accountId,
                RecurringTransactionTemplateId = templateId,
                Description = "Monthly Rent",
                ExpectedAmount = 1500m,
                TransactionType = RecurringTransactionType.Expense,
                Category = "Housing",
                Status = ExpectedTransactionStatus.Pending,
                ExpectedDate = DateTime.UtcNow.AddDays(5),
                GeneratedAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
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
        var result = await service.GetPendingTransactionsAsync(userId);

        // Assert
        var expectedTransactionViewModels = result as ExpectedTransactionViewModel[] ?? result.ToArray();
        expectedTransactionViewModels.Should().NotBeNull();
        expectedTransactionViewModels.Should().HaveCount(1);

        var transaction = expectedTransactionViewModels.First();
        transaction.UserId.Should().Be(userId);
        transaction.AccountId.Should().Be(accountId);
        transaction.RecurringTransactionTemplateId.Should().Be(templateId);
        transaction.Description.Should().Be("Monthly Rent");
        transaction.ExpectedAmount.Should().Be(1500m);
        transaction.TransactionType.Should().Be(RecurringTransactionType.Expense);
        transaction.Category.Should().Be("Housing");
        transaction.Status.Should().Be(ExpectedTransactionStatus.Pending);
    }
}