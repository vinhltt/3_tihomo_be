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

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

public partial class RecurringTransactionTemplateServiceTests
{
    /// <summary>
    ///     (EN) Verifies that GenerateExpectedTransactionsAsync generates transactions when the template is active and
    ///     auto-generate is enabled.<br />
    ///     (VI) Xác minh rằng GenerateExpectedTransactionsAsync tạo giao dịch khi mẫu đang hoạt động và bật tự động tạo.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldGenerateTransactions_WhenTemplateIsActiveAndAutoGenerate()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var daysInAdvance = 30;
        var template = new RecurringTransactionTemplate
        {
            Id = templateId,
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            Name = "Monthly Rent",
            Amount = 1000m,
            IsActive = true,
            AutoGenerate = true,
            NextExecutionDate = DateTime.UtcNow.Date,
            Frequency = RecurrenceFrequency.Monthly,
            TransactionType = RecurringTransactionType.Expense,
            Category = "Housing",
            Description = "Monthly rent payment",
            DaysInAdvance = daysInAdvance
        };

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        templateRepoMock.Setup(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()))
            .ReturnsAsync(1);

        var expectedTransactionRepoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        expectedTransactionRepoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(new List<ExpectedTransaction>().AsQueryable().BuildMock());
        expectedTransactionRepoMock.Setup(r => r.CreateAsync(It.IsAny<ExpectedTransaction>()))
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
        await service.GenerateExpectedTransactionsAsync(templateId, daysInAdvance);

        // Assert
        templateRepoMock.Verify(r => r.GetByIdAsync(templateId), Times.Once);
        expectedTransactionRepoMock.Verify(r => r.GetNoTrackingEntities(), Times.Once);
        expectedTransactionRepoMock.Verify(r => r.CreateAsync(It.IsAny<ExpectedTransaction>()), Times.Once);
        templateRepoMock.Verify(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     (EN) Verifies that GenerateExpectedTransactionsAsync does not generate transactions when the template is inactive.
    ///     <br />
    ///     (VI) Xác minh rằng GenerateExpectedTransactionsAsync không tạo giao dịch khi mẫu không hoạt động.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldNotGenerateTransactions_WhenTemplateIsInactive()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var daysInAdvance = 30;
        var template = new RecurringTransactionTemplate
        {
            Id = templateId,
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            Name = "Inactive Template",
            Amount = 1000m,
            IsActive = false, // Inactive template
            AutoGenerate = true,
            NextExecutionDate = DateTime.UtcNow.Date,
            Frequency = RecurrenceFrequency.Monthly
        };

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        var expectedTransactionRepoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>())
            .Returns(expectedTransactionRepoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        await service.GenerateExpectedTransactionsAsync(templateId, daysInAdvance);

        // Assert
        templateRepoMock.Verify(r => r.GetByIdAsync(templateId), Times.Once);
        expectedTransactionRepoMock.Verify(r => r.CreateAsync(It.IsAny<ExpectedTransaction>()), Times.Never);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     (EN) Verifies that GenerateExpectedTransactionsAsync does not generate transactions when auto-generate is false.
    ///     <br />
    ///     (VI) Xác minh rằng GenerateExpectedTransactionsAsync không tạo giao dịch khi tự động tạo tắt.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldNotGenerateTransactions_WhenAutoGenerateIsFalse()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var daysInAdvance = 30;
        var template = new RecurringTransactionTemplate
        {
            Id = templateId,
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            Name = "Manual Template",
            Amount = 1000m,
            IsActive = true,
            AutoGenerate = false, // Auto-generate disabled
            NextExecutionDate = DateTime.UtcNow.Date,
            Frequency = RecurrenceFrequency.Monthly
        };

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        var expectedTransactionRepoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>())
            .Returns(expectedTransactionRepoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        await service.GenerateExpectedTransactionsAsync(templateId, daysInAdvance);

        // Assert
        templateRepoMock.Verify(r => r.GetByIdAsync(templateId), Times.Once);
        expectedTransactionRepoMock.Verify(r => r.CreateAsync(It.IsAny<ExpectedTransaction>()), Times.Never);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     (EN) Verifies that GenerateExpectedTransactionsAsync rolls back the transaction when an exception occurs during the
    ///     process.<br />
    ///     (VI) Xác minh rằng GenerateExpectedTransactionsAsync thực hiện rollback giao dịch khi có ngoại lệ xảy ra trong quá
    ///     trình xử lý.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldRollbackTransaction_WhenExceptionOccurs()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var daysInAdvance = 30;

        var templateRepoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        templateRepoMock.Setup(r => r.GetByIdAsync(templateId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(templateRepoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var act = async () => await service.GenerateExpectedTransactionsAsync(templateId, daysInAdvance);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Database error");

        templateRepoMock.Verify(r => r.GetByIdAsync(templateId), Times.Once);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify that error was logged
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Error generating expected transactions for template")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}