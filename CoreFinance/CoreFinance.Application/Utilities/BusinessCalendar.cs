using CoreFinance.Domain.Constants;
using CoreFinance.Domain.Enums;

namespace CoreFinance.Application.Utilities;

/// <summary>
///     (EN) Business calendar for UTC+7. "Today" is the nominal calendar date in UTC+7 taken from the injected clock;
///     dates are midnight values with <see cref="DateTimeKind.Unspecified" />, matching the stored
///     timestamp-without-time-zone columns.<br />
///     (VI) Lịch nghiệp vụ theo UTC+7. "Hôm nay" là ngày lịch danh nghĩa ở UTC+7 lấy từ đồng hồ được inject; các ngày là
///     nửa đêm với <see cref="DateTimeKind.Unspecified" />, khớp các cột timestamp without time zone.
/// </summary>
public class BusinessCalendar(TimeProvider timeProvider)
{
    /// <summary>
    ///     (EN) Business offset from UTC.<br />
    ///     (VI) Độ lệch nghiệp vụ so với UTC.
    /// </summary>
    public static readonly TimeSpan Offset = TimeSpan.FromHours(7);

    /// <summary>
    ///     (EN) Current calendar date in UTC+7.<br />
    ///     (VI) Ngày lịch hiện tại theo UTC+7.
    /// </summary>
    public DateTime Today => DateTime.SpecifyKind(timeProvider.GetUtcNow().ToOffset(Offset).Date,
        DateTimeKind.Unspecified);

    /// <summary>
    ///     (EN) Month step of a month-based frequency (1/3/6/12), or null for day-based frequencies.<br />
    ///     (VI) Bước tháng của tần suất theo tháng (1/3/6/12), hoặc null với tần suất theo ngày.
    /// </summary>
    public static int? MonthStep(RecurrenceFrequency frequency)
    {
        return frequency switch
        {
            RecurrenceFrequency.Monthly => 1,
            RecurrenceFrequency.Quarterly => 3,
            RecurrenceFrequency.SemiAnnually => 6,
            RecurrenceFrequency.Annually => 12,
            _ => null
        };
    }

    /// <summary>
    ///     (EN) First slot on the month grid anchored at <paramref name="startDate" /> that falls on or after
    ///     <paramref name="candidate" />. Grid months are StartDate's month plus multiples of the frequency step; the day is
    ///     min(StartDate.Day, days in that month), so a 31st clamps per month and never drifts. Returns null for day-based
    ///     frequencies, which keep plain day arithmetic.<br />
    ///     (VI) Slot đầu tiên trên lưới tháng neo theo <paramref name="startDate" /> rơi vào hoặc sau
    ///     <paramref name="candidate" />. Tháng trên lưới là tháng của StartDate cộng bội số bước; ngày là
    ///     min(StartDate.Day, số ngày của tháng đó), nên ngày 31 được kẹp theo từng tháng và không trôi. Trả null với tần
    ///     suất theo ngày.
    /// </summary>
    public static DateTime? MonthSlotOnOrAfter(DateTime startDate, DateTime candidate, RecurrenceFrequency frequency)
    {
        if (MonthStep(frequency) is not { } step) return null;

        var start = startDate.Date;
        var target = candidate.Date;
        if (target <= start) return start;

        var monthsSinceStart = (target.Year - start.Year) * 12 + target.Month - start.Month;
        var stepsSinceStart = monthsSinceStart / step;
        var slot = AnchorToMonth(start, stepsSinceStart * step);
        return slot >= target ? slot : AnchorToMonth(start, (stepsSinceStart + 1) * step);
    }

