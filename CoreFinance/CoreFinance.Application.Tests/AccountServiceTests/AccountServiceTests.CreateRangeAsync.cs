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
///     Contains test cases for the CreateAsync method that handles a list of accounts in the AccountService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức CreateAsync xử lý danh sách tài khoản trong AccountService. (VI)
/// </summary>
public partial class AccountServiceTests
{
    /// <summary>
    ///     Verifies that CreateAsync (list version) returns ViewModels when the creation of multiple accounts is successful.
    ///     (EN)<br />
    ///     Xác minh rằng CreateAsync (phiên bản danh sách) trả về ViewModels khi việc tạo nhiều tài khoản thành công. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_List_ValidRequest_ReturnsViewModels()
    {
        // Arrange
        var numberOfAccounts = 5;
        var createRequests = new Faker<AccountCreateRequest>()
            .RuleFor(r => r.Name, f => f.Finance.AccountName())
            .RuleFor(r => r.Type, f => f.PickRandom<AccountType>())
            .RuleFor(r => r.Currency, f => f.Finance.Currency().Code)
            .RuleFor(r => r.UserId, Guid.CreateVersion7())
            .Generate(numberOfAccounts);

        var createdEntities = _mapper.Map<List<Account>>(createRequests);
        createdEntities.ForEach(e => e.Id = Guid.CreateVersion7());

        var expectedViewModels = _mapper.Map<IEnumerable<AccountViewModel>>(createdEntities);

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<List<Account>>()))
            .ReturnsAsync(numberOfAccounts);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CreateAsync(createRequests);

        // Assert
        var accountViewModels = result!.ToList();
        accountViewModels.Should().NotBeNullOrEmpty();
        accountViewModels.Should().HaveCount(numberOfAccounts);
        accountViewModels.Should().BeEquivalentTo(expectedViewModels,
            options => options.ExcludingMissingMembers().Excluding(e => e.Id));

        repoMock.Verify(r => r.CreateAsync(It.Is<List<Account>>(list => list.Count == numberOfAccounts)), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    ///     Verifies that CreateAsync (list version) returns an empty list when the request list is empty. (EN)<br />
    ///     Xác minh rằng CreateAsync (phiên bản danh sách) trả về danh sách rỗng khi danh sách yêu cầu rỗng. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_List_EmptyRequestList_ReturnsEmptyList()
    {
        // Arrange
        var createRequests = new List<AccountCreateRequest>();

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        // BeginTransactionAsync should not be called for empty list as per BaseService logic

        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CreateAsync(createRequests);

        // Assert
        var accountViewModels = result!.ToList();
        accountViewModels.Should().NotBeNull();
        accountViewModels.Should().BeEmpty();

        repoMock.Verify(r => r.CreateAsync(It.IsAny<List<Account>>()), Times.Never);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    /// <summary>
    ///     Verifies that CreateAsync (list version) throws a CreateFailedException when the repository returns zero affected
    ///     count. (EN)<br />
    ///     Xác minh rằng CreateAsync (phiên bản danh sách) ném ra CreateFailedException khi repository trả về số bản ghi bị
    ///     ảnh hưởng bằng không. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_List_RepositoryReturnsZeroAffected_ThrowsNullReferenceException()
    {
        // Arrange
        var numberOfAccounts = 5;
        var createRequests = new Faker<AccountCreateRequest>()
            .RuleFor(r => r.Name, f => f.Finance.AccountName())
            .RuleFor(r => r.Type, f => f.PickRandom<AccountType>())
            .RuleFor(r => r.Currency, f => f.Finance.Currency().Code)
            .RuleFor(r => r.UserId, Guid.CreateVersion7())
            .Generate(numberOfAccounts);
        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<List<Account>>()))
            .ReturnsAsync(0);

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);
        transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.CreateAsync(createRequests);

        // Assert
        await act.Should().ThrowAsync<CreateFailedException>();

        repoMock.Verify(r => r.CreateAsync(It.Is<List<Account>>(list => list.Count == numberOfAccounts)), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    ///     Verifies that CreateAsync (list version) propagates the exception when the repository throws an exception during
    ///     creation. (EN)<br />
    ///     Xác minh rằng CreateAsync (phiên bản danh sách) lan truyền ngoại lệ khi repository ném ra một ngoại lệ trong quá
    ///     trình tạo. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_List_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var numberOfAccounts = 5;
        var createRequests = new Faker<AccountCreateRequest>()
            .RuleFor(r => r.Name, f => f.Finance.AccountName())
            .RuleFor(r => r.Type, f => f.PickRandom<AccountType>())
            .RuleFor(r => r.Currency, f => f.Finance.Currency().Code)
            .RuleFor(r => r.UserId, Guid.CreateVersion7())
            .Generate(numberOfAccounts); var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<List<Account>>()))
            .ThrowsAsync(new InvalidOperationException("Simulated DB error"));

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);
        transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.CreateAsync(createRequests);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Simulated DB error");

        repoMock.Verify(r => r.CreateAsync(It.Is<List<Account>>(list => list.Count == numberOfAccounts)), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        transactionMock.Verify(t => t.DisposeAsync(), Times.Once);
    }
}