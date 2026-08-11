using CoreFinance.Application.DTOs.Transaction;
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

namespace CoreFinance.Application.Tests.TransactionServiceTests;

/// <summary>
///     Contains test cases for the GetPagingAsync method of TransactionService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức GetPagingAsync của TransactionService. (VI)
/// </summary>
public partial class TransactionServiceTests
{
    /// <summary>
    ///     (EN) Verifies that GetPagingAsync returns a paged result correctly.<br />
    ///     (VI) Xác minh rằng GetPagingAsync trả về kết quả phân trang một cách chính xác.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldReturnPagedResult()
    {
        // Arrange
        var transactions = TestHelpers.GenerateFakeTransactions(3).ToList();
        var pageSize = 2;
        var pageIndex = 1;
        var orderedTransactions = transactions.OrderBy(t => t.TransactionDate).ToList();

        var transactionsMock = transactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(transactionsMock);
        repoMock.Setup(x => x.GetQueryableTable()).Returns(transactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<TransactionService>>();

        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "", // Empty search value
            Pagination = new Pagination { PageIndex = pageIndex, PageSize = pageSize },
            Orders = new List<SortDescriptor> { new() { Field = "TransactionDate", Direction = SortDirection.Asc } }
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(pageSize); // PageSize = 2

        var expectedViewModels = orderedTransactions.Skip((pageIndex - 1)! * pageSize).Take(pageSize)
            .Select(t => _mapper.Map<TransactionViewModel>(t)).ToList();

        result.Data.Should().BeEquivalentTo(expectedViewModels, options => options.WithStrictOrdering());

        result.Pagination.TotalRow.Should().Be(transactions.Count); // Total records
        result.Pagination.PageSize.Should().Be(pageSize);
        result.Pagination.PageIndex.Should().Be(pageIndex);
    }

    /// <summary>
    ///     Verifies that GetPagingAsync filters results by description. (EN)<br />
    ///     Xác minh rằng GetPagingAsync lọc kết quả theo mô tả. (VI)
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldFilterByDescription()
    {
        // Arrange
        var transactions = TestHelpers.GenerateFakeTransactions(3).ToList();
        var expectedDescription = transactions[0].Description;
        var expectedTransaction = transactions.First(t => t.Description == expectedDescription);

        var transactionsMock = transactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(transactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<TransactionService>>();

        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = expectedDescription // Use the exact description
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        var expectedViewModel = _mapper.Map<TransactionViewModel>(expectedTransaction);
        result.Data.First().Should().BeEquivalentTo(expectedViewModel);
    }

    /// <summary>
    ///     Verifies that GetPagingAsync filters results by category summary. (EN)<br />
    ///     Xác minh rằng GetPagingAsync lọc kết quả theo tóm tắt danh mục. (VI)
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldFilterByCategorySummary()
    {
        // Arrange
        var transactions = TestHelpers.GenerateFakeTransactions(3).ToList();
        var expectedCategorySummary = transactions[0].CategorySummary;
        var expectedTransaction = transactions.First(t => t.CategorySummary == expectedCategorySummary);

        var transactionsMock = transactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(transactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<TransactionService>>();

        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = expectedCategorySummary // Use the exact category summary
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        var expectedViewModel = _mapper.Map<TransactionViewModel>(expectedTransaction);
        result.Data.First().Should().BeEquivalentTo(expectedViewModel);
    }

    /// <summary>
    ///     Verifies that GetPagingAsync filters results by search value in a case-insensitive manner, checking both
    ///     Description and CategorySummary fields. (EN)<br />
    ///     Xác minh rằng GetPagingAsync lọc kết quả theo giá trị tìm kiếm mà không phân biệt chữ hoa chữ thường, kiểm tra cả
    ///     hai trường Mô tả và Tóm tắt danh mục. (VI)
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldFilterBySearchValue_CaseInsensitive()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new() { Description = "Test Transaction One", CategorySummary = "Food" },
            new() { Description = "test transaction two", CategorySummary = "Entertainment" },
            new() { Description = "Another Transaction", CategorySummary = "TEST Category" }
        }.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(transactions);
        repoMock.Setup(x => x.GetQueryableTable()).Returns(transactions);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "test"
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(3); // All 3 transactions contain "test" (case insensitive)

        var expectedViewModels = transactions.Where(t =>
            (t.Description != null && t.Description.ToLower().Contains("test")) ||
            (t.CategorySummary != null && t.CategorySummary.ToLower().Contains("test"))
        ).ToList().Select(_mapper.Map<TransactionViewModel>).ToList();

        result.Data.Should().BeEquivalentTo(expectedViewModels);
    }

    /// <summary>
    ///     Verifies that GetPagingAsync returns an empty result when the search value has no match in Description or
    ///     CategorySummary fields. (EN)<br />
    ///     Xác minh rằng GetPagingAsync trả về kết quả rỗng khi giá trị tìm kiếm không khớp với bất kỳ trường Mô tả hoặc Tóm
    ///     tắt danh mục nào. (VI)
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldReturnEmpty_WhenSearchValueHasNoMatch()
    {
        // Arrange
        var transactionsData = TestHelpers.GenerateFakeTransactions(3).ToList();
        var transactionsMock = transactionsData.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(transactionsMock);
        repoMock.Setup(x => x.GetQueryableTable()).Returns(transactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "NonExistentSearchTermWhichWillNotMatchAnyFakeTransactionDescription"
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
        var emptyTransactions = new List<Transaction>().AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(emptyTransactions);
        repoMock.Setup(x => x.GetQueryableTable()).Returns(emptyTransactions);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

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
    ///     Verifies that GetPagingAsync handles pagination edge cases correctly, such as total items less than page size or
    ///     requesting a page with fewer items. (EN)<br />
    ///     Xác minh rằng GetPagingAsync xử lý đúng các trường hợp biên của phân trang, chẳng hạn như tổng số mục ít hơn kích
    ///     thước trang hoặc yêu cầu trang có ít mục hơn. (VI)
    /// </summary>
    [Theory]
    [InlineData(1, 5, 3, 1, 3)] // Total items < PageSize
    [InlineData(2, 2, 5, 2, 2)] // Requesting page that has fewer items than PageSize
    public async Task GetPagingAsync_ShouldHandlePaginationEdgeCases(int pageIndex, int pageSize, int totalItems,
        int expectedPageIndex, int expectedDataCount)
    {
        // Arrange
        var transactions = TestHelpers.GenerateFakeTransactions(totalItems).ToList();
        var orderedTransactions = transactions.OrderBy(t => t.TransactionDate).ToList();

        var transactionsMock = transactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(transactionsMock);
        repoMock.Setup(x => x.GetQueryableTable()).Returns(transactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "",
            Pagination = new Pagination { PageIndex = pageIndex, PageSize = pageSize },
            Orders = new List<SortDescriptor> { new() { Field = "TransactionDate", Direction = SortDirection.Asc } }
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(expectedDataCount);
        result.Pagination.PageIndex.Should().Be(expectedPageIndex);
        result.Pagination.TotalRow.Should().Be(totalItems);

        var expectedViewModels = orderedTransactions.Skip((pageIndex - 1) * pageSize).Take(pageSize)
            .Select(t => _mapper.Map<TransactionViewModel>(t)).ToList();

        result.Data.Should().BeEquivalentTo(expectedViewModels, options => options.WithStrictOrdering());
    }

    /// <summary>
    ///     Verifies that GetPagingAsync handles a null search value gracefully, returning all results if no other filters are
    ///     applied. (EN)<br />
    ///     Xác minh rằng GetPagingAsync xử lý giá trị tìm kiếm null một cách duyên dáng, trả về tất cả các kết quả nếu không
    ///     có bộ lọc nào khác được áp dụng. (VI)
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldHandleNullSearchValue()
    {
        // Arrange
        var transactions = TestHelpers.GenerateFakeTransactions(2).ToList();
        var transactionsMock = transactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(transactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = null, // Null search value
            Pagination = new Pagination { PageIndex = 1, PageSize = 10 }
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(2); // Should return all transactions when search is null
        result.Pagination.TotalRow.Should().Be(2);
    }

    /// <summary>
    ///     Verifies that GetPagingAsync handles an empty string search value gracefully, returning all results if no other
    ///     filters are applied. (EN)<br />
    ///     Xác minh rằng GetPagingAsync xử lý giá trị tìm kiếm là chuỗi rỗng một cách duyên dáng, trả về tất cả các kết quả
    ///     nếu không có bộ lọc nào khác được áp dụng. (VI)
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldHandleEmptyStringSearchValue()
    {
        // Arrange
        var transactions = TestHelpers.GenerateFakeTransactions(2).ToList();
        var transactionsMock = transactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<Transaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(transactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Transaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<TransactionService>>();
        var service = new TransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "", // Empty string search value
            Pagination = new Pagination { PageIndex = 1, PageSize = 10 }
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(2); // Should return all transactions when search is empty
        result.Pagination.TotalRow.Should().Be(2);
    }
}