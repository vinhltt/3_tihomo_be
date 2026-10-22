using System.Globalization;
using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.Enums;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Valid and excessive page indices return the expected stable slice and complete count.<br />
    ///     (VI) Chỉ số trang hợp lệ hoặc vượt giới hạn trả lát ổn định mong đợi và tổng đầy đủ.
    /// </summary>
    [Theory]
    [InlineData(99, 3, 5, 0)]
    [InlineData(3, 3, 5, 0)]
    [InlineData(2, 2, 3, 4)]
    [InlineData(1, 1, 1, 2)]
    public async Task GetPagingAsync_ShouldClampPageIndex_WhenPageExceedsLastPage(
        int pageIndex, int expectedPage, int firstId, int secondId)
    {
        // Arrange
        var fixture = CreateFixture(Expected(5), Expected(3), Expected(1), Expected(4), Expected(2));
        var request = new FilterBodyRequest { Pagination = new Pagination { PageIndex = pageIndex, PageSize = 2 } };
        var expectedIds = secondId > 0 ? new[] { Id(firstId), Id(secondId) } : new[] { Id(firstId) };

        // Act
        var result = await fixture.Service.GetPagingAsync(request);

        // Assert
        result!.Pagination.PageIndex.Should().Be(expectedPage);
        result.Pagination.PageSize.Should().Be(2);
        result.Pagination.PageCount.Should().Be(3);
        result.Pagination.TotalRow.Should().Be(5);
        result.Data!.Select(t => t.Id).Should().Equal(expectedIds);
    }

    /// <summary>
    ///     (EN) Clamp/count use the intersection of search, overdue status, date range, and template filters.<br />
    ///     (VI) Clamp/tổng dùng giao của tìm kiếm, quá hạn, khoảng ngày và bộ lọc mẫu.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldClampAfterEveryFilter_WhenSearchStatusDatesAndTemplateNarrowRows()
    {
        // Arrange
        var first = Expected(1, -3);
        var last = Expected(2, -2);
        var wrongTemplate = Expected(3, -2);
        wrongTemplate.RecurringTransactionTemplateId = Id(50);
        var wrongSearch = Expected(4, -2);
        wrongSearch.Description = "Unrelated";
        wrongSearch.Category = null;
        var fixture = CreateFixture(Expected(5, -4), wrongTemplate, Expected(6, -1), first, wrongSearch,
            Expected(7, -2, ExpectedTransactionStatus.Confirmed), last, Expected(8, 8));
        var request = new FilterBodyRequest
        {
            SearchValue = "ELECTRICITY",
            Pagination = new Pagination { PageIndex = 99, PageSize = 1 },
            Filter = new FilterRequest
            {
                LogicalOperator = FilterLogicalOperator.And,
                Details =
                [
                    new FilterDetailsRequest
                    {
                        AttributeName = nameof(ExpectedTransactionViewModel.DisplayStatus), Value = "4", FilterType = FilterType.Equal
                    },
                    new FilterDetailsRequest
                    {
                        AttributeName = nameof(ExpectedTransactionViewModel.ExpectedDate),
                        Value = Today.AddDays(-3).ToString("O", CultureInfo.InvariantCulture), FilterType = FilterType.GreaterThanOrEqual
                    },
                    new FilterDetailsRequest
                    {
                        AttributeName = nameof(ExpectedTransactionViewModel.ExpectedDate),
                        Value = Today.AddDays(-2).ToString("O", CultureInfo.InvariantCulture), FilterType = FilterType.LessThanOrEqual
                    },
                    new FilterDetailsRequest
                    {
                        AttributeName = nameof(ExpectedTransactionViewModel.RecurringTransactionTemplateId),
                        Value = TemplateId.ToString(), FilterType = FilterType.Equal
                    }
                ]
            }
        };

        // Act
        var result = await fixture.Service.GetPagingAsync(request);

        // Assert
        result!.Pagination.TotalRow.Should().Be(2);
        result.Pagination.PageCount.Should().Be(2);
        result.Pagination.PageIndex.Should().Be(2);
        result.Data!.Select(t => t.Id).Should().Equal(Id(2));
    }

    /// <summary>
    ///     (EN) Invalid lower bounds are clamped while preserving a valid positive page size.<br />
    ///     (VI) Biên dưới không hợp lệ được clamp, giữ kích thước trang dương hợp lệ.
    /// </summary>
    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(-5, -2, 1)]
    [InlineData(0, 2, 2)]
    public async Task GetPagingAsync_ShouldClampLowerBounds_WhenPageIndexOrSizeIsInvalid(
        int pageIndex, int pageSize, int expectedSize)
    {
        // Arrange
        var fixture = CreateFixture(Expected(3), Expected(1), Expected(2));
        var request = new FilterBodyRequest
        {
            Pagination = new Pagination { PageIndex = pageIndex, PageSize = pageSize }
        };

        // Act
        var result = await fixture.Service.GetPagingAsync(request);

        // Assert
        result!.Pagination.PageIndex.Should().Be(1);
        result.Pagination.PageSize.Should().Be(expectedSize);
        result.Pagination.TotalRow.Should().Be(3);
        result.Data!.Select(t => t.Id).Should().Equal(Enumerable.Range(1, expectedSize).Select(Id));
    }

    /// <summary>
    ///     (EN) Null pagination uses page one and the existing default page size of ten.<br />
    ///     (VI) Phân trang null dùng trang một và kích thước mặc định hiện hành là mười.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldUseDefaultPagination_WhenPaginationIsMissing()
    {
        // Arrange
        var fixture = CreateFixture(Enumerable.Range(1, 11).Reverse().Select(i => Expected(i)).ToArray());

        // Act
        var result = await fixture.Service.GetPagingAsync(new FilterBodyRequest { Pagination = null });

        // Assert
        result!.Pagination.PageIndex.Should().Be(1);
        result.Pagination.PageSize.Should().Be(10);
        result.Pagination.TotalRow.Should().Be(11);
        result.Pagination.PageCount.Should().Be(2);
        result.Data!.Select(t => t.Id).Should().Equal(Enumerable.Range(1, 10).Select(Id));
    }

    /// <summary>
    ///     (EN) Empty sources and unmatched searches return page-one empty results with no fabricated rows.<br />
    ///     (VI) Nguồn rỗng và tìm kiếm không khớp trả kết quả rỗng trang một, không có bản ghi giả.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetPagingAsync_ShouldReturnEmptyPageOne_WhenNoRowsMatch(bool search)
    {
        // Arrange
        var fixture = CreateFixture(search ? [Expected(1), Expected(2)] : []);
        var request = new FilterBodyRequest
        {
            SearchValue = search ? "NoMatchingPeriod" : null,
            Pagination = new Pagination { PageIndex = 99, PageSize = 0 }
        };

        // Act
        var result = await fixture.Service.GetPagingAsync(request);

        // Assert
        result!.Data.Should().BeEmpty();
        result.Pagination.TotalRow.Should().Be(0);
        result.Pagination.PageCount.Should().Be(0);
        result.Pagination.PageIndex.Should().Be(1);
        result.Pagination.PageSize.Should().Be(1);
    }
}
