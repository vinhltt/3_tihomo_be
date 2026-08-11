using CoreFinance.Application.DTOs.Account;
using CoreFinance.Application.Services;
using CoreFinance.Application.Tests.Helpers;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.Enums;

namespace CoreFinance.Application.Tests.AccountServiceTests;

/// <summary>
///     Contains test cases for the GetPagingAsync method of the AccountService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức GetPagingAsync của AccountService. (VI)
/// </summary>
// Tests for the GetPagingAsync method of AccountService
public partial class AccountServiceTests
{
    // Helper method has been moved to TestHelpers.cs
    // private static IQueryable<Account> GenerateFakeAccounts(int count) { ... }

    /// <summary>
    ///     Verifies that GetPagingAsync returns a paged result correctly. (EN)<br />
    ///     Xác minh rằng GetPagingAsync trả về kết quả phân trang một cách chính xác. (VI)
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldReturnPagedResult()
    {
        // Arrange
        var accounts = TestHelpers.GenerateFakeAccounts(3).ToList();
        var pageSize = 2;
        var pageIndex = 1;
        var orderedAccounts = accounts.OrderBy(a => a.Name).ToList();
        // ReSharper disable once UselessBinaryOperation

        var accountsMock = accounts.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(accountsMock);
        repoMock.Setup(x => x.GetQueryableTable()).Returns(accountsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();

        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "", // Có thể để trống hoặc lấy ký tự chung, vì không filter cụ thể
            Pagination = new Pagination { PageIndex = pageIndex, PageSize = pageSize },
            Orders = new List<SortDescriptor> { new() { Field = "Name", Direction = SortDirection.Asc } }
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(pageSize); // PageSize = 2

        var expectedViewModels = orderedAccounts.Skip((pageIndex - 1)! * pageSize).Take(pageSize)
            .Select(a => _mapper.Map<AccountViewModel>(a)).ToList();

        result.Data.Should().BeEquivalentTo(expectedViewModels, options => options.WithStrictOrdering());

        result.Pagination.TotalRow.Should().Be(accounts.Count); // Tổng số bản ghi
        result.Pagination.PageSize.Should().Be(pageSize);
        result.Pagination.PageIndex.Should().Be(pageIndex);
    }

    /// <summary>
    ///     Verifies that GetPagingAsync filters results by search value. (EN)<br />
    ///     Xác minh rằng GetPagingAsync lọc kết quả theo giá trị tìm kiếm. (VI)
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldFilterBySearchValue()
    {
        // Arrange
        var accounts = TestHelpers.GenerateFakeAccounts(3).ToList();
        var expectedName = accounts[0].Name;
        var expectedAccount = accounts.First(a => a.Name == expectedName);

        var accountsMock = accounts.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(accountsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();

        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = expectedName // Dùng đúng tên vừa lấy
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        var expectedViewModel = _mapper.Map<AccountViewModel>(expectedAccount);
        result.Data.First().Should().BeEquivalentTo(expectedViewModel);
    }

    /// <summary>
    ///     Verifies that GetPagingAsync filters results by search value in a case-insensitive manner. (EN)<br />
    ///     Xác minh rằng GetPagingAsync lọc kết quả theo giá trị tìm kiếm mà không phân biệt chữ hoa chữ thường. (VI)
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldFilterBySearchValue_CaseInsensitive()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Name = "Test Account One" },
            new() { Name = "test account two" },
            new() { Name = "Another Account" }
        }.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(accounts);
        repoMock.Setup(x => x.GetQueryableTable()).Returns(accounts);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "test account"
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(2); // "Test Account One" and "test account two"

        var expectedViewModels = accounts.Where(a => a.Name.ToLower().Contains("test account"))
            .ToList()
            .Select(_mapper.Map<AccountViewModel>).ToList();

        result.Data.Should().BeEquivalentTo(expectedViewModels);
    }

    /// <summary>
    ///     Verifies that GetPagingAsync returns an empty result when the search value has no match. (EN)<br />
    ///     Xác minh rằng GetPagingAsync trả về kết quả rỗng khi giá trị tìm kiếm không có kết quả khớp. (VI)
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldReturnEmpty_WhenSearchValueHasNoMatch()
    {
        // Arrange
        var accountsData = TestHelpers.GenerateFakeAccounts(3).ToList(); // Generate some accounts
        var accountsMock = accountsData.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(accountsMock);
        repoMock.Setup(x => x.GetQueryableTable()).Returns(accountsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "NonExistentSearchTermWhichWillNotMatchAnyFakeAccountName"
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty(); // The service's Where clause should result in an empty list
        result.Pagination.TotalRow.Should().Be(0);
    }

    /// <summary>
    ///     Verifies that GetPagingAsync handles the case where the repository returns no data. (EN)<br />
    ///     Xác minh rằng GetPagingAsync xử lý trường hợp repository không trả về dữ liệu. (VI)
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldHandleRepositoryReturningNoData()
    {
        // Arrange
        var emptyAccounts = new List<Account>().AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(emptyAccounts);
        repoMock.Setup(x => x.GetQueryableTable()).Returns(emptyAccounts);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            Pagination = new Pagination { PageIndex = 1, PageSize = 10 }
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        result.Pagination.TotalRow.Should().Be(0);
        result.Pagination.PageIndex.Should().Be(1);
        result.Pagination.PageSize.Should().Be(10);
    }

    /// <summary>
    ///     Verifies that GetPagingAsync handles pagination edge cases correctly. (EN)<br />
    ///     Xác minh rằng GetPagingAsync xử lý đúng các trường hợp biên của phân trang. (VI)
    /// </summary>
    [Theory]
    [InlineData(1, 5, 3, 1, 3)] // Total items < PageSize
    [InlineData(2, 2, 5, 2, 2)] // Requesting page that has fewer items than PageSize
    public async Task GetPagingAsync_ShouldHandlePaginationEdgeCases(int pageIndex, int pageSize, int totalItems,
        int expectedPageIndex, int expectedDataCount)
    {
        // Arrange
        var accounts = TestHelpers.GenerateFakeAccounts(totalItems).ToList();
        var orderedAccounts = accounts.OrderBy(a => a.Name).ToList();

        var accountsMock = accounts.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(accountsMock);
        repoMock.Setup(x => x.GetQueryableTable()).Returns(accountsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            Pagination = new Pagination { PageIndex = pageIndex, PageSize = pageSize },
            Orders = new List<SortDescriptor> { new() { Field = "Name", Direction = SortDirection.Asc } }
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(expectedDataCount);
        result.Pagination.TotalRow.Should().Be(totalItems);
        result.Pagination.PageIndex.Should()
            .Be(expectedPageIndex); // Assuming ToPagingAsync caps PageIndex or handles it appropriately
        result.Pagination.PageSize.Should().Be(pageSize);

        if (expectedDataCount > 0)
        {
            var expectedViewModels = orderedAccounts.Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(a => _mapper.Map<AccountViewModel>(a)).ToList();

            result.Data.Should().BeEquivalentTo(expectedViewModels, options => options.WithStrictOrdering());
        }
    }
}