    /// <summary>
    ///     (EN) Day interval of a day-based frequency. Custom with no interval falls back to 1 day; a custom interval of 0 or
    ///     less is rejected because the schedule would never advance.<br />
    ///     (VI) Khoảng ngày của tần suất theo ngày. Custom không có khoảng dùng mặc định 1 ngày; khoảng custom từ 0 trở xuống
    ///     bị từ chối vì lịch sẽ không bao giờ tiến.
    /// </summary>
    public static int DayInterval(RecurrenceFrequency frequency, int? customIntervalDays)
    {
        return frequency switch
        {
            RecurrenceFrequency.Weekly => 7,
            RecurrenceFrequency.Biweekly => 14,
            RecurrenceFrequency.Custom when customIntervalDays is <= 0 =>
                throw new ArgumentOutOfRangeException(nameof(customIntervalDays),
                    CoreFinanceMessages.CustomIntervalMustBePositive),
            RecurrenceFrequency.Custom => customIntervalDays ?? 1,
            _ => 1
        };
    }

    /// <summary>
    ///     (EN) First slot on or after <paramref name="candidate" />. Month-based frequencies use the month grid anchored at
    ///     <paramref name="startDate" />, so a drifted legacy cursor realigns immediately. Day-based frequencies keep plain
    ///     day arithmetic from the cursor (never earlier than StartDate). An invalid custom interval is rejected here, before
    ///     any slot is produced.<br />
    ///     (VI) Slot đầu tiên từ <paramref name="candidate" /> trở đi. Tần suất theo tháng dùng lưới tháng neo theo
    ///     <paramref name="startDate" />, nên cursor cũ bị trôi được căn lại ngay. Tần suất theo ngày giữ phép cộng ngày từ
    ///     cursor (không sớm hơn StartDate). Khoảng custom không hợp lệ bị từ chối ngay tại đây, trước khi sinh slot nào.
    /// </summary>
    public static DateTime FirstSlotOnOrAfter(DateTime startDate, RecurrenceFrequency frequency, int? customIntervalDays,
        DateTime candidate)
    {
        if (MonthSlotOnOrAfter(startDate, candidate, frequency) is { } monthSlot) return monthSlot;

        DayInterval(frequency, customIntervalDays); // rejects a schedule that can never advance
        return Unspecified(candidate.Date < startDate.Date ? startDate.Date : candidate.Date);
    }

    /// <summary>
    ///     (EN) Slot after <paramref name="current" />: the next month-grid slot for month-based frequencies, otherwise
    ///     <paramref name="current" /> plus the day interval (day-based schedules keep plain day arithmetic).<br />
    ///     (VI) Slot sau <paramref name="current" />: slot kế tiếp trên lưới tháng cho tần suất theo tháng, ngược lại là
    ///     <paramref name="current" /> cộng khoảng ngày (lịch theo ngày giữ phép cộng ngày).
    /// </summary>
    public static DateTime NextSlot(DateTime startDate, RecurrenceFrequency frequency, int? customIntervalDays,
        DateTime current)
    {
        return MonthSlotOnOrAfter(startDate, current.Date.AddDays(1), frequency)
               ?? Unspecified(current.Date.AddDays(DayInterval(frequency, customIntervalDays)));
    }

    /// <summary>
    ///     (EN) Slot identity of a generated date: its month for month-based frequencies (any existing period in that month
    ///     occupies the slot), its exact date otherwise.<br />
    ///     (VI) Định danh slot của một ngày đã sinh: tháng của nó với tần suất theo tháng (mọi kỳ đã có trong tháng đó chiếm
    ///     slot), ngày chính xác với các tần suất còn lại.
    /// </summary>
    public static DateTime SlotKey(RecurrenceFrequency frequency, DateTime date)
    {
        return MonthStep(frequency) != null ? new DateTime(date.Year, date.Month, 1) : date.Date;
    }

    private static DateTime AnchorToMonth(DateTime start, int monthsAfterStart)
    {
        var month = new DateTime(start.Year, start.Month, 1).AddMonths(monthsAfterStart);
        var day = Math.Min(start.Day, DateTime.DaysInMonth(month.Year, month.Month));
        return Unspecified(new DateTime(month.Year, month.Month, day));
    }

    private static DateTime Unspecified(DateTime date) => DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
}
