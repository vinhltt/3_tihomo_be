using Bogus;
using CoreFinance.Application.DTOs.Account;
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

namespace CoreFinance.Application.Tests.AccountServiceTests;

/// <summary>
///     Contains test cases for the UpdateAsync method of AccountService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức UpdateAsync của AccountService. (VI)
/// </summary>
public partial class AccountServiceTests
{
    /// <summary>
    ///     Verifies that UpdateAsync returns the updated ViewModel when the update is successful with a valid request. (EN)
    ///     <br />
    ///     Xác minh rằng UpdateAsync trả về ViewModel đã cập nhật khi việc cập nhật thành công với yêu cầu hợp lệ. (VI)
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedViewModel()
    {
        // Arrange
        var accountId = Guid.CreateVersion7();
        var updateRequest = new Faker<AccountUpdateRequest>()
            .RuleFor(r => r.Id, accountId)
            .RuleFor(r => r.Name, f => f.Finance.AccountName())
            .RuleFor(r => r.Type, f => f.PickRandom<AccountType>().ToString())
            .RuleFor(r => r.Currency, f => f.Finance.Currency().Code)
            .Generate();

        var existingAccount = new Faker<Account>()
            .RuleFor(a => a.Id, accountId)
            .RuleFor(a => a.Name, "Old Account Name")
            .RuleFor(a => a.Type, AccountType.Bank)
            .RuleFor(a => a.Currency, "USD")
            .Generate();

        var accountAfterInternalMap = _mapper.Map(updateRequest, new Account());
        accountAfterInternalMap.Id = existingAccount.Id;

        var expectedViewModel = _mapper.Map<AccountViewModel>(accountAfterInternalMap);
        expectedViewModel.Name = updateRequest.Name;

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.GetByIdAsync(accountId))
            .ReturnsAsync(existingAccount);
        repoMock.Setup(r => r.UpdateAsync(It.Is<Account>(acc => acc.Id == accountId && acc.Name == updateRequest.Name)))
            .ReturnsAsync(1);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.UpdateAsync(accountId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedViewModel, options => options.ExcludingMissingMembers());

        repoMock.Verify(r => r.GetByIdAsync(accountId), Times.Once);
        repoMock.Verify(
            r => r.UpdateAsync(It.Is<Account>(acc => acc.Id == accountId && acc.Name == updateRequest.Name)),
            Times.Once);
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
        var accountId = Guid.CreateVersion7();
        var requestWithDifferentId = new Faker<AccountUpdateRequest>()
            .RuleFor(r => r.Id, Guid.CreateVersion7())
            .Generate();

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();

        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.UpdateAsync(accountId, requestWithDifferentId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();

        repoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Never);
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
        var accountId = Guid.CreateVersion7();
        var updateRequest = new Faker<AccountUpdateRequest>()
            .RuleFor(r => r.Id, accountId)
            .Generate();

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.GetByIdAsync(accountId))
            .ReturnsAsync((Account?)null);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.UpdateAsync(accountId, updateRequest);

        // Assert
        await act.Should().ThrowAsync<NullReferenceException>();

        repoMock.Verify(r => r.GetByIdAsync(accountId), Times.Once);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Never);
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
        var accountId = Guid.CreateVersion7();
        var updateRequest = new Faker<AccountUpdateRequest>()
            .RuleFor(r => r.Id, accountId)
            .RuleFor(r => r.Name, f => f.Finance.AccountName())
            .Generate();

        var existingAccount = new Faker<Account>()
            .RuleFor(a => a.Id, accountId)
            .Generate();

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.GetByIdAsync(accountId))
            .ReturnsAsync(existingAccount);
        repoMock.Setup(r => r.UpdateAsync(It.Is<Account>(acc => acc.Id == accountId && acc.Name == updateRequest.Name)))
            .ReturnsAsync(0);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.UpdateAsync(accountId, updateRequest);

        // Assert
        await act.Should().ThrowAsync<UpdateFailedException>();

        repoMock.Verify(r => r.GetByIdAsync(accountId), Times.Once);
        repoMock.Verify(
            r => r.UpdateAsync(It.Is<Account>(acc => acc.Id == accountId && acc.Name == updateRequest.Name)),
            Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    }
}