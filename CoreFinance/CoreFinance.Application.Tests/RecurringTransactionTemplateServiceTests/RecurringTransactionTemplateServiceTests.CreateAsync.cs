using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.Exceptions;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

/// <summary>
///     Contains test cases for the CreateAsync method of RecurringTransactionTemplateService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức CreateAsync của RecurringTransactionTemplateService. (VI)
/// </summary>
public partial class RecurringTransactionTemplateServiceTests
{
    /// <summary>
    ///     Verifies that CreateAsync returns a ViewModel when the creation of a recurring transaction template is successful.
    ///     (EN)<br />
    ///     Xác minh rằng CreateAsync trả về ViewModel khi việc tạo một mẫu giao dịch định kỳ thành công. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldReturnViewModel_WhenCreationIsSuccessful()
    {
        // Arrange
        var createRequest = new RecurringTransactionTemplateCreateRequest
        {
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            Name = "Netflix Subscription",
            Description = "Monthly Netflix subscription",
            Amount = 15.99m,
            TransactionType = RecurringTransactionType.Expense,
            Category = "Entertainment",
            Frequency = RecurrenceFrequency.Monthly,
            StartDate = DateTime.UtcNow.Date,
            IsActive = true,
            AutoGenerate = false, // Set to false to avoid complex mocking
            DaysInAdvance = 30
        };

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.CreateAsync(It.IsAny<RecurringTransactionTemplate>()))
            .ReturnsAsync(1);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(createRequest.UserId);
        result.AccountId.Should().Be(createRequest.AccountId);
        result.Name.Should().Be(createRequest.Name);
        result.Description.Should().Be(createRequest.Description);
        result.Amount.Should().Be(createRequest.Amount);
        result.TransactionType.Should().Be(createRequest.TransactionType);
        result.Category.Should().Be(createRequest.Category);
        result.Frequency.Should().Be(createRequest.Frequency);
        result.StartDate.Should().Be(createRequest.StartDate);
        result.NextExecutionDate.Should().Be(createRequest.StartDate); // Should be set to StartDate
        result.IsActive.Should().Be(createRequest.IsActive);
        result.AutoGenerate.Should().Be(createRequest.AutoGenerate);
        result.DaysInAdvance.Should().Be(createRequest.DaysInAdvance);

        templateRepoMock.Verify(r => r.CreateAsync(It.IsAny<RecurringTransactionTemplate>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     Verifies that CreateAsync sets the NextExecutionDate to the StartDate when NextExecutionDate is not explicitly
    ///     provided. (EN)<br />
    ///     Xác minh rằng CreateAsync đặt NextExecutionDate thành StartDate khi NextExecutionDate không được cung cấp rõ ràng.
    ///     (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldSetNextExecutionDateToStartDate_WhenNextExecutionDateIsDefault()
    {
        // Arrange
        var createRequest = new RecurringTransactionTemplateCreateRequest
        {
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            Name = "Test Template",
            Amount = 100m,
            StartDate = DateTime.UtcNow.Date,
            AutoGenerate = false
            // NextExecutionDate is not set, should default to StartDate
        };

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.CreateAsync(It.IsAny<RecurringTransactionTemplate>()))
            .ReturnsAsync(1);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.NextExecutionDate.Should().Be(createRequest.StartDate);
    }

    /// <summary>
    ///     Verifies that CreateAsync generates expected transactions when AutoGenerate is true. (EN)<br />
    ///     Xác minh rằng CreateAsync tạo các giao dịch dự kiến khi AutoGenerate là true. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldGenerateExpectedTransactions_WhenAutoGenerateIsTrue()
    {
        // Arrange
        var createRequest = new RecurringTransactionTemplateCreateRequest
        {
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            Name = "Auto Generate Template",
            Amount = 100m,
            StartDate = DateTime.UtcNow.Date,
            AutoGenerate = true,
            DaysInAdvance = 30,
            IsActive = true
        };

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.CreateAsync(It.IsAny<RecurringTransactionTemplate>()))
            .ReturnsAsync(1);
        templateRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new RecurringTransactionTemplate
            {
                Id = Guid.CreateVersion7(),
                UserId = createRequest.UserId,
                AccountId = createRequest.AccountId,
                Name = createRequest.Name,
                Amount = createRequest.Amount,
                IsActive = true,
                AutoGenerate = true,
                NextExecutionDate = createRequest.StartDate,
                Frequency = RecurrenceFrequency.Monthly,
                DaysInAdvance = 30
            });

        var expectedTransactionRepoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        expectedTransactionRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(new List<ExpectedTransaction>().BuildMock());
        expectedTransactionRepoMock.Setup(r => r.CreateAsync(It.IsAny<ExpectedTransaction>()))
            .ReturnsAsync(1);
        templateRepoMock.Setup(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()))
            .ReturnsAsync(1);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>())
            .Returns(expectedTransactionRepoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.AutoGenerate.Should().BeTrue();

        // Verify that GenerateExpectedTransactionsAsync was called (indirectly through the additional repository calls)
        templateRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        expectedTransactionRepoMock.Verify(r => r.GetNoTrackingEntities(), Times.Once);
    }

    /// <summary>
    ///     Verifies that CreateAsync throws a CreateFailedException when the repository's create operation returns zero
    ///     affected count. (EN)<br />
    ///     Xác minh rằng CreateAsync ném ra CreateFailedException khi thao tác tạo của repository trả về số bản ghi bị ảnh
    ///     hưởng bằng không. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldThrowCreateFailedException_WhenRepositoryReturnsZeroAffectedCount()
    {
        // Arrange
        var createRequest = new RecurringTransactionTemplateCreateRequest
        {
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            Name = "Test Template",
            Amount = 100m,
            AutoGenerate = false
        };

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.CreateAsync(It.IsAny<RecurringTransactionTemplate>()))
            .ReturnsAsync(0); // Simulate 0 records affected

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.CreateAsync(createRequest);

        // Assert
        await act.Should().ThrowAsync<CreateFailedException>();

        templateRepoMock.Verify(r => r.CreateAsync(It.IsAny<RecurringTransactionTemplate>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}