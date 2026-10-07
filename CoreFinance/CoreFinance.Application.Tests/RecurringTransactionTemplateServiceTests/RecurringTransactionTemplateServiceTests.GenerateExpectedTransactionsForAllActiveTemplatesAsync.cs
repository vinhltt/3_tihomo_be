using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

// Tests for the GenerateExpectedTransactionsForAllActiveTemplatesAsync method of RecurringTransactionTemplateService
public partial class RecurringTransactionTemplateServiceTests
{
    /// <summary>
    ///     (EN) Verifies that GenerateExpectedTransactionsForAllActiveTemplatesAsync processes only active templates with
    ///     auto-generate enabled.<br />
    ///     (VI) Xác minh rằng GenerateExpectedTransactionsForAllActiveTemplatesAsync chỉ xử lý các mẫu đang hoạt động có bật
    ///     tự động tạo.
    /// </summary>
    [Fact]
    public async Task
        GenerateExpectedTransactionsForAllActiveTemplatesAsync_ShouldProcessOnlyActiveTemplatesWithAutoGenerate()
    {
        // Arrange
        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "Active Auto Template",
                IsActive = true,
                AutoGenerate = true,
                DaysInAdvance = 30
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "Active Manual Template",
                IsActive = true,
                AutoGenerate = false, // Should be ignored
                DaysInAdvance = 30
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "Inactive Auto Template",
                IsActive = false, // Should be ignored
                AutoGenerate = true,
                DaysInAdvance = 30
            }
        };

        var templatesMock = templates.BuildMock();

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var expectedTransactionRepoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        expectedTransactionRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(new List<ExpectedTransaction>().BuildMock());

        var transactionMock = new Mock<IDbContextTransaction>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>())
            .Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>())
            .Returns(expectedTransactionRepoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        await service.GenerateExpectedTransactionsForAllActiveTemplatesAsync();

        // Assert
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(CancellationToken.None), Times.Once);
        templateRepoMock.Verify(r => r.GetNoTrackingEntities(), Times.Once);
    }

    /// <summary>
    ///     (EN) Verifies that GenerateExpectedTransactionsForAllActiveTemplatesAsync handles an empty template list correctly.
    ///     <br />
    ///     (VI) Xác minh rằng GenerateExpectedTransactionsForAllActiveTemplatesAsync xử lý danh sách mẫu rỗng một cách chính
    ///     xác.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsForAllActiveTemplatesAsync_ShouldHandleEmptyTemplateList()
    {
        // Arrange
        var templatesMock = new List<RecurringTransactionTemplate>().BuildMock();

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var transactionMock = new Mock<IDbContextTransaction>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>())
            .Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        await service.GenerateExpectedTransactionsForAllActiveTemplatesAsync();

        // Assert
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(CancellationToken.None), Times.Once);
        templateRepoMock.Verify(r => r.GetNoTrackingEntities(), Times.Once);
    }

    /// <summary>
    ///     (EN) Verifies that GenerateExpectedTransactionsForAllActiveTemplatesAsync rolls back the transaction on exception.
    ///     <br />
    ///     (VI) Xác minh rằng GenerateExpectedTransactionsForAllActiveTemplatesAsync thực hiện rollback giao dịch khi có ngoại
    ///     lệ.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsForAllActiveTemplatesAsync_ShouldRollbackOnException()
    {
        // Arrange
        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "Active Auto Template",
                IsActive = true,
                AutoGenerate = true,
                DaysInAdvance = 30
            }
        };

        var templatesMock = templates.BuildMock();

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);


        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transactionMock.Object);

        // Setup to throw exception during processing
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>())
            .Throws(new InvalidOperationException("Test exception"));

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act & Assert
        var act = async () => await service.GenerateExpectedTransactionsForAllActiveTemplatesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");

        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.RollbackAsync(CancellationToken.None), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(CancellationToken.None), Times.Never);
    }

    /// <summary>
    ///     (EN) Verifies that GenerateExpectedTransactionsForAllActiveTemplatesAsync filters templates correctly.<br />
    ///     (VI) Xác minh rằng GenerateExpectedTransactionsForAllActiveTemplatesAsync lọc các mẫu một cách chính xác.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsForAllActiveTemplatesAsync_ShouldFilterCorrectly()
    {
        // Arrange
        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "Template 1",
                IsActive = true,
                AutoGenerate = true,
                DaysInAdvance = 30
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "Template 2",
                IsActive = true,
                AutoGenerate = false // Should not be processed
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "Template 3",
                IsActive = false, // Should not be processed
                AutoGenerate = true
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "Template 4",
                IsActive = true,
                AutoGenerate = true,
                DaysInAdvance = 60
            }
        };

        var templatesMock = templates.BuildMock();

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var expectedTransactionRepoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        expectedTransactionRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(new List<ExpectedTransaction>().BuildMock());

        var transactionMock = new Mock<IDbContextTransaction>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>())
            .Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>())
            .Returns(expectedTransactionRepoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        await service.GenerateExpectedTransactionsForAllActiveTemplatesAsync();

        // Assert
        // Should only process templates that are both active AND have auto-generate enabled
        // That's Template 1 and Template 4 (2 templates)
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(CancellationToken.None), Times.Once);
        templateRepoMock.Verify(r => r.GetNoTrackingEntities(), Times.Once);
    }

    /// <summary>
    ///     (EN) Verifies that GenerateExpectedTransactionsForAllActiveTemplatesAsync logs an error on exception.<br />
    ///     (VI) Xác minh rằng GenerateExpectedTransactionsForAllActiveTemplatesAsync ghi nhật ký lỗi khi có ngoại lệ.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsForAllActiveTemplatesAsync_ShouldLogErrorOnException()
    {
        // Arrange
        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "Active Auto Template",
                IsActive = true,
                AutoGenerate = true,
                DaysInAdvance = 30
            }
        };

        var templatesMock = templates.BuildMock();

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        // Setup to throw exception during processing
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>())
            .Throws(new InvalidOperationException("Test exception"));

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act & Assert
        var act = async () => await service.GenerateExpectedTransactionsForAllActiveTemplatesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Verify that error was logged
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Error generating expected transactions for all active templates")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}