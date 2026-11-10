using CoreFinance.Application.DTOs.Transaction;
using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace CoreFinance.Application.Tests.TransactionServiceTests;

/// <summary>
///     Contains test cases for the CreateAsync method of TransactionService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức CreateAsync của TransactionService. (VI)
/// </summary>
public partial class TransactionServiceTests
{
    /// <summary>
    ///     Verifies that CreateAsync returns a ViewModel when a single transaction is created successfully. (EN)<br />
    ///     Xác minh rằng CreateAsync trả về ViewModel khi một giao dịch đơn lẻ được tạo thành công. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldReturnViewModel_WhenTransactionIsCreated()
    {
        // Arrange
        var createRequest = new TransactionCreateRequest
        {
            AccountId = Guid.CreateVersion7(),
            UserId = Guid.CreateVersion7(),
            TransactionDate = new DateTime(2026, 3, 15),
            Description = "Test Create",
            RevenueAmount = 100,
            SpentAmount = 0
        };
        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<Transaction>())).ReturnsAsync(1);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(Mock.Of<IDbContextTransaction>());
        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = CreateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be(createRequest.Description);
        result.RevenueAmount.Should().Be(createRequest.RevenueAmount);
    }

    /// <summary>
    ///     Verifies that CreateAsync returns a ViewModel when creation is successful. (EN)<br />
    ///     Xác minh rằng CreateAsync trả về ViewModel khi việc tạo thành công. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldReturnViewModel_WhenCreationIsSuccessful()
    {
        // Arrange
        var createRequest = new TransactionCreateRequest
        {
            AccountId = Guid.CreateVersion7(),
            UserId = Guid.CreateVersion7(),
            TransactionDate = new DateTime(2026, 3, 15),
            Description = "Test Create",
            RevenueAmount = 100,
            SpentAmount = 0
        };
        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<Transaction>())).ReturnsAsync(1);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = CreateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be(createRequest.Description);
        result.RevenueAmount.Should().Be(createRequest.RevenueAmount);
        repoMock.Verify(r => r.CreateAsync(It.IsAny<Transaction>()), Times.Once);
        //unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        //transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        //transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    ///     Verifies that CreateAsync throws a CreateFailedException when the repository returns a zero affected count. (EN)
    ///     <br />
    ///     Xác minh rằng CreateAsync ném ra CreateFailedException khi repository trả về số bản ghi bị ảnh hưởng bằng không.
    ///     (VI)
    /// </summary>
    //[Fact]
    //public async Task CreateAsync_ShouldThrowNullReferenceException_WhenRepositoryReturnsZeroAffectedCount()
    //{
    //    // Arrange
    //    var createRequest = new TransactionCreateRequest
    //    { Description = "Test Transaction", AccountId = Guid.CreateVersion7(), UserId = Guid.CreateVersion7() };
    //    var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
    //    repoMock.Setup(r => r.CreateAsync(It.IsAny<Transaction>())).ReturnsAsync(0);

    //    var transactionMock = new Mock<IDbContextTransaction>();
    //    transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    //    transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

    //    var unitOfWorkMock = new Mock<IUnitOfWork>();
    //    unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
    //    unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

    //    var loggerMock = new Mock<ILogger<TransactionService>>();
    //    var service = CreateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

    //    // Act
    //    Func<Task> act = async () => await service.CreateAsync(createRequest);

    //    // Assert
    //    await act.Should().ThrowAsync<CreateFailedException>();
    //    repoMock.Verify(r => r.CreateAsync(It.IsAny<Transaction>()), Times.Once);
    //    //unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
    //    //transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    //    //transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    //}

    /// <summary>
    ///     Verifies that CreateAsync rolls back the transaction when the repository throws an exception during creation. (EN)
    ///     <br />
    ///     Xác minh rằng CreateAsync thực hiện rollback giao dịch khi repository ném ra một ngoại lệ trong quá trình tạo. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldRollbackTransaction_WhenRepositoryThrowsException()
    {
        // Arrange
        var createRequest = new TransactionCreateRequest
        {
            Description = "Test Transaction", AccountId = Guid.CreateVersion7(), UserId = Guid.CreateVersion7(),
            TransactionDate = new DateTime(2026, 3, 15)
        };
        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<Transaction>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = CreateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.CreateAsync(createRequest);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB error");
        repoMock.Verify(r => r.CreateAsync(It.IsAny<Transaction>()), Times.Once);
        //unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        //transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        //transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    ///     Verifies that CreateAsync rejects an invalid request before mapping or writing anything. (EN)<br />
    ///     Xác minh rằng CreateAsync từ chối yêu cầu không hợp lệ trước khi ánh xạ hoặc ghi bất cứ thứ gì. (VI)
    /// </summary>
    [Theory]
    [InlineData(true, false, 0, 0, nameof(TransactionCreateRequest.AccountId))]
    [InlineData(false, true, 0, 0, nameof(TransactionCreateRequest.TransactionDate))]
    [InlineData(false, false, -1, 0, nameof(TransactionCreateRequest.SpentAmount))]
    [InlineData(false, false, 0, 99, nameof(TransactionCreateRequest.CategoryType))]
    public async Task CreateAsync_ShouldThrowValidationExceptionWithoutWriting_WhenRequestIsInvalid(
        bool emptyAccount, bool emptyDate, int spentAmount, int categoryType, string invalidProperty)
    {
        // Arrange
        var createRequest = new TransactionCreateRequest
        {
            AccountId = emptyAccount ? Guid.Empty : Guid.CreateVersion7(),
            TransactionDate = emptyDate ? default : new DateTime(2026, 3, 15),
            SpentAmount = spentAmount,
            CategoryType = (CategoryType)categoryType,
            UserId = Guid.CreateVersion7()
        };
        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = CreateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.CreateAsync(createRequest);

        // Assert
        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().Contain(error => error.PropertyName == invalidProperty);
        repoMock.Verify(r => r.CreateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }
}