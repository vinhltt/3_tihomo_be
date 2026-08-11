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

public partial class AccountServiceTests
{
    /// <summary>
    ///     Verifies that CreateAsync returns a ViewModel when the creation is successful. (EN)<br />
    ///     Xác minh rằng CreateAsync trả về ViewModel khi việc tạo thành công. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldReturnViewModel_WhenCreationIsSuccessful()
    {
        // Arrange
        var createRequest = new AccountCreateRequest
        {
            Name = "New Savings Account",
            Type = AccountType.Bank,
            Currency = "USD",
            InitialBalance = 1000,
            UserId = Guid.CreateVersion7()
            // Add other required properties for AccountCreateRequest
        };

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<Account>()))
            .ReturnsAsync(1); // Simulate 1 record affected

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);


        var loggerMock = new Mock<ILogger<AccountService>>();
        // Use the real _mapper instance available from the partial class constructor
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        // Assert properties directly to verify mapping logic
        result.Name.Should().Be(createRequest.Name);
        result.Type.Should().Be(createRequest.Type);
        result.Currency.Should().Be(createRequest.Currency);
        result.InitialBalance.Should().Be(createRequest.InitialBalance); // Based on profile mapping

        // Verify that the repository method was called
        repoMock.Verify(r => r.CreateAsync(It.IsAny<Account>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    ///     Verifies that CreateAsync throws a CreateFailedException when the repository returns a zero affected count. (EN)
    ///     <br />
    ///     Xác minh rằng CreateAsync ném ra CreateFailedException khi repository trả về số bản ghi bị ảnh hưởng bằng không.
    ///     (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldThrowNullReferenceException_WhenRepositoryReturnsZeroAffectedCount()
    {
        // Arrange
        var createRequest = new AccountCreateRequest { Name = "Test Account", UserId = Guid.CreateVersion7() };

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<Account>()))
            .ReturnsAsync(0); // Simulate 0 records affected

        var transactionMock = new Mock<IDbContextTransaction>();
        // According to BaseService.cs, CommitAsync is called in the catch block when effectedCount <= 0
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();
        // Use the real _mapper instance
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.CreateAsync(createRequest);

        // Assert
        await act.Should().ThrowAsync<CreateFailedException>();

        repoMock.Verify(r => r.CreateAsync(It.IsAny<Account>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once); // BaseService commits in catch
        transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    ///     Verifies that CreateAsync rolls back the transaction when the repository throws an exception during creation. (EN)
    ///     <br />
    ///     Xác minh rằng CreateAsync thực hiện rollback giao dịch khi repository ném ra một ngoại lệ trong quá trình tạo. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldRollbackTransaction_WhenRepositoryThrowsException()
    {
        // Arrange
        var createRequest = new AccountCreateRequest { Name = "Test Account", UserId = Guid.CreateVersion7() };

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<Account>()))
            .ThrowsAsync(new InvalidOperationException("DB error")); // Simulate a DB error

        var transactionMock = new Mock<IDbContextTransaction>();
        // According to BaseService.cs, CommitAsync is called in the catch block when an exception occurs
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);


        var loggerMock = new Mock<ILogger<AccountService>>();
        // Use the real _mapper instance
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.CreateAsync(createRequest);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB error");

        repoMock.Verify(r => r.CreateAsync(It.IsAny<Account>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once); // BaseService commits in catch
        transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    }
}