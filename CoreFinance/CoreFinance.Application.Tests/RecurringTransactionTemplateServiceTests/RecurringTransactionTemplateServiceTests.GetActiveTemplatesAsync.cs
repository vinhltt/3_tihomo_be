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
    ///     (EN) Verifies that GetActiveTemplatesAsync keeps only active templates from the owner-scoped repository query.
    ///     Owner isolation itself is enforced by the real repository and is proven against PostgreSQL, not here.<br />
    ///     (VI) Xác minh rằng GetActiveTemplatesAsync chỉ giữ các mẫu đang hoạt động từ truy vấn repository đã giới hạn theo
    ///     owner. Việc cô lập owner do repository thật đảm nhiệm và được chứng minh trên PostgreSQL, không phải ở đây.
    /// </summary>
    [Fact]
    public async Task GetActiveTemplatesAsync_ShouldReturnOnlyActiveTemplates_FromOwnerScopedRepository()
    {
        // Arrange
        var ownerId = Guid.CreateVersion7();

        // The mocked repository stands in for BaseRepository, which already returns only the current owner's rows.
        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = ownerId,
                Name = "Active Template 1",
                IsActive = true
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = ownerId,
                Name = "Inactive Template",
                IsActive = false
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = ownerId,
                Name = "Active Template 2",
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
        var service = CreateService(unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetActiveTemplatesAsync();

        // Assert
        var recurringTransactionTemplateViewModels = result.ToList();
        recurringTransactionTemplateViewModels.Should().HaveCount(2);
        recurringTransactionTemplateViewModels.Should().OnlyContain(t => t.IsActive);
        recurringTransactionTemplateViewModels.Select(t => t.Name).Should()
            .BeEquivalentTo("Active Template 1", "Active Template 2");
    }

    /// <summary>
    ///     (EN) Verifies that GetActiveTemplatesAsync returns an empty list when no active templates exist.<br />
    ///     (VI) Xác minh rằng GetActiveTemplatesAsync trả về danh sách rỗng khi không có mẫu đang hoạt động nào tồn tại.
    /// </summary>
    [Fact]
    public async Task GetActiveTemplatesAsync_ShouldReturnEmptyList_WhenNoActiveTemplatesExist()
    {
        // Arrange
        var ownerId = Guid.CreateVersion7();

        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = ownerId,
                Name = "Inactive Template 1",
                IsActive = false
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = ownerId,
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
        var service = CreateService(unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetActiveTemplatesAsync();

        // Assert
        var recurringTransactionTemplateViewModels = result.ToList();
        recurringTransactionTemplateViewModels.Should().NotBeNull();
        recurringTransactionTemplateViewModels.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Verifies that GetActiveTemplatesAsync reads only through GetNoTrackingEntities (the owner-scoped query) and
    ///     performs no write, transaction, lock or other repository access.<br />
    ///     (VI) Xác minh rằng GetActiveTemplatesAsync chỉ đọc qua GetNoTrackingEntities (truy vấn đã giới hạn theo owner) và
    ///     không ghi, không mở transaction, không khóa hay truy cập repository nào khác.
    /// </summary>
    [Fact]
    public async Task GetActiveTemplatesAsync_ShouldReadOnlyThroughNoTrackingEntities_WithoutWritesOrLocks()
    {
        // Arrange
        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                UserId = Guid.CreateVersion7(),
                Name = "Active Template",
                IsActive = true
            }
        };

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>(MockBehavior.Strict);
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templates.BuildMock());

        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = CreateService(unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetActiveTemplatesAsync();

        // Assert
        result.Should().ContainSingle().Which.Name.Should().Be("Active Template");
        repoMock.Verify(r => r.GetNoTrackingEntities(), Times.Once);
        repoMock.VerifyNoOtherCalls();
        unitOfWorkMock.Verify(u => u.Repository<RecurringTransactionTemplate, Guid>(), Times.Once);
        unitOfWorkMock.VerifyNoOtherCalls();
    }

    /// <summary>
    ///     (EN) Verifies that GetActiveTemplatesAsync returns an empty list when no templates exist in the repository.<br />
    ///     (VI) Xác minh rằng GetActiveTemplatesAsync trả về danh sách rỗng khi không có mẫu nào tồn tại trong repository.
    /// </summary>
    [Fact]
    public async Task GetActiveTemplatesAsync_ShouldReturnEmptyList_WhenNoTemplatesExist()
    {
        // Arrange
        var templatesMock = new List<RecurringTransactionTemplate>().BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = CreateService(unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetActiveTemplatesAsync();

        // Assert
        var recurringTransactionTemplateViewModels = result.ToList();
        recurringTransactionTemplateViewModels.Should().NotBeNull();
        recurringTransactionTemplateViewModels.Should().BeEmpty();
    }
}
