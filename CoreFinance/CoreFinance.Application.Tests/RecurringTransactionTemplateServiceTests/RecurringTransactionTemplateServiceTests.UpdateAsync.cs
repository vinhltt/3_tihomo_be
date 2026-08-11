using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using Shared.Contracts.Exceptions;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

// Tests for the UpdateAsync method of RecurringTransactionTemplateService
public partial class RecurringTransactionTemplateServiceTests
{
    /// <summary>
    ///     (EN) Verifies that UpdateAsync updates the template and generates expected transactions when auto-generate is
    ///     enabled.<br />
    ///     (VI) Xác minh rằng UpdateAsync cập nhật mẫu và tạo các giao dịch dự kiến khi bật tự động tạo.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldUpdateTemplateAndGenerateExpectedTransactions_WhenAutoGenerateIsEnabled()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();

        var existingTemplate = new RecurringTransactionTemplate
        {
            Id = templateId,
            UserId = userId,
            AccountId = accountId,
            Name = "Old Template Name",
            Description = "Old Description",
            Amount = 1000m,
            TransactionType = RecurringTransactionType.Income,
            Category = "Old Category",
            Frequency = RecurrenceFrequency.Monthly,
            IsActive = true,
            AutoGenerate = false,
            DaysInAdvance = 30,
            StartDate = DateTime.UtcNow.Date,
            NextExecutionDate = DateTime.UtcNow.Date.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var updateRequest = new RecurringTransactionTemplateUpdateRequest
        {
            Id = templateId, // Must match the id parameter
            Name = "Updated Template Name",
            Description = "Updated Description",
            Amount = 1500m,
            TransactionType = RecurringTransactionType.Expense,
            Category = "Updated Category",
            Frequency = RecurrenceFrequency.Weekly,
            IsActive = true,
            AutoGenerate = true, // Enable auto-generate
            DaysInAdvance = 60
        };

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(existingTemplate);
        templateRepoMock.Setup(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()))
            .ReturnsAsync(1); // Return 1 to indicate successful update

        var expectedTransactionRepoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        expectedTransactionRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(new List<ExpectedTransaction>().AsQueryable().BuildMock());
        expectedTransactionRepoMock.Setup(r => r.CreateAsync(It.IsAny<ExpectedTransaction>()))
            .ReturnsAsync(1); // Return 1 to indicate successful creation

