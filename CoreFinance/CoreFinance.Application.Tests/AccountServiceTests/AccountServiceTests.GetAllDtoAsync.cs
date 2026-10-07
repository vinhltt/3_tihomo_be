using CoreFinance.Application.DTOs.Account;
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
///     Contains test cases for the GetAllDtoAsync method of AccountService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức GetAllDtoAsync của AccountService. (VI)
/// </summary>
// Tests for the GetAllDtoAsync method of AccountService
public partial class AccountServiceTests
{
    /// <summary>
    ///     Verifies that GetAllDtoAsync returns all accounts correctly when accounts exist. (EN)<br />
    ///     Xác minh rằng GetAllDtoAsync trả về tất cả các tài khoản một cách chính xác khi có tài khoản tồn tại. (VI)
    /// </summary>
    [Fact]
    public async Task GetAllDtoAsync_ShouldReturnAllAccounts_WhenAccountsExist()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Account 1", Type = AccountType.Bank, Currency = "USD", InitialBalance = 100
            },
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Account 2", Type = AccountType.Cash, Currency = "EUR", InitialBalance = 50
            },
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Account 3", Type = AccountType.CreditCard, Currency = "VND",
                InitialBalance = 0
            }
        };

        var accountsMock = accounts.BuildMock();

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        // Assuming GetAllDtoAsync uses GetNoTrackingEntities()
        repoMock.Setup(r => r.GetNoTrackingEntities()).Returns(accountsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();

        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetAllDtoAsync();

        // Assert
        var accountViewModels = result!.ToList();
        accountViewModels.Should().NotBeNull();
        accountViewModels.Should().HaveCount(accounts.Count);

        // Map the expected accounts using the real mapper to compare with the result
        var expectedViewModels = accounts.Select(_mapper.Map<AccountViewModel>).ToList();
        accountViewModels.Should().BeEquivalentTo(expectedViewModels);

        // Verify that the repository method was called
        repoMock.Verify(r => r.GetNoTrackingEntities(), Times.Once);
    }

    /// <summary>
    ///     Verifies that GetAllDtoAsync returns an empty list when no accounts exist in the repository. (EN)<br />
    ///     Xác minh rằng GetAllDtoAsync trả về danh sách rỗng khi không có tài khoản nào tồn tại trong repository. (VI)
    /// </summary>
    [Fact]
    public async Task GetAllDtoAsync_ShouldReturnEmptyList_WhenNoAccountsExist()
    {
        // Arrange
        var emptyAccounts = new List<Account>().BuildMock();

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        // Assuming GetAllDtoAsync uses GetNoTrackingEntities()
        repoMock.Setup(r => r.GetNoTrackingEntities()).Returns(emptyAccounts);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();

        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.GetAllDtoAsync();

        // Assert
        var accountViewModels = result!.ToList();
        accountViewModels.Should().NotBeNull();
        accountViewModels.Should().BeEmpty();

        // Verify that the repository method was called
        repoMock.Verify(r => r.GetNoTrackingEntities(), Times.Once);
    }
}