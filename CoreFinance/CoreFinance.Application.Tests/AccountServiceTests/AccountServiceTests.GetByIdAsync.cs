using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CoreFinance.Application.Tests.AccountServiceTests;

/// <summary>
///     Contains test cases for the GetByIdAsync method of AccountService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức GetByIdAsync của AccountService. (VI)
/// </summary>
public partial class AccountServiceTests
{
    /// <summary>
    ///     Verifies that GetByIdAsync returns the correct account ViewModel when the account exists. (EN)<br />
    ///     Xác minh rằng GetByIdAsync trả về đúng ViewModel của tài khoản khi tài khoản tồn tại. (VI)
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ShouldReturnAccount_WhenAccountExists()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<AccountService>>();
        var repositoryMock = new Mock<IBaseRepository<Account, Guid>>();

        var accountId = Guid.CreateVersion7();
        var account = new Account { Id = accountId, Name = "Test Account" };

        unitOfWorkMock.Setup(uow => uow.Repository<Account, Guid>()).Returns(repositoryMock.Object);
        repositoryMock.Setup(repo => repo.GetByIdNoTrackingAsync(accountId)).ReturnsAsync(account);

        var accountService = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await accountService.GetByIdAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(accountId);
        result.Name.Should().Be("Test Account");
    }

    /// <summary>
    ///     Verifies that GetByIdAsync returns null when the account does not exist in the repository. (EN)<br />
    ///     Xác minh rằng GetByIdAsync trả về null khi tài khoản không tồn tại trong repository. (VI)
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenAccountDoesNotExist()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<AccountService>>();
        var repositoryMock = new Mock<IBaseRepository<Account, Guid>>();

        var accountId = Guid.CreateVersion7();

        unitOfWorkMock.Setup(uow => uow.Repository<Account, Guid>()).Returns(repositoryMock.Object);
        repositoryMock.Setup(repo => repo.GetByIdAsync(accountId)).ReturnsAsync((Account?)null);

        var accountService = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await accountService.GetByIdAsync(accountId);

        // Assert
        result.Should().BeNull();
    }
}