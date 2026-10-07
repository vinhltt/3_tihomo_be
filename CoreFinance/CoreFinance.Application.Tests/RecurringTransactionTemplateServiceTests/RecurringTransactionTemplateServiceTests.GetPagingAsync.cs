using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

// Tests for the GetPagingAsync method of RecurringTransactionTemplateService
public partial class RecurringTransactionTemplateServiceTests
{
    /// <summary>
    ///     Verifies that GetPagingAsync returns a paged result correctly.<br />
    ///     (EN) Verifies that GetPagingAsync returns a paged result correctly.<br />
    ///     (VI) Xác minh rằng GetPagingAsync trả về kết quả phân trang một cách chính xác.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldReturnPagedResult()
    {
        // Arrange
        var templates = new List<RecurringTransactionTemplate>
        {
            new() { Id = Guid.CreateVersion7(), Name = "Template 1", Description = "Description 1", Category = "Food" },
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Template 2", Description = "Description 2", Category = "Entertainment"
            },
            new() { Id = Guid.CreateVersion7(), Name = "Template 3", Description = "Description 3", Category = "Transport" }
        };
        var pageSize = 2;
        var pageIndex = 1;

        var templatesMock = templates.BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();

        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

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
        result.Pagination.TotalRow.Should().Be(templates.Count); // Total records
        result.Pagination.PageSize.Should().Be(pageSize);
        result.Pagination.PageIndex.Should().Be(pageIndex);
    }

    /// <summary>
    ///     Verifies that GetPagingAsync filters results by name.<br />
    ///     (EN) Verifies that GetPagingAsync filters results by name.<br />
    ///     (VI) Xác minh rằng GetPagingAsync lọc kết quả theo tên.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldFilterByName()
    {
        // Arrange
        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Unique Template Name", Description = "Description 1", Category = "Food"
            },
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Another Template", Description = "Description 2",
                Category = "Entertainment"
            },
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Different Template", Description = "Description 3", Category = "Transport"
            }
        };

        var templatesMock = templates.BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();

        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "Unique" // Should match only the first template
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        result.Data.First().Name.Should().Contain("Unique");
    }

    /// <summary>
    ///     Verifies that GetPagingAsync filters results by description.<br />
    ///     (EN) Verifies that GetPagingAsync filters results by description.<br />
    ///     (VI) Xác minh rằng GetPagingAsync lọc kết quả theo mô tả.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldFilterByDescription()
    {
        // Arrange
        var templates = new List<RecurringTransactionTemplate>
        {
            new() { Id = Guid.CreateVersion7(), Name = "Template 1", Description = "Unique Description", Category = "Food" },
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Template 2", Description = "Another Description",
                Category = "Entertainment"
            },
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Template 3", Description = "Different Description", Category = "Transport"
            }
        };

        var templatesMock = templates.BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();

        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "Unique" // Should match only the first template
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
    ///     Verifies that GetPagingAsync filters results by category.<br />
    ///     (EN) Verifies that GetPagingAsync filters results by category.<br />
    ///     (VI) Xác minh rằng GetPagingAsync lọc kết quả theo danh mục.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldFilterByCategory()
    {
        // Arrange
        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Template 1", Description = "Description 1", Category = "UniqueCategory"
            },
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Template 2", Description = "Description 2", Category = "Entertainment"
            },
            new() { Id = Guid.CreateVersion7(), Name = "Template 3", Description = "Description 3", Category = "Transport" }
        };

        var templatesMock = templates.BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();

        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "UniqueCategory" // Should match only the first template
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
    ///     Verifies that GetPagingAsync filters results by search value in a case-insensitive manner.<br />
    ///     (EN) Verifies that GetPagingAsync filters results by search value in a case-insensitive manner.<br />
    ///     (VI) Xác minh rằng GetPagingAsync lọc kết quả theo giá trị tìm kiếm mà không phân biệt chữ hoa chữ thường.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldFilterBySearchValue_CaseInsensitive()
    {
        // Arrange
        var templates = new List<RecurringTransactionTemplate>
        {
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Test Template One", Description = "Test Description", Category = "Food"
            },
            new()
            {
                Id = Guid.CreateVersion7(), Name = "test template two", Description = "Another Description",
                Category = "Entertainment"
            },
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Another Template", Description = "TEST Description",
                Category = "TEST Category"
            }
        }.BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templates);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        var request = new FilterBodyRequest
        {
            SearchValue = "test"
        };

        // Act
        var result = await service.GetPagingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(3); // All 3 templates contain "test" (case insensitive)
    }

    /// <summary>
    ///     Verifies that GetPagingAsync returns an empty result when the search value has no match.<br />
    ///     (EN) Verifies that GetPagingAsync returns an empty result when the search value has no match.<br />
    ///     (VI) Xác minh rằng GetPagingAsync trả về kết quả rỗng khi giá trị tìm kiếm không có kết quả khớp.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldReturnEmpty_WhenSearchValueHasNoMatch()
    {
        // Arrange
        var templates = new List<RecurringTransactionTemplate>
        {
            new() { Id = Guid.CreateVersion7(), Name = "Template 1", Description = "Description 1", Category = "Food" },
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Template 2", Description = "Description 2", Category = "Entertainment"
            }
        };
        var templatesMock = templates.BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

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
    ///     Verifies that GetPagingAsync handles the case where the repository returns no data.<br />
    ///     (EN) Verifies that GetPagingAsync handles the case where the repository returns no data.<br />
    ///     (VI) Xác minh rằng GetPagingAsync xử lý trường hợp repository không trả về dữ liệu.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldHandleRepositoryReturningNoData()
    {
        // Arrange
        var emptyTemplates = new List<RecurringTransactionTemplate>().BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(emptyTemplates);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

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
    ///     Verifies that GetPagingAsync handles null search value correctly.<br />
    ///     (EN) Verifies that GetPagingAsync handles null search value correctly.<br />
    ///     (VI) Xác minh rằng GetPagingAsync xử lý đúng giá trị tìm kiếm null.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldHandleNullSearchValue()
    {
        // Arrange
        var templates = new List<RecurringTransactionTemplate>
        {
            new() { Id = Guid.CreateVersion7(), Name = "Template 1", Description = "Description 1", Category = "Food" },
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Template 2", Description = "Description 2", Category = "Entertainment"
            }
        };
        var templatesMock = templates.BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

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
        result.Data.Count().Should().Be(2); // Should return all templates when search is null
        result.Pagination.TotalRow.Should().Be(2);
    }

    /// <summary>
    ///     Verifies that GetPagingAsync handles empty string search value correctly.<br />
    ///     (EN) Verifies that GetPagingAsync handles empty string search value correctly.<br />
    ///     (VI) Xác minh rằng GetPagingAsync xử lý đúng giá trị tìm kiếm là chuỗi rỗng.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldHandleEmptyStringSearchValue()
    {
        // Arrange
        var templates = new List<RecurringTransactionTemplate>
        {
            new() { Id = Guid.CreateVersion7(), Name = "Template 1", Description = "Description 1", Category = "Food" },
            new()
            {
                Id = Guid.CreateVersion7(), Name = "Template 2", Description = "Description 2", Category = "Entertainment"
            }
        };
        var templatesMock = templates.BuildMock();

        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities())
            .Returns(templatesMock);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RecurringTransactionTemplateService>>();
        var service = new RecurringTransactionTemplateService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

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
        result.Data.Count().Should().Be(2); // Should return all templates when search is empty
        result.Pagination.TotalRow.Should().Be(2);
    }
}