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

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

// Clamp behaviour of GetPagingAsync: ToPagingAsync throws past the last page, the service must not.
public partial class RecurringTransactionTemplateServiceTests
{
    private static RecurringTransactionTemplateService BuildPagingService(List<RecurringTransactionTemplate> rows)
    {
        var mock = rows.BuildMock();
        var repoMock = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repoMock.Setup(r => r.GetNoTrackingEntities()).Returns(mock);
        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repoMock.Object);
        return new RecurringTransactionTemplateService(TestHelpers.CreateMapper(), uow.Object, new Mock<ILogger<RecurringTransactionTemplateService>>().Object);
    }

    private static RecurringTransactionTemplate Make(string n) => new RecurringTransactionTemplate { Id = Guid.CreateVersion7(), Name = n };

    private static List<RecurringTransactionTemplate> Rows(int keep, int drop) =>
        Enumerable.Range(0, keep).Select(i => Make("Keep" + i))
            .Concat(Enumerable.Range(0, drop).Select(i => Make("Drop" + i))).ToList();

    [Theory]
    [InlineData(99, 5, 3)] // far past the end -> last page (5 rows / size 2 = 3 pages)
    [InlineData(3, 5, 3)] // exactly the last page
    [InlineData(1, 5, 1)] // normal
    public async Task GetPagingAsync_ShouldClampPageIndex_WhenPageIsPastTheEnd(int pageIndex, int total,
        int expectedPage)
    {
        var service = BuildPagingService(Rows(total, 0));
        var request = new FilterBodyRequest { Pagination = new Pagination { PageIndex = pageIndex, PageSize = 2 } };

        var result = await service.GetPagingAsync(request);

        result!.Pagination.PageIndex.Should().Be(expectedPage);
        result.Pagination.TotalRow.Should().Be(total);
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPagingAsync_ShouldClampAgainstFilteredCount_WhenFilterNarrowsResult()
    {
        // 2 matching rows of 6: unfiltered count would give 3 pages and let page 2 through to a throw.
        var service = BuildPagingService(Rows(2, 4));
        var request = new FilterBodyRequest
        {
            Pagination = new Pagination { PageIndex = 2, PageSize = 2 },
            Filter = new FilterRequest
            {
                Details = [new FilterDetailsRequest { AttributeName = "Name", Value = "Keep", FilterType = FilterType.Contains }]
            }
        };

        var result = await service.GetPagingAsync(request);

        result!.Pagination.PageIndex.Should().Be(1);
        result.Pagination.TotalRow.Should().Be(2);
    }

    [Theory]
    [InlineData(0, 0, 1)] // below 1 -> page 1 and default size, empty table must not throw
    [InlineData(-5, 3, 1)]
    public async Task GetPagingAsync_ShouldReturnPageOne_WhenPageIndexBelowOneOrTableEmpty(int pageIndex, int total,
        int expectedPage)
    {
        var service = BuildPagingService(Rows(total, 0));
        var request = new FilterBodyRequest { Pagination = new Pagination { PageIndex = pageIndex, PageSize = 0 } };

        var result = await service.GetPagingAsync(request);

        result!.Pagination.PageIndex.Should().Be(expectedPage);
        result.Pagination.PageSize.Should().Be(1); // pageSize floor
        result.Pagination.TotalRow.Should().Be(total);
    }
}
