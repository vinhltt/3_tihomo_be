using System.Globalization;
using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Enums;
using CoreFinance.Domain.Enums;
using FluentAssertions;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Reminders include all overdue dates and all of day seven, excluding day eight and terminal periods.<br />
    ///     (VI) Nhắc hạn gồm mọi ngày quá hạn và trọn ngày thứ bảy, loại ngày thứ tám và kỳ kết thúc.
    /// </summary>
    [Fact]
    public async Task GetRemindersAsync_ShouldReturnStableInclusiveFeed_WhenPeriodsStraddleReminderBoundaries()
    {
        // Arrange
        var lastDay = Expected(5, 7);
        lastDay.ExpectedDate = lastDay.ExpectedDate.AddHours(23).AddMinutes(59).AddSeconds(59);
        var old = Expected(1, -100);
        var yesterday = Expected(2, -1);
        var fixture = CreateFixture(lastDay, Expected(4), Expected(8, 8), Expected(3), yesterday, old,
            Expected(9, -1, ExpectedTransactionStatus.Confirmed),
            Expected(10, -1, ExpectedTransactionStatus.Cancelled),
            Expected(11, -1, ExpectedTransactionStatus.Completed));

        // Act
        var result = await fixture.Service.GetRemindersAsync();

        // Assert
        result.Select(t => t.Id).Should().Equal(Id(1), Id(2), Id(3), Id(4), Id(5));
        result.Select(t => t.DaysUntilDue).Should().Equal(-100, -1, 0, 0, 7);
        result.Select(t => t.DisplayStatus).Should().Equal(4, 4, 0, 0, 0);
        result.Should().OnlyContain(t => t.Status == ExpectedTransactionStatus.Pending);
        old.Status.Should().Be(ExpectedTransactionStatus.Pending);
        yesterday.Status.Should().Be(ExpectedTransactionStatus.Pending);
        yesterday.ProcessedAt.Should().BeNull();
    }

    /// <summary>
    ///     (EN) The reminder feed is complete rather than being silently capped at a paging limit.<br />
    ///     (VI) Feed nhắc hạn đầy đủ, không bị cắt ngầm theo giới hạn phân trang.
    /// </summary>
    [Fact]
    public async Task GetRemindersAsync_ShouldReturnEveryMatchingPeriod_WhenFeedExceedsPagingLimits()
    {
        // Arrange
        var rows = Enumerable.Range(1, 125).Select(i => Expected(i, -i)).ToArray();
        var fixture = CreateFixture(rows);

        // Act
        var result = await fixture.Service.GetRemindersAsync();

        // Assert
        result.Should().HaveCount(125);
        result.Select(t => t.Id).Should().Equal(Enumerable.Range(1, 125).Reverse().Select(Id));
    }

    /// <summary>
    ///     (EN) Display status and reminder membership change at UTC+7 midnight, not UTC midnight.<br />
    ///     (VI) Nhãn quá hạn và thành viên feed đổi tại nửa đêm UTC+7, không phải nửa đêm UTC.
    /// </summary>
    [Theory]
    [InlineData("2026-10-08T16:59:59Z", 0, 0, false)]
    [InlineData("2026-10-08T17:00:00Z", 4, -1, true)]
    public async Task GetRemindersAsync_ShouldUseUtcPlusSevenMidnight_WhenBusinessDateChanges(
        string utcNow, int displayStatus, int daysUntilDue, bool includesNextBoundary)
    {
        // Arrange
        var dueOnUtcDate = Expected(1, -1);
        var nextBoundary = Expected(2, 7);
        var fixture = new ServiceFixture(_mapper, [nextBoundary, dueOnUtcDate],
            DateTimeOffset.Parse(utcNow, CultureInfo.InvariantCulture));

        // Act
        var result = await fixture.Service.GetRemindersAsync();

        // Assert
        result.Single(t => t.Id == Id(1)).DisplayStatus.Should().Be(displayStatus);
        result.Single(t => t.Id == Id(1)).DaysUntilDue.Should().Be(daysUntilDue);
        result.Any(t => t.Id == Id(2)).Should().Be(includesNextBoundary);
        dueOnUtcDate.Status.Should().Be(ExpectedTransactionStatus.Pending);
    }

    /// <summary>
    ///     (EN) A successful cancellation or payment removes a period from the consumer reminder feed.<br />
    ///     (VI) Hủy hoặc ghi nhận thành công loại kỳ khỏi feed nhắc hạn cho người dùng.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetRemindersAsync_ShouldRemoveProcessedPeriod_WhenCancelledOrPaid(bool payment)
    {
        // Arrange
        var expected = Expected(1, -1);
        var fixture = CreateFixture(expected);
        var before = await fixture.Service.GetRemindersAsync();

        // Act
        var action = payment
            ? await fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest())
            : await fixture.Service.CancelExpectedTransactionAsync(expected.Id, "No bill");
        var after = await fixture.Service.GetRemindersAsync();

        // Assert
        before.Select(t => t.Id).Should().Equal(Id(1));
        action.Outcome.Should().Be(ExpectedTransactionActionOutcome.Success);
        after.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Empty and nonmatching sources yield a real empty reminder feed.<br />
    ///     (VI) Nguồn rỗng hoặc không khớp trả feed nhắc hạn thực sự rỗng.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetRemindersAsync_ShouldReturnEmpty_WhenNoPendingPeriodIsDue(bool otherPeriods)
    {
        // Arrange
        var fixture = CreateFixture(otherPeriods
            ? [Expected(1, 8), Expected(2, status: ExpectedTransactionStatus.Confirmed),
                Expected(3, status: ExpectedTransactionStatus.Cancelled)]
            : []);

        // Act
        var result = await fixture.Service.GetRemindersAsync();

        // Assert
        result.Should().BeEmpty();
    }
}
