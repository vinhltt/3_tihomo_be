using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

// Tests for the GetActiveTemplatesAsync method of RecurringTransactionTemplateService
public partial class RecurringTransactionTemplateServiceTests
{
    /// <summary>
    ///     (EN) Verifies that GetActiveTemplatesAsync returns only active templates for a specific user.<br />
    ///     (VI) Xác minh rằng GetActiveTemplatesAsync chỉ trả về các mẫu đang hoạt động cho một người dùng cụ thể.
    /// </summary>
    [Fact]
    public async Task GetActiveTemplatesAsync_ShouldReturnOnlyActiveTemplates_ForSpecificUser()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();

        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Name = "Active Template 1",
                IsActive = true
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Name = "Inactive Template",
                IsActive = false
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Name = "Active Template 2",
                IsActive = true
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = otherUserId,
                Name = "Other User Active Template",
                IsActive = true
            }
        };

        var templatesMock = templates.BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetActiveTemplatesAsync(userId);

        // Assert
        var recurringTransactionTemplateViewModels = result.ToList();
        recurringTransactionTemplateViewModels.Should().NotBeNull();
        recurringTransactionTemplateViewModels.Should().HaveCount(2);
        recurringTransactionTemplateViewModels.Should().OnlyContain(t => t.IsActive);
        recurringTransactionTemplateViewModels.Should().OnlyContain(t => t.UserId == userId);
        recurringTransactionTemplateViewModels.Select(t => t.Name).Should()
            .Contain(["Active Template 1", "Active Template 2"]);
    }

    /// <summary>
    ///     (EN) Verifies that GetActiveTemplatesAsync returns an empty list when no active templates exist for the user.<br />
    ///     (VI) Xác minh rằng GetActiveTemplatesAsync trả về danh sách rỗng khi không có mẫu đang hoạt động nào tồn tại cho
    ///     người dùng.
    /// </summary>
    [Fact]
    public async Task GetActiveTemplatesAsync_ShouldReturnEmptyList_WhenNoActiveTemplatesExist()
    {
        // Arrange
        var userId = Guid.CreateVersion7();

        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Name = "Inactive Template 1",
                IsActive = false
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Name = "Inactive Template 2",
                IsActive = false
            }
        };

        var templatesMock = templates.BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetActiveTemplatesAsync(userId);

        // Assert
        var recurringTransactionTemplateViewModels = result.ToList();
        recurringTransactionTemplateViewModels.Should().NotBeNull();
        recurringTransactionTemplateViewModels.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Verifies that GetActiveTemplatesAsync returns an empty list when the user has no templates.<br />
    ///     (VI) Xác minh rằng GetActiveTemplatesAsync trả về danh sách rỗng khi người dùng không có mẫu nào.
    /// </summary>
    [Fact]
    public async Task GetActiveTemplatesAsync_ShouldReturnEmptyList_WhenUserHasNoTemplates()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();

        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = otherUserId,
                Name = "Other User Template",
                IsActive = true
            }
        };

        var templatesMock = templates.BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetActiveTemplatesAsync(userId);

        // Assert
        var recurringTransactionTemplateViewModels = result.ToList();
        recurringTransactionTemplateViewModels.Should().NotBeNull();
        recurringTransactionTemplateViewModels.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Verifies that GetActiveTemplatesAsync returns an empty list when no templates exist in the repository.<br />
    ///     (VI) Xác minh rằng GetActiveTemplatesAsync trả về danh sách rỗng khi không có mẫu nào tồn tại trong repository.
    /// </summary>
    [Fact]
    public async Task GetActiveTemplatesAsync_ShouldReturnEmptyList_WhenNoTemplatesExist()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var templatesMock = new List<RecurringTransactionTemplate>().BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetActiveTemplatesAsync(userId);

        // Assert
        var recurringTransactionTemplateViewModels = result.ToList();
        recurringTransactionTemplateViewModels.Should().NotBeNull();
        recurringTransactionTemplateViewModels.Should().BeEmpty();
    }
}