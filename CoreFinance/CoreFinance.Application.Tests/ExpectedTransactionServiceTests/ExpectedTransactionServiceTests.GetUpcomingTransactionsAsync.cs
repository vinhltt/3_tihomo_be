using CoreFinance.Domain.Enums;
using FluentAssertions;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) The default upcoming window includes all of today and day 30 in UTC+7, ordered by date/ID.<br />
    ///     (VI) Cửa sổ sắp tới mặc định gồm trọn hôm nay và ngày 30 theo UTC+7, sắp theo ngày/ID.
    /// </summary>
    [Fact]
    public async Task GetUpcomingTransactionsAsync_ShouldUseInclusiveBusinessDates_WhenDefaultWindowIsRequested()
    {
        // Arrange
        var today = Expected(1);
        var tiedEarly = Expected(2, 15);
        var tiedLate = Expected(3, 15);
        var lastDay = Expected(4, 30);
        lastDay.ExpectedDate = lastDay.ExpectedDate.AddHours(23).AddMinutes(59);
        var fixture = CreateFixture(lastDay, tiedLate, Expected(5, -1), tiedEarly, Expected(6, 31), today,
            Expected(7, 5, ExpectedTransactionStatus.Confirmed),
            Expected(8, 5, ExpectedTransactionStatus.Cancelled),
            Expected(9, 5, ExpectedTransactionStatus.Completed));

        // Act
        var result = (await fixture.Service.GetUpcomingTransactionsAsync(OwnerId)).ToList();

        // Assert
        result.Select(t => t.Id).Should().Equal(Id(1), Id(2), Id(3), Id(4));
        result.Select(t => t.DaysUntilDue).Should().Equal(0, 15, 15, 30);
        result.Should().OnlyContain(t => t.Status == ExpectedTransactionStatus.Pending && t.DisplayStatus == 0);
    }

    /// <summary>
    ///     (EN) A custom day window includes its whole last date and never includes yesterday or the next date.<br />
    ///     (VI) Cửa sổ ngày tùy chỉnh gồm trọn ngày cuối, không gồm hôm qua hay ngày tiếp theo.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public async Task GetUpcomingTransactionsAsync_ShouldIncludeWholeLastDay_WhenCustomWindowIsRequested(int days)
    {
        // Arrange
        var lastDay = Expected(2, days);
        lastDay.ExpectedDate = lastDay.ExpectedDate.AddHours(23).AddMinutes(59).AddSeconds(59);
        var fixture = CreateFixture(Expected(4, days + 1), lastDay, Expected(3, -1), Expected(1));

        // Act
        var result = (await fixture.Service.GetUpcomingTransactionsAsync(OwnerId, days)).ToList();

        // Assert
        result.Select(t => t.Id).Should().Equal(Id(1), Id(2));
        result.Select(t => t.DaysUntilDue).Should().Equal(0, days);
    }

    /// <summary>
    ///     (EN) Empty or entirely out-of-window data returns an empty upcoming list.<br />
    ///     (VI) Nguồn rỗng hoặc hoàn toàn ngoài cửa sổ trả danh sách sắp tới rỗng.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetUpcomingTransactionsAsync_ShouldReturnEmpty_WhenNoUpcomingPeriodExists(bool outside)
    {
        // Arrange
        var fixture = CreateFixture(outside ? [Expected(1, -5), Expected(2, 50)] : []);

        // Act
        var result = await fixture.Service.GetUpcomingTransactionsAsync(OwnerId);

        // Assert
        result.Should().BeEmpty();
    }
}
