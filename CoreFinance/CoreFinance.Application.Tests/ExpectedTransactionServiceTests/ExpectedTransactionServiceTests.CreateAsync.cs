using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.Exceptions;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

/// <summary>
///     Contains test cases for the CreateAsync method of ExpectedTransactionService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức CreateAsync của ExpectedTransactionService. (VI)
/// </summary>
public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     Verifies that CreateAsync returns a ViewModel when the creation of a single expected transaction is successful.
    ///     (EN)<br />
    ///     Xác minh rằng CreateAsync trả về ViewModel khi việc tạo một giao dịch dự kiến đơn lẻ thành công. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldReturnViewModel_WhenCreationIsSuccessful()
    {
        // Arrange
        var createRequest = new ExpectedTransactionCreateRequest
        {
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            RecurringTransactionTemplateId = Guid.CreateVersion7(),
            ExpectedDate = DateTime.UtcNow.AddDays(7),
            ExpectedAmount = 100.50m,
            Description = "Monthly subscription",
            TransactionType = RecurringTransactionType.Expense,
            Category = "Entertainment"
        };

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<ExpectedTransaction>()))
            .ReturnsAsync(1); // Simulate 1 record affected

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(createRequest.UserId);
        result.AccountId.Should().Be(createRequest.AccountId);
        result.ExpectedDate.Should().Be(createRequest.ExpectedDate);
        result.ExpectedAmount.Should().Be(createRequest.ExpectedAmount);
        result.Description.Should().Be(createRequest.Description);
        result.TransactionType.Should().Be(createRequest.TransactionType);
        result.Category.Should().Be(createRequest.Category);
        result.Status.Should().Be(ExpectedTransactionStatus.Pending); // Default value set in override

        // Verify that the repository method was called
        repoMock.Verify(r => r.CreateAsync(It.IsAny<ExpectedTransaction>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    ///     Verifies that CreateAsync sets the default status to Pending when the status is not explicitly provided in the
    ///     request. (EN)<br />
    ///     Xác minh rằng CreateAsync đặt trạng thái mặc định là Pending khi trạng thái không được cung cấp rõ ràng trong yêu
    ///     cầu. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldSetDefaultStatus_WhenStatusIsNotProvided()
    {
        // Arrange
        var createRequest = new ExpectedTransactionCreateRequest
        {
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            ExpectedDate = DateTime.UtcNow.AddDays(7),
            ExpectedAmount = 100.50m,
            Description = "Test transaction"
            // Status is not set, should default to Pending
        };

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<ExpectedTransaction>()))
            .ReturnsAsync(1);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ExpectedTransactionStatus.Pending);
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
        var createRequest = new ExpectedTransactionCreateRequest
        {
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            ExpectedDate = DateTime.UtcNow.AddDays(7),
            ExpectedAmount = 100.50m
        };

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<ExpectedTransaction>()))
            .ReturnsAsync(0); // Simulate 0 records affected

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.CreateAsync(createRequest);

        // Assert
        await act.Should().ThrowAsync<CreateFailedException>();

        repoMock.Verify(r => r.CreateAsync(It.IsAny<ExpectedTransaction>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    ///     Verifies that CreateAsync rolls back the transaction when the repository's create operation throws an exception.
    ///     (EN)<br />
    ///     Xác minh rằng CreateAsync rollback transaction khi thao tác tạo của repository ném ra một ngoại lệ. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldRollbackTransaction_WhenRepositoryThrowsException()
    {
        // Arrange
        var createRequest = new ExpectedTransactionCreateRequest
        {
            UserId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            ExpectedDate = DateTime.UtcNow.AddDays(7),
            ExpectedAmount = 100.50m
        };

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<ExpectedTransaction>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.CreateAsync(createRequest);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB error");

        repoMock.Verify(r => r.CreateAsync(It.IsAny<ExpectedTransaction>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    }
}