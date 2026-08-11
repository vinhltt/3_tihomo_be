using CoreFinance.Application.DTOs.Transaction;
using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Exceptions;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace CoreFinance.Application.Tests.TransactionServiceTests;

/// <summary>
///     Contains test cases for the UpdateAsync method of TransactionService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức UpdateAsync của TransactionService. (VI)
/// </summary>
public partial class TransactionServiceTests
{
    /// <summary>
    ///     Verifies that UpdateAsync returns the updated ViewModel when the transaction is successfully updated. (EN)<br />
    ///     Xác minh rằng UpdateAsync trả về ViewModel đã cập nhật khi giao dịch được cập nhật thành công. (VI)
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedViewModel_WhenTransactionIsUpdated()
    {
        // Arrange
        var transactionId = Guid.CreateVersion7();
        var updateRequest = new TransactionUpdateRequest
        {
            Id = transactionId,
            AccountId = Guid.CreateVersion7(),
            UserId = Guid.CreateVersion7(),
            Description = "Updated Description",
            RevenueAmount = 200,
            SpentAmount = 50
        };
        var transaction = new Transaction
        { Id = transactionId, Description = "Old Description", RevenueAmount = 100, SpentAmount = 0 };
        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r =>
                r.GetByIdAsync(transactionId,
                    It.IsAny<Expression<Func<Transaction, object>>[]>()))
            .ReturnsAsync(transaction);
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<Transaction>())).ReturnsAsync(1);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(Mock.Of<IDbContextTransaction>());
        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.UpdateAsync(transactionId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(transactionId);
        result.Description.Should().Be(updateRequest.Description);
        result.RevenueAmount.Should().Be(updateRequest.RevenueAmount);
    }

    /// <summary>
    ///     Verifies that UpdateAsync returns the updated ViewModel when the update is successful with a valid request. (EN)
    ///     <br />
    ///     Xác minh rằng UpdateAsync trả về ViewModel đã cập nhật khi việc cập nhật thành công với yêu cầu hợp lệ. (VI)
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedViewModel()
    {
        // Arrange
        var transactionId = Guid.CreateVersion7();
        var updateRequest = new TransactionUpdateRequest
        {
            Id = transactionId,
            AccountId = Guid.CreateVersion7(),
            UserId = Guid.CreateVersion7(),
            Description = "Updated Transaction",
            RevenueAmount = 200,
            SpentAmount = 50
        };
        var existingTransaction = new Transaction
        { Id = transactionId, Description = "Old Transaction", RevenueAmount = 100, SpentAmount = 0 };
        var transactionAfterMap = _mapper.Map(updateRequest, new Transaction());
        transactionAfterMap.Id = existingTransaction.Id;
        var expectedViewModel = _mapper.Map<TransactionViewModel>(transactionAfterMap);
        expectedViewModel.Description = updateRequest.Description;
        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.GetByIdAsync(transactionId)).ReturnsAsync(existingTransaction);
        repoMock.Setup(r =>
                r.UpdateAsync(It.Is<Transaction>(t =>
                    t.Id == transactionId && t.Description == updateRequest.Description)))
            .ReturnsAsync(1);
        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);
        // Act
        var result = await service.UpdateAsync(transactionId, updateRequest);
        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedViewModel, options => options.ExcludingMissingMembers());
        repoMock.Verify(r => r.GetByIdAsync(transactionId), Times.Once);
        repoMock.Verify(
            r => r.UpdateAsync(It.Is<Transaction>(t =>
                t.Id == transactionId && t.Description == updateRequest.Description)), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    ///     Verifies that UpdateAsync throws a KeyNotFoundException when the provided ID in the request mismatches the entity
    ///     ID. (EN)<br />
    ///     Xác minh rằng UpdateAsync ném ra KeyNotFoundException khi ID được cung cấp trong yêu cầu không khớp với ID thực
    ///     thể. (VI)
    /// </summary>
    [Fact]
    public async Task UpdateAsync_IdMismatch_ThrowsKeyNotFoundException()
    {
        // Arrange
        var transactionId = Guid.CreateVersion7();
        var requestWithDifferentId = new TransactionUpdateRequest { Id = Guid.CreateVersion7() };
        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);
        // Act
        Func<Task> act = async () => await service.UpdateAsync(transactionId, requestWithDifferentId);
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
        repoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Transaction>()), Times.Never);
    }

    /// <summary>
    ///     Verifies that UpdateAsync throws a NullReferenceException when the entity to update is not found in the repository.
    ///     (EN)<br />
    ///     Xác minh rằng UpdateAsync ném ra NullReferenceException khi không tìm thấy thực thể cần cập nhật trong repository.
    ///     (VI)
    /// </summary>
    [Fact]
    public async Task UpdateAsync_EntityNotFound_ThrowsNullReferenceException()
    {
        // Arrange
        var transactionId = Guid.CreateVersion7();
        var updateRequest = new TransactionUpdateRequest { Id = transactionId };
        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.GetByIdAsync(transactionId)).ReturnsAsync((Transaction?)null);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);
        // Act
        Func<Task> act = async () => await service.UpdateAsync(transactionId, updateRequest);
        // Assert
        await act.Should().ThrowAsync<NullReferenceException>();
        repoMock.Verify(r => r.GetByIdAsync(transactionId), Times.Once);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Transaction>()), Times.Never);
    }

    /// <summary>
    ///     Verifies that UpdateAsync throws an UpdateFailedException when the repository's update operation returns a zero
    ///     affected count. (EN)<br />
    ///     Xác minh rằng UpdateAsync ném ra UpdateFailedException khi thao tác cập nhật của repository trả về số bản ghi bị
    ///     ảnh hưởng bằng không. (VI)
    /// </summary>
    [Fact]
    public async Task UpdateAsync_UpdateFails_ThrowsUpdateFailedException()
    {
        // Arrange
        var transactionId = Guid.CreateVersion7();
        var updateRequest = new TransactionUpdateRequest { Id = transactionId, Description = "Fail Update" };
        var existingTransaction = new Transaction { Id = transactionId };
        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.GetByIdAsync(transactionId)).ReturnsAsync(existingTransaction);
        repoMock.Setup(r =>
                r.UpdateAsync(It.Is<Transaction>(t =>
                    t.Id == transactionId && t.Description == updateRequest.Description)))
            .ReturnsAsync(0);
        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);
        // Act
        Func<Task> act = async () => await service.UpdateAsync(transactionId, updateRequest);
        // Assert
        await act.Should().ThrowAsync<UpdateFailedException>();
        repoMock.Verify(r => r.GetByIdAsync(transactionId), Times.Once);
        repoMock.Verify(
            r => r.UpdateAsync(It.Is<Transaction>(t =>
                t.Id == transactionId && t.Description == updateRequest.Description)), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    }
}