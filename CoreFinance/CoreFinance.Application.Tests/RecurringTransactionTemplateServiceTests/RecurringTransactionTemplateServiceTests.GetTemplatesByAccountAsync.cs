using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

// Tests for the GetTemplatesByAccountAsync method of RecurringTransactionTemplateService
public partial class RecurringTransactionTemplateServiceTests
{
    /// <summary>
    ///     Verifies that GetTemplatesByAccountAsync returns templates associated with the specified account ID.<br />
    ///     (EN) Verifies that GetTemplatesByAccountAsync returns templates associated with the specified account ID.<br />
    ///     (VI) Xác minh rằng GetTemplatesByAccountAsync trả về các mẫu liên quan đến ID tài khoản được chỉ định.
    /// </summary>
    [Fact]
    public async Task GetTemplatesByAccountAsync_ShouldReturnTemplatesForSpecificAccount()
    {
        // Arrange
        var accountId = Guid.CreateVersion7();
        var otherAccountId = Guid.CreateVersion7();

        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                AccountId = accountId,
                Name = "Account Template 1",
                IsActive = true
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                AccountId = accountId,
                Name = "Account Template 2",
                IsActive = false
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                AccountId = otherAccountId,
                Name = "Other Account Template",
                IsActive = true
            }
        };

        var templatesMock = templates.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetTemplatesByAccountAsync(accountId);

        // Assert
        var recurringTransactionTemplateViewModels = result.ToList();
        recurringTransactionTemplateViewModels.Should().NotBeNull();
        recurringTransactionTemplateViewModels.Should().HaveCount(2);
        recurringTransactionTemplateViewModels.Should().OnlyContain(t => t.AccountId == accountId);
        recurringTransactionTemplateViewModels.Select(t => t.Name).Should()
            .Contain(["Account Template 1", "Account Template 2"]);
    }

    /// <summary>
    ///     Verifies that GetTemplatesByAccountAsync returns an empty list when the account has no associated templates.<br />
    ///     (EN) Verifies that GetTemplatesByAccountAsync returns an empty list when the account has no associated templates.
    ///     <br />
    ///     (VI) Xác minh rằng GetTemplatesByAccountAsync trả về danh sách rỗng khi tài khoản không có mẫu liên quan.
    /// </summary>
    [Fact]
    public async Task GetTemplatesByAccountAsync_ShouldReturnEmptyList_WhenAccountHasNoTemplates()
    {
        // Arrange
        var accountId = Guid.CreateVersion7();
        var otherAccountId = Guid.CreateVersion7();

        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                AccountId = otherAccountId,
                Name = "Other Account Template",
                IsActive = true
            }
        };

        var templatesMock = templates.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetTemplatesByAccountAsync(accountId);

        // Assert
        var recurringTransactionTemplateViewModels = result.ToList();
        recurringTransactionTemplateViewModels.Should().NotBeNull();
        recurringTransactionTemplateViewModels.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies that GetTemplatesByAccountAsync returns an empty list when no templates exist in the repository.<br />
    ///     (EN) Verifies that GetTemplatesByAccountAsync returns an empty list when no templates exist in the repository.
    ///     <br />
    ///     (VI) Xác minh rằng GetTemplatesByAccountAsync trả về danh sách rỗng khi không có mẫu nào tồn tại trong repository.
    /// </summary>
    [Fact]
    public async Task GetTemplatesByAccountAsync_ShouldReturnEmptyList_WhenNoTemplatesExist()
    {
        // Arrange
        var accountId = Guid.CreateVersion7();
        var templatesMock = new List<RecurringTransactionTemplate>().AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetTemplatesByAccountAsync(accountId);

        // Assert
        var recurringTransactionTemplateViewModels = result.ToList();
        recurringTransactionTemplateViewModels.Should().NotBeNull();
        recurringTransactionTemplateViewModels.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies that GetTemplatesByAccountAsync returns both active and inactive templates for the specified account.
    ///     <br />
    ///     (EN) Verifies that GetTemplatesByAccountAsync returns both active and inactive templates for the specified account.
    ///     <br />
    ///     (VI) Xác minh rằng GetTemplatesByAccountAsync trả về cả mẫu đang hoạt động và không hoạt động cho tài khoản được
    ///     chỉ định.
    /// </summary>
    [Fact]
    public async Task GetTemplatesByAccountAsync_ShouldReturnBothActiveAndInactiveTemplates()
    {
        // Arrange
        var accountId = Guid.CreateVersion7();

        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                AccountId = accountId,
                Name = "Active Template",
                IsActive = true,
                Amount = 1000m,
                TransactionType = RecurringTransactionType.Income
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                AccountId = accountId,
                Name = "Inactive Template",
                IsActive = false,
                Amount = 500m,
                TransactionType = RecurringTransactionType.Expense
            }
        };

        var templatesMock = templates.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetTemplatesByAccountAsync(accountId);

        // Assert
        var recurringTransactionTemplateViewModels = result.ToList();
        recurringTransactionTemplateViewModels.Should().NotBeNull();
        recurringTransactionTemplateViewModels.Should().HaveCount(2);
        recurringTransactionTemplateViewModels.Should().OnlyContain(t => t.AccountId == accountId);

        var activeTemplate = recurringTransactionTemplateViewModels.First(t => t.IsActive);
        var inactiveTemplate = recurringTransactionTemplateViewModels.First(t => !t.IsActive);

        activeTemplate.Name.Should().Be("Active Template");
        activeTemplate.Amount.Should().Be(1000m);
        activeTemplate.TransactionType.Should().Be(RecurringTransactionType.Income);

        inactiveTemplate.Name.Should().Be("Inactive Template");
        inactiveTemplate.Amount.Should().Be(500m);
        inactiveTemplate.TransactionType.Should().Be(RecurringTransactionType.Expense);
    }

    /// <summary>
    ///     Verifies that GetTemplatesByAccountAsync returns recurring transaction templates with the correct properties mapped
    ///     to the ViewModel.<br />
    ///     (EN) Verifies that GetTemplatesByAccountAsync returns recurring transaction templates with the correct properties
    ///     mapped to the ViewModel.<br />
    ///     (VI) Xác minh rằng GetTemplatesByAccountAsync trả về các mẫu giao dịch định kỳ với các thuộc tính chính xác được
    ///     ánh xạ tới ViewModel.
    /// </summary>
    [Fact]
    public async Task GetTemplatesByAccountAsync_ShouldReturnCorrectTemplateProperties()
    {
        // Arrange
        var accountId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                AccountId = accountId,
                UserId = userId,
                Name = "Monthly Rent",
                Description = "Monthly rent payment",
                Amount = 1500m,
                TransactionType = RecurringTransactionType.Expense,
                Category = "Housing",
                Frequency = RecurrenceFrequency.Monthly,
                IsActive = true,
                AutoGenerate = true,
                DaysInAdvance = 30,
                StartDate = DateTime.UtcNow.Date,
                NextExecutionDate = DateTime.UtcNow.Date.AddDays(30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var templatesMock = templates.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetTemplatesByAccountAsync(accountId);

        // Assert
        var recurringTransactionTemplateViewModels = result.ToList();
        recurringTransactionTemplateViewModels.Should().NotBeNull();
        recurringTransactionTemplateViewModels.Should().HaveCount(1);

        var template = recurringTransactionTemplateViewModels.First();
        template.AccountId.Should().Be(accountId);
        template.UserId.Should().Be(userId);
        template.Name.Should().Be("Monthly Rent");
        template.Description.Should().Be("Monthly rent payment");
        template.Amount.Should().Be(1500m);
        template.TransactionType.Should().Be(RecurringTransactionType.Expense);
        template.Category.Should().Be("Housing");
        template.Frequency.Should().Be(RecurrenceFrequency.Monthly);
        template.IsActive.Should().BeTrue();
        template.AutoGenerate.Should().BeTrue();
        template.DaysInAdvance.Should().Be(30);
    }

    /// <summary>
    ///     Verifies that GetTemplatesByAccountAsync returns multiple templates with different frequencies for the specified
    ///     account.<br />
    ///     (EN) Verifies that GetTemplatesByAccountAsync returns multiple templates with different frequencies for the
    ///     specified account.<br />
    ///     (VI) Xác minh rằng GetTemplatesByAccountAsync trả về nhiều mẫu với các tần suất khác nhau cho tài khoản được chỉ
    ///     định.
    /// </summary>
    [Fact]
    public async Task GetTemplatesByAccountAsync_ShouldReturnMultipleTemplatesWithDifferentFrequencies()
    {
        // Arrange
        var accountId = Guid.CreateVersion7();

        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                AccountId = accountId,
                Name = "Daily Template",
                Frequency = RecurrenceFrequency.Daily,
                IsActive = true
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                AccountId = accountId,
                Name = "Weekly Template",
                Frequency = RecurrenceFrequency.Weekly,
                IsActive = true
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                AccountId = accountId,
                Name = "Monthly Template",
                Frequency = RecurrenceFrequency.Monthly,
                IsActive = true
            }
        };

        var templatesMock = templates.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetTemplatesByAccountAsync(accountId);

        // Assert
        var recurringTransactionTemplateViewModels = result.ToList();
        recurringTransactionTemplateViewModels.Should().NotBeNull();
        recurringTransactionTemplateViewModels.Should().HaveCount(3);
        recurringTransactionTemplateViewModels.Should().OnlyContain(t => t.AccountId == accountId);

        recurringTransactionTemplateViewModels.Should().Contain(t => t.Frequency == RecurrenceFrequency.Daily);
        recurringTransactionTemplateViewModels.Should().Contain(t => t.Frequency == RecurrenceFrequency.Weekly);
        recurringTransactionTemplateViewModels.Should().Contain(t => t.Frequency == RecurrenceFrequency.Monthly);
    }
}