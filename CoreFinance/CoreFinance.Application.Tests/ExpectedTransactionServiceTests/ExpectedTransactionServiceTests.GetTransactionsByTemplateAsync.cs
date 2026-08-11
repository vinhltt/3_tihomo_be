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
///     Contains test cases for the GetTransactionsByTemplateAsync method of ExpectedTransactionService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức GetTransactionsByTemplateAsync của ExpectedTransactionService. (VI)
/// </summary>
public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     Verifies that GetTransactionsByTemplateAsync returns transactions for a specific template. (EN)<br />
    ///     Xác minh rằng GetTransactionsByTemplateAsync trả về các giao dịch cho một mẫu cụ thể. (VI)
    /// </summary>
    [Fact]
    public async Task GetTransactionsByTemplateAsync_ShouldReturnTransactionsForSpecificTemplate()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var otherTemplateId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = templateId,
                UserId = userId,
                Description = "Template Transaction 1",
                ExpectedAmount = 1000m,
                Status = ExpectedTransactionStatus.Pending,
                ExpectedDate = DateTime.UtcNow.AddDays(5)
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = templateId,
                UserId = userId,
                Description = "Template Transaction 2",
                ExpectedAmount = 1500m,
                Status = ExpectedTransactionStatus.Confirmed,
                ExpectedDate = DateTime.UtcNow.AddDays(10)
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = otherTemplateId, // Different template
                UserId = userId,
                Description = "Other Template Transaction",
                ExpectedAmount = 2000m,
                Status = ExpectedTransactionStatus.Pending,
                ExpectedDate = DateTime.UtcNow.AddDays(15)
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = templateId,
                UserId = userId,
                Description = "Template Transaction 3",
                ExpectedAmount = 2500m,
                Status = ExpectedTransactionStatus.Cancelled,
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
        var result = await service.GetTransactionsByTemplateAsync(templateId);

        // Assert
        var expectedTransactionViewModels = result.ToList();
        expectedTransactionViewModels.Should().NotBeNull();
        expectedTransactionViewModels.Should().HaveCount(3);
        expectedTransactionViewModels.Should().OnlyContain(t => t.RecurringTransactionTemplateId == templateId);
        expectedTransactionViewModels.Select(t => t.Description).Should().Contain([
            "Template Transaction 1", "Template Transaction 2", "Template Transaction 3"
        ]);

        // Should be ordered by ExpectedDate
        var resultList = expectedTransactionViewModels.ToList();
        resultList[0].Description.Should().Be("Template Transaction 1");
        resultList[1].Description.Should().Be("Template Transaction 2");
        resultList[2].Description.Should().Be("Template Transaction 3");
    }

    /// <summary>
    ///     Verifies that GetTransactionsByTemplateAsync returns an empty list when the template has no transactions. (EN)
    ///     <br />
    ///     Xác minh rằng GetTransactionsByTemplateAsync trả về danh sách rỗng khi mẫu không có giao dịch nào. (VI)
    /// </summary>
    [Fact]
    public async Task GetTransactionsByTemplateAsync_ShouldReturnEmptyList_WhenTemplateHasNoTransactions()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var otherTemplateId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = otherTemplateId, // Different template
                UserId = userId,
                Description = "Other Template Transaction",
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
        var result = await service.GetTransactionsByTemplateAsync(templateId);

        // Assert
        var expectedTransactionViewModels = result.ToList();
        expectedTransactionViewModels.Should().NotBeNull();
        expectedTransactionViewModels.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies that GetTransactionsByTemplateAsync returns an empty list when no transactions exist in the repository.
    ///     (EN)<br />
    ///     Xác minh rằng GetTransactionsByTemplateAsync trả về danh sách rỗng khi không có giao dịch nào tồn tại trong
    ///     repository. (VI)
    /// </summary>
    [Fact]
    public async Task GetTransactionsByTemplateAsync_ShouldReturnEmptyList_WhenNoTransactionsExist()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var expectedTransactionsMock = new List<ExpectedTransaction>().AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetTransactionsByTemplateAsync(templateId);

        // Assert
        var expectedTransactionViewModels = result.ToList();
        expectedTransactionViewModels.Should().NotBeNull();
        expectedTransactionViewModels.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies that GetTransactionsByTemplateAsync returns transactions ordered by ExpectedDate. (EN)<br />
    ///     Xác minh rằng GetTransactionsByTemplateAsync trả về các giao dịch được sắp xếp theo ExpectedDate. (VI)
    /// </summary>
    [Fact]
    public async Task GetTransactionsByTemplateAsync_ShouldReturnTransactionsOrderedByExpectedDate()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = templateId,
                UserId = userId,
                Description = "Transaction C",
                ExpectedAmount = 1000m,
                Status = ExpectedTransactionStatus.Pending,
                ExpectedDate = DateTime.UtcNow.AddDays(20)
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = templateId,
                UserId = userId,
                Description = "Transaction A",
                ExpectedAmount = 1500m,
                Status = ExpectedTransactionStatus.Pending,
                ExpectedDate = DateTime.UtcNow.AddDays(5)
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = templateId,
                UserId = userId,
                Description = "Transaction B",
                ExpectedAmount = 2000m,
                Status = ExpectedTransactionStatus.Pending,
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
        var result = await service.GetTransactionsByTemplateAsync(templateId);

        // Assert
        var resultList = result.ToList();
        resultList.Should().NotBeNull();
        resultList.Should().HaveCount(3);

        resultList[0].Description.Should().Be("Transaction A"); // Earliest date
        resultList[1].Description.Should().Be("Transaction B"); // Middle date
        resultList[2].Description.Should().Be("Transaction C"); // Latest date

        // Verify ordering
        for (var i = 0; i < resultList.Count - 1; i++)
            resultList[i].ExpectedDate.Should().BeOnOrBefore(resultList[i + 1].ExpectedDate);
    }

    /// <summary>
    ///     Verifies that GetTransactionsByTemplateAsync returns transactions of all status types. (EN)<br />
    ///     Xác minh rằng GetTransactionsByTemplateAsync trả về các giao dịch của tất cả các loại trạng thái. (VI)
    /// </summary>
    [Fact]
    public async Task GetTransactionsByTemplateAsync_ShouldReturnAllStatusTypes()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = templateId,
                UserId = userId,
                Description = "Pending Transaction",
                ExpectedAmount = 1000m,
                Status = ExpectedTransactionStatus.Pending,
                ExpectedDate = DateTime.UtcNow.AddDays(5)
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = templateId,
                UserId = userId,
                Description = "Confirmed Transaction",
                ExpectedAmount = 1500m,
                Status = ExpectedTransactionStatus.Confirmed,
                ExpectedDate = DateTime.UtcNow.AddDays(10)
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = templateId,
                UserId = userId,
                Description = "Cancelled Transaction",
                ExpectedAmount = 2000m,
                Status = ExpectedTransactionStatus.Cancelled,
                ExpectedDate = DateTime.UtcNow.AddDays(15)
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
        var result = await service.GetTransactionsByTemplateAsync(templateId);

        // Assert
        var expectedTransactionViewModels = result.ToList();
        expectedTransactionViewModels.Should().NotBeNull();
        expectedTransactionViewModels.Should().HaveCount(3);
        expectedTransactionViewModels.Should().Contain(t => t.Status == ExpectedTransactionStatus.Pending);
        expectedTransactionViewModels.Should().Contain(t => t.Status == ExpectedTransactionStatus.Confirmed);
        expectedTransactionViewModels.Should().Contain(t => t.Status == ExpectedTransactionStatus.Cancelled);
    }

    /// <summary>
    ///     Verifies that GetTransactionsByTemplateAsync returns the correct transaction properties in the ViewModel. (EN)
    ///     <br />
    ///     Xác minh rằng GetTransactionsByTemplateAsync trả về đúng các thuộc tính giao dịch trong ViewModel. (VI)
    /// </summary>
    [Fact]
    public async Task GetTransactionsByTemplateAsync_ShouldReturnCorrectTransactionProperties()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = templateId,
                UserId = userId,
                AccountId = accountId,
                Description = "Monthly Salary",
                ExpectedAmount = 5000m,
                TransactionType = RecurringTransactionType.Income,
                Category = "Salary",
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
        var result = await service.GetTransactionsByTemplateAsync(templateId);

        // Assert
        var expectedTransactionViewModels = result.ToList();
        expectedTransactionViewModels.Should().NotBeNull();
        expectedTransactionViewModels.Should().HaveCount(1);
        var transaction = expectedTransactionViewModels.First();

        transaction.RecurringTransactionTemplateId.Should().Be(templateId);
        transaction.UserId.Should().Be(userId);
        transaction.AccountId.Should().Be(accountId);
        transaction.Description.Should().Be("Monthly Salary");
        transaction.ExpectedAmount.Should().Be(5000m);
        transaction.TransactionType.Should().Be(RecurringTransactionType.Income);
        transaction.Category.Should().Be("Salary");
        transaction.Status.Should().Be(ExpectedTransactionStatus.Pending);
    }

    /// <summary>
    ///     Verifies that GetTransactionsByTemplateAsync handles past and future transactions correctly, returning them all.
    ///     (EN)<br />
    ///     Xác minh rằng GetTransactionsByTemplateAsync xử lý đúng các giao dịch quá khứ và tương lai, trả về tất cả chúng.
    ///     (VI)
    /// </summary>
    [Fact]
    public async Task GetTransactionsByTemplateAsync_ShouldHandlePastAndFutureTransactions()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var today = DateTime.UtcNow.Date;

        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = templateId,
                UserId = userId,
                Description = "Past Transaction",
                ExpectedAmount = 1000m,
                Status = ExpectedTransactionStatus.Confirmed,
                ExpectedDate = today.AddDays(-10)
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = templateId,
                UserId = userId,
                Description = "Today Transaction",
                ExpectedAmount = 1500m,
                Status = ExpectedTransactionStatus.Pending,
                ExpectedDate = today
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                RecurringTransactionTemplateId = templateId,
                UserId = userId,
                Description = "Future Transaction",
                ExpectedAmount = 2000m,
                Status = ExpectedTransactionStatus.Pending,
                ExpectedDate = today.AddDays(10)
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
        var result = await service.GetTransactionsByTemplateAsync(templateId);

        // Assert
        var expectedTransactionViewModels = result.ToList();
        expectedTransactionViewModels.Should().NotBeNull();
        expectedTransactionViewModels.Should().HaveCount(3);

        expectedTransactionViewModels[0].Description.Should().Be("Past Transaction"); // Earliest date
        expectedTransactionViewModels[1].Description.Should().Be("Today Transaction"); // Middle date
        expectedTransactionViewModels[2].Description.Should().Be("Future Transaction"); // Latest date
    }
}