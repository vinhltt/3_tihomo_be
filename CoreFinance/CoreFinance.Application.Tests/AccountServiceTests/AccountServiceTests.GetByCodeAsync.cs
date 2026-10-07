using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;

namespace CoreFinance.Application.Tests.AccountServiceTests;

/// <summary>
///     Contains test cases for the GetByCodeAsync method of AccountService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức GetByCodeAsync của AccountService. (VI)
///     Feature: tihomo-4 - N8N Google Sheets Integration
/// </summary>
public partial class AccountServiceTests
{
    /// <summary>
    ///     Verifies that GetByCodeAsync returns the correct account ViewModel when the code matches. (EN)<br />
    ///     Xác minh rằng GetByCodeAsync trả về đúng ViewModel của tài khoản khi mã khớp. (VI)
    /// </summary>
    [Fact]
    public async Task GetByCodeAsync_ShouldReturnAccount_WhenCodeMatches()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<AccountService>>();
        var repositoryMock = new Mock<IBaseRepository<Account, Guid>>();

        var testCode = "techcombank_debit";
        var accountId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var account = new Account
        {
            Id = accountId,
            Name = "Techcombank Debit Card",
            Code = testCode,
            Type = AccountType.DebitCard,
            Currency = "VND",
            InitialBalance = 0,
            CurrentBalance = 5000000,
            IsActive = true,
            UserId = userId
        };

        var accounts = new List<Account> { account }.BuildMock();

        unitOfWorkMock.Setup(uow => uow.Repository<Account, Guid>()).Returns(repositoryMock.Object);
        repositoryMock.Setup(repo => repo.GetNoTrackingEntities()).Returns(accounts);

