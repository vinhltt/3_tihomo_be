using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.Enums;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Default date/ID ordering stays stable across page boundaries, including equal-date rows.<br />
    ///     (VI) Thứ tự ngày/ID mặc định ổn định qua các trang, kể cả bản ghi cùng ngày.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldReturnStableDefaultOrder_WhenDatesTieAcrossPages()
    {
        // Arrange
        var fixture = CreateFixture(Expected(5, 7), Expected(4), Expected(1, 7), Expected(3), Expected(2, -1));

        // Act
        var first = await fixture.Service.GetPagingAsync(new FilterBodyRequest
        {
            Pagination = new Pagination { PageIndex = 1, PageSize = 2 }
        });
        var second = await fixture.Service.GetPagingAsync(new FilterBodyRequest
        {
            Pagination = new Pagination { PageIndex = 2, PageSize = 2 }
        });
        var last = await fixture.Service.GetPagingAsync(new FilterBodyRequest
        {
            Pagination = new Pagination { PageIndex = 3, PageSize = 2 }
        });

        // Assert
        first!.Data!.Select(t => t.Id).Should().Equal(Id(2), Id(3));
        second!.Data!.Select(t => t.Id).Should().Equal(Id(4), Id(1));
        last!.Data!.Select(t => t.Id).Should().Equal(Id(5));
        first.Pagination.TotalRow.Should().Be(5);
        second.Pagination.TotalRow.Should().Be(5);
        last.Pagination.TotalRow.Should().Be(5);
        first.Pagination.PageCount.Should().Be(3);
        second.Pagination.PageCount.Should().Be(3);
        last.Pagination.PageCount.Should().Be(3);
        last.Pagination.PageIndex.Should().Be(3);
        last.Pagination.PageSize.Should().Be(2);
    }

    /// <summary>
    ///     (EN) Custom ordering receives an ID tie-breaker without replacing the requested primary order.<br />
    ///     (VI) Thứ tự tùy chọn có ID phụ ổn định mà không thay tiêu chí chính được yêu cầu.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldKeepCustomOrderAndStableTies_WhenCallerOmitsIdOrder()
    {
        // Arrange
        var fixture = CreateFixture(Expected(2, amount: 500m), Expected(3, amount: 200m),
            Expected(1, amount: 500m), Expected(4, amount: 700m));
        var request = new FilterBodyRequest
        {
            Orders = [new SortDescriptor { Field = nameof(ExpectedTransactionViewModel.ExpectedAmount), Direction = SortDirection.Desc }],
            Pagination = new Pagination { PageIndex = 2, PageSize = 2 }
        };

        // Act
        var result = await fixture.Service.GetPagingAsync(request);

        // Assert
        result!.Data!.Select(t => t.Id).Should().Equal(Id(2), Id(3));
        result.Pagination.TotalRow.Should().Be(4);
        result.Pagination.PageCount.Should().Be(2);
    }

    /// <summary>
    ///     (EN) An explicit descending ID tie-breaker is honored rather than overwritten by the default.<br />
    ///     (VI) ID phụ giảm dần được truyền rõ ràng được giữ, không bị mặc định ghi đè.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldHonorExplicitIdOrder_WhenCallerProvidesTieBreaker()
    {
        // Arrange
        var fixture = CreateFixture(Expected(2), Expected(1), Expected(3));
        var request = new FilterBodyRequest
        {
            Orders =
            [
                new SortDescriptor { Field = nameof(ExpectedTransactionViewModel.ExpectedDate), Direction = SortDirection.Asc },
                new SortDescriptor { Field = nameof(ExpectedTransactionViewModel.Id), Direction = SortDirection.Desc }
            ]
        };

        // Act
        var result = await fixture.Service.GetPagingAsync(request);

        // Assert
        result!.Data!.Select(t => t.Id).Should().Equal(Id(3), Id(2), Id(1));
    }

    /// <summary>
    ///     (EN) Search matches description, category, or owned template text case-insensitively and handles null text.<br />
    ///     (VI) Tìm kiếm mô tả, danh mục hoặc tên mẫu cùng chủ không phân biệt hoa thường và xử lý text null.
    /// </summary>
    [Theory]
    [InlineData("Description")]
    [InlineData("Category")]
    [InlineData("TemplateName")]
    public async Task GetPagingAsync_ShouldSearchConsumerText_WhenSearchIsCaseInsensitive(string field)
    {
        // Arrange
        var match = Expected(1);
        match.Description = field == "Description" ? "Monthly NEEDLE bill" : null;
        match.Category = field == "Category" ? "NEEDLE utilities" : null;
        if (field == "TemplateName")
            match.RecurringTransactionTemplate = new RecurringTransactionTemplate
            {
                Id = TemplateId, UserId = OwnerId, Name = "NEEDLE subscription"
            };
        var emptyText = Expected(2);
        emptyText.Description = null;
        emptyText.Category = null;
        var foreignNavigation = Expected(3);
        foreignNavigation.Description = null;
        foreignNavigation.Category = null;
        foreignNavigation.RecurringTransactionTemplate = new RecurringTransactionTemplate
        {
            Id = Id(50), UserId = OtherOwnerId, Name = "Private NEEDLE template"
        };
        var fixture = CreateFixture(emptyText, foreignNavigation, match);

        // Act
        var result = await fixture.Service.GetPagingAsync(new FilterBodyRequest { SearchValue = "needle" });

        // Assert
        result!.Data!.Select(t => t.Id).Should().Equal(Id(1));
        result.Pagination.TotalRow.Should().Be(1);
    }

    /// <summary>
    ///     (EN) Missing or empty search input leaves consumer-visible rows available.<br />
    ///     (VI) Đầu vào tìm kiếm thiếu hoặc rỗng giữ các bản ghi có thể hiển thị.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetPagingAsync_ShouldReturnAllRows_WhenSearchIsNotProvided(string? search)
    {
        // Arrange
        var fixture = CreateFixture(Expected(2, 7), Expected(1));

        // Act
        var result = await fixture.Service.GetPagingAsync(new FilterBodyRequest { SearchValue = search });

        // Assert
        result!.Data!.Select(t => t.Id).Should().Equal(Id(1), Id(2));
        result.Pagination.TotalRow.Should().Be(2);
    }

    /// <summary>
    ///     (EN) Derived display status separates overdue Pending periods from current Pending and all terminal states.<br />
    ///     (VI) Nhãn dẫn xuất tách Pending quá hạn khỏi Pending còn hạn và mọi trạng thái kết thúc.
    /// </summary>
    [Theory]
    [InlineData(0, 2, 3)]
    [InlineData(1, 4, 0)]
    [InlineData(2, 5, 0)]
    [InlineData(3, 6, 0)]
    [InlineData(4, 1, 0)]
    public async Task GetPagingAsync_ShouldFilterAndCountDisplayStatus_WhenLifecycleStatesAreMixed(
        int displayStatus, int firstId, int secondId)
    {
        // Arrange
        var overdue = Expected(1, -1);
        var fixture = CreateFixture(Expected(6, -1, ExpectedTransactionStatus.Completed), Expected(3, 7),
            Expected(5, -1, ExpectedTransactionStatus.Cancelled),
            Expected(4, -1, ExpectedTransactionStatus.Confirmed), Expected(2), overdue);
        var request = new FilterBodyRequest
        {
            Filter = new FilterRequest
            {
                Details = [new FilterDetailsRequest
                {
                    AttributeName = nameof(ExpectedTransactionViewModel.DisplayStatus), Value = displayStatus.ToString(),
                    FilterType = FilterType.Equal
                }]
            }
        };
        var expectedIds = secondId > 0 ? new[] { Id(firstId), Id(secondId) } : new[] { Id(firstId) };

        // Act
        var result = await fixture.Service.GetPagingAsync(request);

        // Assert
        result!.Data!.Select(t => t.Id).Should().Equal(expectedIds);
        result.Data.Should().OnlyContain(t => t.DisplayStatus == displayStatus);
        result.Pagination.TotalRow.Should().Be(expectedIds.Length);
        overdue.Status.Should().Be(ExpectedTransactionStatus.Pending);
        overdue.ProcessedAt.Should().BeNull();
    }

    /// <summary>
    ///     (EN) Display-status ordering acts on the complete filtered source before the page is selected.<br />
    ///     (VI) Sắp theo nhãn trạng thái tác động toàn nguồn đã lọc trước khi chọn trang.
    /// </summary>
    [Fact]
    public async Task GetPagingAsync_ShouldOrderDerivedStatusBeforePaging_WhenCallerSortsDisplayStatus()
    {
        // Arrange
        var fixture = CreateFixture(Expected(3), Expected(2, -1, ExpectedTransactionStatus.Completed), Expected(1, -1));
        var request = new FilterBodyRequest
        {
            Orders = [new SortDescriptor { Field = nameof(ExpectedTransactionViewModel.DisplayStatus), Direction = SortDirection.Desc }],
            Pagination = new Pagination { PageIndex = 1, PageSize = 1 }
        };

        // Act
        var result = await fixture.Service.GetPagingAsync(request);

        // Assert
        result!.Data!.Select(t => t.Id).Should().Equal(Id(1));
        result.Data!.Single().DisplayStatus.Should().Be(4);
        result.Pagination.TotalRow.Should().Be(3);
    }
}
