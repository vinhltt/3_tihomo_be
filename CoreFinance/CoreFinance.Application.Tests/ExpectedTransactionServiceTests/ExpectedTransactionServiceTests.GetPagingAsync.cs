using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

// Tests for the GetPagingAsync method of ExpectedTransactionService
public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Verifies that GetPagingAsync returns a paged result correctly.<br />
    ///     (VI) Xác minh rằng GetPagingAsync trả về kết quả phân trang một cách chính xác.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldReturnPagedResult()
    {
        // Arrange
        var expectedTransactions = new List<ExpectedTransaction>
        {
            new()
            {
                Id = Guid.CreateVersion7(), Description = "Transaction 1", Category = "Food",
                ExpectedDate = DateTime.UtcNow.AddDays(1)
            },
            new()
            {
                Id = Guid.CreateVersion7(), Description = "Transaction 2", Category = "Entertainment",
                ExpectedDate = DateTime.UtcNow.AddDays(2)
            },
            new()
            {
                Id = Guid.CreateVersion7(), Description = "Transaction 3", Category = "Transport",
                ExpectedDate = DateTime.UtcNow.AddDays(3)
            }
        };
        var pageSize = 2;
        var pageIndex = 1;

        var expectedTransactionsMock = expectedTransactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();

        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "", // Empty search value
            Pagination = new Pagination { PageIndex = pageIndex, PageSize = pageSize }
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(pageSize); // PageSize = 2
        result.Pagination.TotalRow.Should().Be(expectedTransactions.Count); // Total records
        result.Pagination.PageSize.Should().Be(pageSize);
        result.Pagination.PageIndex.Should().Be(pageIndex);
    }

    /// <summary>
    ///     (EN) Verifies that GetPagingAsync filters results by description.<br />
    ///     (VI) Xác minh rằng GetPagingAsync lọc kết quả theo mô tả.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldFilterByDescription()
    {
        // Arrange
        var expectedTransactions = new List<ExpectedTransaction>
        {
            new() { Id = Guid.CreateVersion7(), Description = "Unique Description", Category = "Food" },
            new() { Id = Guid.CreateVersion7(), Description = "Another Description", Category = "Entertainment" },
            new() { Id = Guid.CreateVersion7(), Description = "Different Description", Category = "Transport" }
        };

        var expectedTransactionsMock = expectedTransactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();

        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "Unique" // Should match only the first transaction
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        result.Data.First().Description.Should().Contain("Unique");
    }

    /// <summary>
    ///     (EN) Verifies that GetPagingAsync filters results by category.<br />
    ///     (VI) Xác minh rằng GetPagingAsync lọc kết quả theo danh mục.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldFilterByCategory()
    {
        // Arrange
        var expectedTransactions = new List<ExpectedTransaction>
        {
            new() { Id = Guid.CreateVersion7(), Description = "Transaction 1", Category = "UniqueCategory" },
            new() { Id = Guid.CreateVersion7(), Description = "Transaction 2", Category = "Entertainment" },
            new() { Id = Guid.CreateVersion7(), Description = "Transaction 3", Category = "Transport" }
        };

        var expectedTransactionsMock = expectedTransactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();

        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "UniqueCategory" // Should match only the first transaction
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        result.Data.First().Category.Should().Be("UniqueCategory");
    }

    /// <summary>
    ///     (EN) Verifies that GetPagingAsync filters results by search value in a case-insensitive manner.<br />
    ///     (VI) Xác minh rằng GetPagingAsync lọc kết quả theo giá trị tìm kiếm mà không phân biệt chữ hoa chữ thường.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldFilterBySearchValue_CaseInsensitive()
    {
        // Arrange
        var expectedTransactions = new List<ExpectedTransaction>
        {
            new() { Id = Guid.CreateVersion7(), Description = "Test Expected Transaction One", Category = "Food" },
            new() { Id = Guid.CreateVersion7(), Description = "test expected transaction two", Category = "Entertainment" },
            new() { Id = Guid.CreateVersion7(), Description = "Another Expected Transaction", Category = "TEST Category" }
        }.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactions);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "test"
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(3); // All 3 expected transactions contain "test" (case insensitive)
    }

    /// <summary>
    ///     (EN) Verifies that GetPagingAsync returns an empty result when the search value has no match.<br />
    ///     (VI) Xác minh rằng GetPagingAsync trả về kết quả rỗng khi giá trị tìm kiếm không có kết quả khớp.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldReturnEmpty_WhenSearchValueHasNoMatch()
    {
        // Arrange
        var expectedTransactions = new List<ExpectedTransaction>
        {
            new() { Id = Guid.CreateVersion7(), Description = "Transaction 1", Category = "Food" },
            new() { Id = Guid.CreateVersion7(), Description = "Transaction 2", Category = "Entertainment" }
        };
        var expectedTransactionsMock = expectedTransactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "NonExistentSearchTerm"
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
    ///     (EN) Verifies that GetPagingAsync handles the case where the repository returns no data.<br />
    ///     (VI) Xác minh rằng GetPagingAsync xử lý trường hợp repository không trả về dữ liệu.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldHandleRepositoryReturningNoData()
    {
        // Arrange
        var emptyExpectedTransactions = new List<ExpectedTransaction>().AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(emptyExpectedTransactions);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

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

    [Fact]
    public async Task GetPagingAsync_ShouldHandleNullSearchValue()
    {
        // Arrange
        var expectedTransactions = new List<ExpectedTransaction>
        {
            new() { Id = Guid.CreateVersion7(), Description = "Transaction 1", Category = "Food" },
            new() { Id = Guid.CreateVersion7(), Description = "Transaction 2", Category = "Entertainment" }
        };
        var expectedTransactionsMock = expectedTransactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

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
        result.Data.Count().Should().Be(2); // Should return all expected transactions when search is null
        result.Pagination.TotalRow.Should().Be(2);
    }

    [Fact]
    public async Task GetPagingAsync_ShouldHandleEmptyStringSearchValue()
    {
        // Arrange
        var expectedTransactions = new List<ExpectedTransaction>
        {
            new() { Id = Guid.CreateVersion7(), Description = "Transaction 1", Category = "Food" },
            new() { Id = Guid.CreateVersion7(), Description = "Transaction 2", Category = "Entertainment" }
        };
        var expectedTransactionsMock = expectedTransactions.AsQueryable().BuildMock();

        var repoMock = new Mock<IBaseRepository<ExpectedTransaction, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(expectedTransactionsMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<ExpectedTransactionService>>();
        var service = new ExpectedTransactionService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

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
        result.Data.Count().Should().Be(2); // Should return all expected transactions when search is empty
        result.Pagination.TotalRow.Should().Be(2);
    }
}