        var accountService = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await accountService.GetByCodeAsync(testCode);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(accountId);
        result.Code.Should().Be(testCode);
        result.Name.Should().Be("Techcombank Debit Card");
        result.Type.Should().Be(AccountType.DebitCard);
    }

    /// <summary>
    ///     Verifies that GetByCodeAsync returns null when no account with the specified code exists. (EN)<br />
    ///     Xác minh rằng GetByCodeAsync trả về null khi không có tài khoản với mã được chỉ định. (VI)
    /// </summary>
    [Fact]
    public async Task GetByCodeAsync_ShouldReturnNull_WhenCodeNotFound()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<AccountService>>();
        var repositoryMock = new Mock<IBaseRepository<Account, Guid>>();

        var nonExistentCode = "non_existent_code";
        var accounts = new List<Account>().BuildMock();

        unitOfWorkMock.Setup(uow => uow.Repository<Account, Guid>()).Returns(repositoryMock.Object);
        repositoryMock.Setup(repo => repo.GetNoTrackingEntities()).Returns(accounts);

        var accountService = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await accountService.GetByCodeAsync(nonExistentCode);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    ///     Verifies that GetByCodeAsync is case-sensitive when matching codes. (EN)<br />
    ///     Xác minh rằng GetByCodeAsync phân biệt chữ hoa chữ thường khi khớp mã. (VI)
    /// </summary>
    [Fact]
    public async Task GetByCodeAsync_ShouldBeCaseSensitive()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<AccountService>>();
        var repositoryMock = new Mock<IBaseRepository<Account, Guid>>();

        var testCode = "techcombank_debit";
        var account = new Account
        {
            Id = Guid.CreateVersion7(),
            Name = "Test Account",
            Code = testCode,
            Type = AccountType.Bank,
            Currency = "VND",
            InitialBalance = 0,
            CurrentBalance = 0,
            IsActive = true,
            UserId = Guid.CreateVersion7()
        };

        var accounts = new List<Account> { account }.BuildMock();

        unitOfWorkMock.Setup(uow => uow.Repository<Account, Guid>()).Returns(repositoryMock.Object);
        repositoryMock.Setup(repo => repo.GetNoTrackingEntities()).Returns(accounts);

        var accountService = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act - Try uppercase code
        var result = await accountService.GetByCodeAsync("TECHCOMBANK_DEBIT");

        // Assert
        result.Should().BeNull("Code comparison should be case-sensitive");
    }

    /// <summary>
    ///     Verifies that GetByCodeAsync only returns active accounts. (EN)<br />
    ///     Xác minh rằng GetByCodeAsync chỉ trả về các tài khoản đang hoạt động. (VI)
    /// </summary>
    [Fact]
    public async Task GetByCodeAsync_ShouldOnlyReturnActiveAccounts()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<AccountService>>();
        var repositoryMock = new Mock<IBaseRepository<Account, Guid>>();

        var testCode = "inactive_account";
        var inactiveAccount = new Account
        {
            Id = Guid.CreateVersion7(),
            Name = "Inactive Account",
            Code = testCode,
            Type = AccountType.Bank,
            Currency = "VND",
            InitialBalance = 0,
            CurrentBalance = 0,
            IsActive = false, // Inactive account
            UserId = Guid.CreateVersion7()
        };

        var accounts = new List<Account> { inactiveAccount }.BuildMock();

        unitOfWorkMock.Setup(uow => uow.Repository<Account, Guid>()).Returns(repositoryMock.Object);
        repositoryMock.Setup(repo => repo.GetNoTrackingEntities()).Returns(accounts);

        var accountService = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await accountService.GetByCodeAsync(testCode);

        // Assert
        result.Should().BeNull("Inactive accounts should not be returned");
    }

    /// <summary>
    ///     Verifies that GetByCodeAsync returns the first matching account when multiple accounts have the same code (should not happen due to unique constraint). (EN)<br />
    ///     Xác minh rằng GetByCodeAsync trả về tài khoản khớp đầu tiên khi nhiều tài khoản có cùng mã (không nên xảy ra do ràng buộc duy nhất). (VI)
    /// </summary>
    [Fact]
    public async Task GetByCodeAsync_ShouldReturnFirstMatch_WhenMultipleAccountsWithSameCode()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<AccountService>>();
        var repositoryMock = new Mock<IBaseRepository<Account, Guid>>();

        var testCode = "duplicate_code";
        var account1 = new Account
        {
            Id = Guid.CreateVersion7(),
            Name = "First Account",
            Code = testCode,
            Type = AccountType.Bank,
            Currency = "VND",
            InitialBalance = 0,
            CurrentBalance = 1000,
            IsActive = true,
            UserId = Guid.CreateVersion7()
        };

        var account2 = new Account
        {
            Id = Guid.CreateVersion7(),
            Name = "Second Account",
            Code = testCode,
            Type = AccountType.Wallet,
            Currency = "USD",
            InitialBalance = 0,
            CurrentBalance = 500,
            IsActive = true,
            UserId = Guid.CreateVersion7()
        };

        var accounts = new List<Account> { account1, account2 }.BuildMock();

        unitOfWorkMock.Setup(uow => uow.Repository<Account, Guid>()).Returns(repositoryMock.Object);
        repositoryMock.Setup(repo => repo.GetNoTrackingEntities()).Returns(accounts);

        var accountService = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await accountService.GetByCodeAsync(testCode);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("First Account", "Should return the first matching account");
    }

    /// <summary>
    ///     Verifies that GetByCodeAsync handles null or whitespace codes gracefully. (EN)<br />
    ///     Xác minh rằng GetByCodeAsync xử lý các mã null hoặc khoảng trắng một cách nhẹ nhàng. (VI)
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetByCodeAsync_ShouldReturnNull_WhenCodeIsNullOrWhitespace(string? code)
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<AccountService>>();
        var repositoryMock = new Mock<IBaseRepository<Account, Guid>>();

        var accounts = new List<Account>().BuildMock();

        unitOfWorkMock.Setup(uow => uow.Repository<Account, Guid>()).Returns(repositoryMock.Object);
        repositoryMock.Setup(repo => repo.GetNoTrackingEntities()).Returns(accounts);

        var accountService = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await accountService.GetByCodeAsync(code!);

        // Assert
        result.Should().BeNull("Null or whitespace code should return null");
    }

    /// <summary>
    ///     Verifies that GetByCodeAsync works with various valid code formats. (EN)<br />
    ///     Xác minh rằng GetByCodeAsync hoạt động với các định dạng mã hợp lệ khác nhau. (VI)
    /// </summary>
    [Theory]
    [InlineData("techcombank_debit", "Techcombank Debit Card")]
    [InlineData("bidv_checking", "BIDV Checking Account")]
    [InlineData("momo_wallet", "MoMo E-Wallet")]
    [InlineData("cash_vnd", "Cash VND")]
    [InlineData("account_123", "Account 123")]
    public async Task GetByCodeAsync_ShouldSupportVariousValidCodeFormats(string code, string accountName)
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<AccountService>>();
        var repositoryMock = new Mock<IBaseRepository<Account, Guid>>();

        var account = new Account
        {
            Id = Guid.CreateVersion7(),
            Name = accountName,
            Code = code,
            Type = AccountType.Bank,
            Currency = "VND",
            InitialBalance = 0,
            CurrentBalance = 0,
            IsActive = true,
            UserId = Guid.CreateVersion7()
        };

        var accounts = new List<Account> { account }.BuildMock();

        unitOfWorkMock.Setup(uow => uow.Repository<Account, Guid>()).Returns(repositoryMock.Object);
        repositoryMock.Setup(repo => repo.GetNoTrackingEntities()).Returns(accounts);

        var accountService = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await accountService.GetByCodeAsync(code);

        // Assert
        result.Should().NotBeNull($"Valid code format '{code}' should be found");
        result!.Code.Should().Be(code);
        result.Name.Should().Be(accountName);
    }
}
