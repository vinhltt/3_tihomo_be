using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

public partial class RecurringTransactionTemplateServiceTests
{
    /// <summary>
    ///     (EN) Verifies that ToggleActiveStatusAsync returns true when the template's active status is toggled successfully.
    ///     <br />
    ///     (VI) Xác minh rằng ToggleActiveStatusAsync trả về true khi trạng thái hoạt động của mẫu được chuyển đổi thành công.
    /// </summary>
    [Fact]
    public async Task ToggleActiveStatusAsync_ShouldReturnTrue_WhenTemplateIsToggledSuccessfully()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var template = new RecurringTransactionTemplate
        {
            Id = templateId,
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            Name = "Test Template",
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()))
            .ReturnsAsync(1);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.ToggleActiveStatusAsync(templateId, true);

        // Assert
        result.Should().BeTrue();
        template.IsActive.Should().BeTrue();
        template.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        repoMock.Verify(r => r.GetByIdAsync(templateId), Times.Once);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     (EN) Verifies that ToggleActiveStatusAsync returns false when the template is not found.<br />
    ///     (VI) Xác minh rằng ToggleActiveStatusAsync trả về false khi không tìm thấy mẫu.
    /// </summary>
    [Fact]
    public async Task ToggleActiveStatusAsync_ShouldReturnFalse_WhenTemplateNotFound()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync((RecurringTransactionTemplate?)null);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.ToggleActiveStatusAsync(templateId, true);

        // Assert
        result.Should().BeFalse();
        repoMock.Verify(r => r.GetByIdAsync(templateId), Times.Once);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    /// <summary>
    ///     (EN) Verifies that ToggleActiveStatusAsync toggles the active status from true to false correctly.<br />
    ///     (VI) Xác minh rằng ToggleActiveStatusAsync chuyển đổi trạng thái hoạt động từ true sang false một cách chính xác.
    /// </summary>
    [Fact]
    public async Task ToggleActiveStatusAsync_ShouldToggleFromTrueToFalse_WhenTemplateIsCurrentlyActive()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();
        var template = new RecurringTransactionTemplate
        {
            Id = templateId,
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            Name = "Active Template",
            IsActive = true, // Currently active
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()))
            .ReturnsAsync(1);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.ToggleActiveStatusAsync(templateId, false);

        // Assert
        result.Should().BeTrue();
        template.IsActive.Should().BeFalse(); // Should be toggled to false
        template.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        repoMock.Verify(r => r.GetByIdAsync(templateId), Times.Once);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>()), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     (EN) Verifies that ToggleActiveStatusAsync returns false when an exception occurs during the process.<br />
    ///     (VI) Xác minh rằng ToggleActiveStatusAsync trả về false khi có ngoại lệ xảy ra trong quá trình xử lý.
    /// </summary>
    [Fact]
    public async Task ToggleActiveStatusAsync_ShouldReturnFalse_WhenExceptionOccurs()
    {
        // Arrange
        var templateId = Guid.CreateVersion7();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetByIdAsync(templateId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.ToggleActiveStatusAsync(templateId, true);

        // Assert
        result.Should().BeFalse();
        repoMock.Verify(r => r.GetByIdAsync(templateId), Times.Once);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify that error was logged
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error toggling active status for template")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}