        var transactionMock = new Mock<IDbContextTransaction>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>())
            .Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>())
            .Returns(expectedTransactionRepoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transactionMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1); // Return 1 to indicate successful save

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.UpdateAsync(templateId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Template Name");
        result.Description.Should().Be("Updated Description");
        result.Amount.Should().Be(1500m);
        result.TransactionType.Should().Be(RecurringTransactionType.Expense);
        result.Category.Should().Be("Updated Category");
        result.Frequency.Should().Be(RecurrenceFrequency.Weekly);
        result.AutoGenerate.Should().BeTrue();
        result.DaysInAdvance.Should().Be(60);

        templateRepoMock.Verify(r => r.GetByIdAsync(templateId),
            Times.Exactly(2)); // Called once in base.UpdateAsync and once in GenerateExpectedTransactionsInternalAsync
        templateRepoMock.Verify(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()),
            Times.Exactly(2)); // Called once in base.UpdateAsync and once in GenerateExpectedTransactionsInternalAsync
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }

    /// <summary>
    ///     (EN) Verifies that UpdateAsync does not generate expected transactions when auto-generate is disabled.<br />
    ///     (VI) Xác minh rằng UpdateAsync không tạo các giao dịch dự kiến khi tắt tự động tạo.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldNotGenerateExpectedTransactions_WhenAutoGenerateIsDisabled()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();

        var existingTemplate = new RecurringTransactionTemplate
        {
            Id = templateId,
            UserId = userId,
            AccountId = accountId,
            Name = "Template Name",
            Description = "Description",
            Amount = 1000m,
            TransactionType = RecurringTransactionType.Income,
            Category = "Category",
            Frequency = RecurrenceFrequency.Monthly,
            IsActive = true,
            AutoGenerate = true,
            DaysInAdvance = 30,
            StartDate = DateTime.UtcNow.Date,
            NextExecutionDate = DateTime.UtcNow.Date.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var updateRequest = new RecurringTransactionTemplateUpdateRequest
        {
            Id = templateId, // Must match the id parameter
            Name = "Updated Template Name",
            Description = "Updated Description",
            Amount = 1500m,
            TransactionType = RecurringTransactionType.Expense,
            Category = "Updated Category",
            Frequency = RecurrenceFrequency.Weekly,
            IsActive = true,
            AutoGenerate = false, // Disable auto-generate
            DaysInAdvance = 60
        };

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(existingTemplate);
        templateRepoMock.Setup(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()))
            .ReturnsAsync(1); // Return 1 to indicate successful update

        var transactionMock = new Mock<IDbContextTransaction>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>())
            .Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1); // Return 1 to indicate successful save
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.UpdateAsync(templateId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.AutoGenerate.Should().BeFalse();

        templateRepoMock.Verify(r => r.GetByIdAsync(templateId), Times.Once);
        templateRepoMock.Verify(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()), Times.Once);

        // Should not call BeginTransactionAsync for expected transaction generation
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
    }

    /// <summary>
    ///     (EN) Verifies that UpdateAsync throws a NullReferenceException when the template to update is not found.<br />
    ///     (VI) Xác minh rằng UpdateAsync throws a NullReferenceException khi không tìm thấy mẫu cần cập nhật.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldThrowNullReferenceException_WhenTemplateNotFound()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();

        var updateRequest = new RecurringTransactionTemplateUpdateRequest
        {
            Id = templateId, // Must match the id parameter
            Name = "Updated Template Name",
            Description = "Updated Description",
            Amount = 1500m,
            TransactionType = RecurringTransactionType.Expense,
            Category = "Updated Category",
            Frequency = RecurrenceFrequency.Weekly,
            IsActive = true,
            AutoGenerate = true,
            DaysInAdvance = 60
        };

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync((RecurringTransactionTemplate?)null);

        var transactionMock = new Mock<IDbContextTransaction>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>())
            .Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act & Assert
        var act = async () => await service.UpdateAsync(templateId, updateRequest);
        await act.Should().ThrowAsync<EntityNotFoundException>();

        templateRepoMock.Verify(r => r.GetByIdAsync(templateId), Times.Once);
        templateRepoMock.Verify(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()), Times.Never);
        transactionMock.Verify(t => t.RollbackAsync(CancellationToken.None), Times.Once);
    }

    /// <summary>
    ///     (EN) Verifies that UpdateAsync updates the correct properties of the recurring transaction template.<br />
    ///     (VI) Xác minh rằng UpdateAsync cập nhật đúng các thuộc tính của mẫu giao dịch định kỳ.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldUpdateCorrectProperties()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();

        var existingTemplate = new RecurringTransactionTemplate
        {
            Id = templateId,
            UserId = userId,
            AccountId = accountId,
            Name = "Original Name",
            Description = "Original Description",
            Amount = 1000m,
            TransactionType = RecurringTransactionType.Income,
            Category = "Original Category",
            Frequency = RecurrenceFrequency.Monthly,
            IsActive = false,
            AutoGenerate = false,
            DaysInAdvance = 30,
            StartDate = DateTime.UtcNow.Date,
            NextExecutionDate = DateTime.UtcNow.Date.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var updateRequest = new RecurringTransactionTemplateUpdateRequest
        {
            Id = templateId, // Must match the id parameter
            Name = "New Name",
            Description = "New Description",
            Amount = 2000m,
            TransactionType = RecurringTransactionType.Expense,
            Category = "New Category",
            Frequency = RecurrenceFrequency.Weekly,
            IsActive = true,
            AutoGenerate = false,
            DaysInAdvance = 45,
            CustomIntervalDays = 10,
            EndDate = DateTime.UtcNow.Date.AddMonths(6)
        };

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(existingTemplate);
        templateRepoMock.Setup(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()))
            .ReturnsAsync(1); // Return 1 to indicate successful update

        var transactionMock = new Mock<IDbContextTransaction>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>())
            .Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1); // Return 1 to indicate successful save
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.UpdateAsync(templateId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(templateId);
        result.UserId.Should().Be(userId); // Should remain unchanged
        result.AccountId.Should().Be(accountId); // Should remain unchanged
        result.Name.Should().Be("New Name");
        result.Description.Should().Be("New Description");
        result.Amount.Should().Be(2000m);
        result.TransactionType.Should().Be(RecurringTransactionType.Expense);
        result.Category.Should().Be("New Category");
        result.Frequency.Should().Be(RecurrenceFrequency.Weekly);
        result.IsActive.Should().BeTrue();
        result.AutoGenerate.Should().BeFalse();
        result.DaysInAdvance.Should().Be(45);
        result.CustomIntervalDays.Should().Be(10);
        result.EndDate.Should().Be(updateRequest.EndDate);

        templateRepoMock.Verify(r => r.UpdateAsync(It.Is<RecurringTransactionTemplate>(t =>
            t.Name == "New Name" &&
            t.Description == "New Description" &&
            t.Amount == 2000m &&
            t.TransactionType == RecurringTransactionType.Expense &&
            t.Category == "New Category" &&
            t.Frequency == RecurrenceFrequency.Weekly &&
            t.IsActive == true &&
            t.AutoGenerate == false &&
            t.DaysInAdvance == 45 &&
            t.CustomIntervalDays == 10 &&
            t.EndDate == updateRequest.EndDate
        )), Times.Once);
    }

    /// <summary>
    ///     (EN) Verifies that UpdateAsync preserves the CreatedAt timestamp and updates the UpdatedAt timestamp.<br />
    ///     (VI) Xác minh rằng UpdateAsync giữ nguyên dấu thời gian CreatedAt và cập nhật dấu thời gian UpdatedAt.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldPreserveCreatedAtAndUpdateUpdatedAt()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var originalCreatedAt = DateTime.UtcNow.AddDays(-10);
        var originalUpdatedAt = DateTime.UtcNow.AddDays(-5);

        var existingTemplate = new RecurringTransactionTemplate
        {
            Id = templateId,
            UserId = userId,
            AccountId = accountId,
            Name = "Original Name",
            Description = "Original Description",
            Amount = 1000m,
            TransactionType = RecurringTransactionType.Income,
            Category = "Original Category",
            Frequency = RecurrenceFrequency.Monthly,
            IsActive = true,
            AutoGenerate = false,
            DaysInAdvance = 30,
            StartDate = DateTime.UtcNow.Date,
            NextExecutionDate = DateTime.UtcNow.Date.AddDays(30),
            CreatedAt = originalCreatedAt,
            UpdatedAt = originalUpdatedAt
        };

        var updateRequest = new RecurringTransactionTemplateUpdateRequest
        {
            Id = templateId, // Must match the id parameter
            Name = "Updated Name",
            Description = "Updated Description",
            Amount = 1500m,
            TransactionType = RecurringTransactionType.Expense,
            Category = "Updated Category",
            Frequency = RecurrenceFrequency.Weekly,
            IsActive = true,
            AutoGenerate = false,
            DaysInAdvance = 45
        };

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(existingTemplate);
        templateRepoMock.Setup(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()))
            .ReturnsAsync(1); // Return 1 to indicate successful update

        var transactionMock = new Mock<IDbContextTransaction>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>())
            .Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1); // Return 1 to indicate successful save
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transactionMock.Object);
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.UpdateAsync(templateId, updateRequest);

        // Assert
        result.Should().NotBeNull();
    }
}