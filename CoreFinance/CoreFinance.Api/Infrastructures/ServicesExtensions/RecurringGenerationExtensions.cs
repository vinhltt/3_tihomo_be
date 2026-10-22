using CoreFinance.Api.BackgroundServices;
using CoreFinance.Api.Configuration;

namespace CoreFinance.Api.Infrastructures.ServicesExtensions;

/// <summary>
///     (EN) Registers the recurring generation feature: validated settings and the background worker.<br />
///     (VI) Đăng ký tính năng sinh định kỳ: cấu hình đã kiểm tra và worker nền.
/// </summary>
public static class RecurringGenerationExtensions
{
    /// <summary>
    ///     (EN) Binds <see cref="RecurringGenerationSettings" />, validates RunAtLocalTime at startup (even when the job is
    ///     disabled) and adds <see cref="RecurringTransactionGenerationService" />.<br />
    ///     (VI) Liên kết <see cref="RecurringGenerationSettings" />, kiểm tra RunAtLocalTime khi khởi động (kể cả khi tắt
    ///     job) và thêm <see cref="RecurringTransactionGenerationService" />.
    /// </summary>
    /// <param name="services">(EN) The service collection.<br />(VI) Tập dịch vụ.</param>
    /// <param name="configuration">(EN) The host configuration holding the section.<br />(VI) Cấu hình host chứa section.</param>
    /// <returns>(EN) The same service collection.<br />(VI) Chính tập dịch vụ đó.</returns>
    public static IServiceCollection AddRecurringGeneration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RecurringGenerationSettings>()
            .Bind(configuration.GetSection(RecurringGenerationSettings.SectionName))
            .Validate(settings => settings.RunAtLocalTime >= TimeSpan.Zero &&
                                  settings.RunAtLocalTime < TimeSpan.FromDays(1),
                "RecurringGeneration:RunAtLocalTime must be in [00:00,24:00).")
            .ValidateOnStart();
        services.AddHostedService<RecurringTransactionGenerationService>();
        return services;
    }
}
