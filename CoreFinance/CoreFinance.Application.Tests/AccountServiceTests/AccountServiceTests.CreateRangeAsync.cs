using Bogus;
using CoreFinance.Application.DTOs.Account;
using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Constants;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Contracts.Exceptions;

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
        var service = CreateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

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
        var service = CreateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

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
        var service = CreateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

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
        var service = CreateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

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

    /// <summary>
    ///     Verifies that CreateAsync (list version) rejects the whole batch when only one item is invalid, before opening a
    ///     transaction or writing to the repository. (EN)<br />
    ///     Xác minh rằng CreateAsync (phiên bản danh sách) từ chối cả lô khi chỉ một phần tử không hợp lệ, trước khi mở giao
    ///     dịch hoặc ghi vào repository. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_List_ShouldThrowValidationExceptionWithoutWriting_WhenOneItemIsInvalid()
    {
        // Arrange
        var createRequests = new List<AccountCreateRequest>
        {
            new() { Name = "Valid Wallet", Type = AccountType.Wallet, Currency = "USD", UserId = Guid.CreateVersion7() },
            new() { Name = "", Type = AccountType.Bank, Currency = "USD", UserId = Guid.CreateVersion7() }
        };
        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = CreateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.CreateAsync(createRequests);

        // Assert
        (await act.Should().ThrowAsync<FluentValidation.ValidationException>())
            .Which.Errors.Should().Contain(error => error.PropertyName == BatchItemKey(1, nameof(AccountCreateRequest.Name)));
        repoMock.Verify(r => r.CreateAsync(It.IsAny<List<Account>>()), Times.Never);
        repoMock.Verify(r => r.CreateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    // FluentValidation 12.0.0 prefixes a collection rule built with RuleForEach(x => x) by the lambda parameter name,
    // so the key of the second item's Name is "x[1].Name" (observed, not assumed).
    private static string BatchItemKey(int index, string property) => $"x[{index}].{property}";

    private static AccountCreateRequest ValidAccountRequest() =>
        new() { Name = "Valid Wallet", Type = AccountType.Wallet, Currency = "USD", UserId = Guid.CreateVersion7() };

    private static AccountCreateRequest InvalidAccountRequest() =>
        new() { Name = "", Type = AccountType.Bank, Currency = "USD", UserId = Guid.CreateVersion7() };

    /// <summary>
    ///     Verifies that CreateAsync (list version) reports the errors of every invalid item in one ValidationException
    ///     with index-qualified keys, and the first error belongs to the first invalid item. (EN)<br />
    ///     Xác minh rằng CreateAsync (phiên bản danh sách) gom lỗi của mọi phần tử sai vào một ValidationException với key
    ///     có chỉ số, và lỗi đầu tiên thuộc phần tử sai đầu tiên. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_List_ShouldReportEveryInvalidItemWithIndexedKeys_WhenSeveralItemsAreInvalid()
    {
        // Arrange
        var createRequests = new List<AccountCreateRequest>
        {
            InvalidAccountRequest(), ValidAccountRequest(), InvalidAccountRequest()
        };
        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = CreateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.CreateAsync(createRequests);

        // Assert
        var failure = (await act.Should().ThrowAsync<FluentValidation.ValidationException>()).Which;
        failure.Errors.Select(error => error.PropertyName).Should().Equal(
            BatchItemKey(0, nameof(AccountCreateRequest.Name)),
            BatchItemKey(2, nameof(AccountCreateRequest.Name)));
        failure.Errors.First().PropertyName.Should().Be(BatchItemKey(0, nameof(AccountCreateRequest.Name)));
        repoMock.Verify(r => r.CreateAsync(It.IsAny<List<Account>>()), Times.Never);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    /// <summary>
    ///     Verifies that CreateAsync (list version) rejects a batch above the limit with exactly one limit error, even when
    ///     its items are invalid, and neither opens a transaction, writes nor saves. (EN)<br />
    ///     Xác minh rằng CreateAsync (phiên bản danh sách) từ chối lô vượt giới hạn chỉ với đúng một lỗi giới hạn, kể cả khi
    ///     các phần tử sai, và không mở giao dịch, không ghi, không lưu. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_List_ShouldReportOnlyBatchLimitWithoutWriting_WhenBatchExceedsMaxSize()
    {
        // Arrange
        var createRequests = Enumerable.Range(0, CoreFinanceMessages.MaxBatchSize + 1)
            .Select(_ => InvalidAccountRequest()).ToList();
        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = CreateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.CreateAsync(createRequests);

        // Assert
        var failure = (await act.Should().ThrowAsync<FluentValidation.ValidationException>()).Which;
        failure.Errors.Should().ContainSingle().Which.ErrorMessage.Should().Be(CoreFinanceMessages.BatchSizeExceeded);
        repoMock.Verify(r => r.CreateAsync(It.IsAny<List<Account>>()), Times.Never);
        repoMock.Verify(r => r.CreateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    ///     Verifies that CreateAsync (list version) accepts a valid batch of exactly the maximum size. (EN)<br />
    ///     Xác minh rằng CreateAsync (phiên bản danh sách) chấp nhận lô hợp lệ đúng bằng kích thước tối đa. (VI)
    /// </summary>
    [Fact]
    public async Task CreateAsync_List_ShouldCreateAllItems_WhenBatchHasExactlyMaxSize()
    {
        // Arrange
        var batchSize = CoreFinanceMessages.MaxBatchSize;
        var createRequests = Enumerable.Range(0, batchSize).Select(_ => ValidAccountRequest()).ToList();
        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.CreateAsync(It.IsAny<List<Account>>())).ReturnsAsync(batchSize);
        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = CreateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.CreateAsync(createRequests);

        // Assert
        result!.Should().HaveCount(batchSize);
        repoMock.Verify(r => r.CreateAsync(It.Is<List<Account>>(list => list.Count == batchSize)), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}