namespace CoreFinance.Api.Configuration;

/// <summary>
///     (EN) Daily recurring generation job settings (section "RecurringGeneration").<br />
///     (VI) Cấu hình job sinh định kỳ hằng ngày (section "RecurringGeneration").
/// </summary>
public class RecurringGenerationSettings
{
    /// <summary>
    ///     Configuration section name.
    /// </summary>
    public const string SectionName = "RecurringGeneration";

    /// <summary>
    ///     (EN) Whether the job runs: a catch-up pass at startup, then daily.<br />
    ///     (VI) Job có chạy hay không: một lượt sinh bù khi khởi động, sau đó hằng ngày.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     (EN) Daily run time in UTC+7, in [00:00,24:00); validated at startup even when disabled.<br />
    ///     (VI) Giờ chạy hằng ngày theo UTC+7, trong [00:00,24:00); kiểm tra khi khởi động kể cả khi tắt job.
    /// </summary>
    public TimeSpan RunAtLocalTime { get; set; } = new(0, 5, 0);
}
