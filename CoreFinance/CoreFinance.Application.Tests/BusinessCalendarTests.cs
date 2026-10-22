using System.Globalization;
using CoreFinance.Application.Utilities;
using CoreFinance.Domain.Enums;
using FluentAssertions;

namespace CoreFinance.Application.Tests;

/// <summary>
///     (EN) UTC+7 business date boundary and month-grid anchoring.<br />
///     (VI) Biên ngày nghiệp vụ UTC+7 và neo lưới tháng.
/// </summary>
public class BusinessCalendarTests
{
    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private static DateTime D(int year, int month, int day) => new(year, month, day);

    /// <summary>
    ///     (EN) Business dates change at UTC+7 midnight, including the year boundary.<br />
    ///     (VI) Ngày nghiệp vụ đổi tại nửa đêm UTC+7, kể cả biên năm.
    /// </summary>
    [Theory]
    [InlineData("2026-01-31T16:59:59Z", "2026-01-31")]
    [InlineData("2026-01-31T17:00:00Z", "2026-02-01")]
    [InlineData("2026-12-31T16:59:59Z", "2026-12-31")]
    [InlineData("2026-12-31T17:00:00Z", "2027-01-01")]
    public void Today_SwitchesDateAt1700Utc(string utcNow, string expectedDate)
    {
        // Arrange
        var calendar = new BusinessCalendar(new FixedTimeProvider(
            DateTimeOffset.Parse(utcNow, CultureInfo.InvariantCulture)));

        // Act
        var today = calendar.Today;

        // Assert
        today.Should().Be(DateTime.Parse(expectedDate, CultureInfo.InvariantCulture));
        today.Kind.Should().Be(DateTimeKind.Unspecified);
        today.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void MonthSlotOnOrAfter_Monthly31_ClampsPerMonthWithoutDrift()
    {
        var start = D(2026, 1, 31);
        var slots = new List<DateTime>();
        var candidate = start;
        for (var i = 0; i < 12; i++)
        {
            var slot = BusinessCalendar.MonthSlotOnOrAfter(start, candidate, RecurrenceFrequency.Monthly)!.Value;
            slots.Add(slot);
            candidate = slot.AddDays(1);
        }

        slots.Should().Equal(D(2026, 1, 31), D(2026, 2, 28), D(2026, 3, 31), D(2026, 4, 30), D(2026, 5, 31),
            D(2026, 6, 30), D(2026, 7, 31), D(2026, 8, 31), D(2026, 9, 30), D(2026, 10, 31), D(2026, 11, 30),
            D(2026, 12, 31));
    }

    /// <summary>
    ///     (EN) Candidates before, on, and after a slot retain the anchored month grid.<br />
    ///     (VI) Ngày ứng viên trước, đúng và sau một kỳ vẫn theo lưới tháng được neo.
    /// </summary>
    [Theory]
    [InlineData(RecurrenceFrequency.Quarterly, "2026-02-01", "2026-04-30")]
    [InlineData(RecurrenceFrequency.SemiAnnually, "2026-02-01", "2026-07-31")]
    [InlineData(RecurrenceFrequency.Annually, "2026-02-01", "2027-01-31")]
    [InlineData(RecurrenceFrequency.Monthly, "2026-02-28", "2026-02-28")]
    [InlineData(RecurrenceFrequency.Monthly, "2025-12-01", "2026-01-31")]
    [InlineData(RecurrenceFrequency.Monthly, "2026-01-31T22:00:00", "2026-01-31")]
    [InlineData(RecurrenceFrequency.Monthly, "2026-03-01", "2026-03-31")]
    public void MonthSlotOnOrAfter_FromJan31_ReturnsFirstGridSlotOnOrAfterCandidate(
        RecurrenceFrequency frequency, string candidate, string expected)
    {
        // Arrange
        var target = DateTime.Parse(candidate, CultureInfo.InvariantCulture);

        // Act
        var slot = BusinessCalendar.MonthSlotOnOrAfter(D(2026, 1, 31), target, frequency);

        // Assert
        slot.Should().Be(DateTime.Parse(expected, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void MonthSlotOnOrAfter_AnnualLeapDay_RecoversOnLeapYear()
    {
        var start = D(2024, 2, 29);

        BusinessCalendar.MonthSlotOnOrAfter(start, D(2024, 3, 1), RecurrenceFrequency.Annually)
            .Should().Be(D(2025, 2, 28));
        BusinessCalendar.MonthSlotOnOrAfter(start, D(2027, 3, 1), RecurrenceFrequency.Annually)
            .Should().Be(D(2028, 2, 29));
    }

    [Theory]
    [InlineData(RecurrenceFrequency.Daily)]
    [InlineData(RecurrenceFrequency.Weekly)]
    [InlineData(RecurrenceFrequency.Biweekly)]
    [InlineData(RecurrenceFrequency.Custom)]
    public void MonthSlotOnOrAfter_DayBasedFrequency_ReturnsNull(RecurrenceFrequency frequency)
    {
        BusinessCalendar.MonthSlotOnOrAfter(D(2026, 1, 31), D(2026, 3, 1), frequency).Should().BeNull();
    }

    /// <summary>
    ///     (EN) Nonpositive custom intervals cannot produce an advancing schedule.<br />
    ///     (VI) Khoảng custom không dương không thể tạo lịch tiến lên.
    /// </summary>
    [Fact]
    public void DayInterval_ShouldRejectNegativeInterval_WhenFrequencyIsCustom()
    {
        // Arrange
        var interval = -1;

        // Act
        var act = () => BusinessCalendar.DayInterval(RecurrenceFrequency.Custom, interval);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    ///     (EN) Positive custom intervals advance by the specified days and discard cursor time.<br />
    ///     (VI) Khoảng custom dương tiến đúng số ngày đã chọn và bỏ phần giờ của cursor.
    /// </summary>
    [Theory]
    [InlineData(1, 11)]
    [InlineData(5, 15)]
    public void NextSlot_ShouldAdvanceByCustomInterval_WhenIntervalIsPositive(int interval, int expectedDay)
    {
        // Arrange
        var start = D(2026, 3, 1);
        var current = D(2026, 3, 10).AddHours(23);

        // Act
        var next = BusinessCalendar.NextSlot(start, RecurrenceFrequency.Custom, interval, current);

        // Assert
        next.Should().Be(D(2026, 3, expectedDay));
        next.Kind.Should().Be(DateTimeKind.Unspecified);
        next.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    /// <summary>
    ///     (EN) Day-based candidates use plain dates but never precede StartDate.<br />
    ///     (VI) Ngày ứng viên theo ngày dùng ngày thuần nhưng không được trước StartDate.
    /// </summary>
    [Theory]
    [InlineData(9, 10)]
    [InlineData(10, 10)]
    [InlineData(11, 11)]
    public void FirstSlotOnOrAfter_ShouldRespectStartFloor_WhenFrequencyIsDayBased(int candidateDay, int expectedDay)
    {
        // Arrange
        var start = D(2026, 3, 10).AddHours(8);
        var candidate = DateTime.SpecifyKind(D(2026, 3, candidateDay).AddHours(23), DateTimeKind.Utc);

        // Act
        var first = BusinessCalendar.FirstSlotOnOrAfter(start, RecurrenceFrequency.Weekly, null, candidate);

        // Assert
        first.Should().Be(D(2026, 3, expectedDay));
        first.Kind.Should().Be(DateTimeKind.Unspecified);
        first.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    /// <summary>
    ///     (EN) A drifted month-based candidate realigns to the original month-end anchor.<br />
    ///     (VI) Ngày ứng viên theo tháng bị trôi được căn lại theo mốc cuối tháng ban đầu.
    /// </summary>
    [Fact]
    public void FirstSlotOnOrAfter_ShouldUseAnchoredGrid_WhenFrequencyIsMonthly()
    {
        // Arrange
        var start = D(2026, 1, 31);
        var candidate = D(2026, 3, 28).AddHours(20);

        // Act
        var first = BusinessCalendar.FirstSlotOnOrAfter(start, RecurrenceFrequency.Monthly, null, candidate);

        // Assert
        first.Should().Be(D(2026, 3, 31));
        first.Kind.Should().Be(DateTimeKind.Unspecified);
        first.TimeOfDay.Should().Be(TimeSpan.Zero);
    }
}
