using CoreFinance.Api.Configuration;
using CoreFinance.Application.Interfaces;
using CoreFinance.Application.Utilities;
using CoreFinance.Domain.BaseRepositories.Systems;
using CoreFinance.Infrastructure;
using Microsoft.Extensions.Options;

namespace CoreFinance.Api.BackgroundServices;

/// <summary>
///     (EN) Generates expected transactions for every owner: one catch-up pass at startup, then daily at the configured
///     UTC+7 time. Each owner and each template run in their own DI scope and transaction, so one failure never blocks the
///     rest. Only expected transactions are created; actual transactions never are.<br />
///     (VI) Sinh giao dịch dự kiến cho mọi owner: một lượt sinh bù khi khởi động, sau đó hằng ngày theo giờ UTC+7 đã cấu
///     hình. Mỗi owner và mỗi mẫu chạy trong DI scope và transaction riêng, lỗi của một mẫu không chặn phần còn lại. Chỉ
///     tạo giao dịch dự kiến, không bao giờ tạo giao dịch thực.
/// </summary>
public class RecurringTransactionGenerationService(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    IOptions<RecurringGenerationSettings> settings,
    ILogger<RecurringTransactionGenerationService> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!settings.Value.Enabled)
        {
            logger.LogInformation("Recurring generation job is disabled");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            // The next run is computed from the clock before the pass starts, so a pass that crosses midnight still
            // runs the new day's schedule instead of skipping it.
            var nextRun = NextRunUtc(timeProvider.GetUtcNow(), settings.Value.RunAtLocalTime);
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                // An unhandled exception would stop the whole host (BackgroundServiceExceptionBehavior.StopHost).
                logger.LogError(ex, "Recurring generation pass failed; next attempt at {NextRunUtc}", nextRun);
            }

            var delay = nextRun - timeProvider.GetUtcNow();
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, timeProvider, stoppingToken);
        }
    }

    /// <summary>
    ///     (EN) Next occurrence of <paramref name="runAtLocalTime" /> (UTC+7) strictly after <paramref name="nowUtc" />.<br />
    ///     (VI) Lần xuất hiện kế tiếp của <paramref name="runAtLocalTime" /> (UTC+7) sau <paramref name="nowUtc" />.
    /// </summary>
    public static DateTimeOffset NextRunUtc(DateTimeOffset nowUtc, TimeSpan runAtLocalTime)
    {
        var localNow = nowUtc.ToOffset(BusinessCalendar.Offset);
        var candidate = new DateTimeOffset(localNow.Date + runAtLocalTime, BusinessCalendar.Offset);
        return (candidate > localNow ? candidate : candidate.AddDays(1)).ToUniversalTime();
    }

    /// <summary>
    ///     (EN) One pass over every owner with an active auto-generate template.<br />
    ///     (VI) Một lượt qua mọi owner có mẫu đang hoạt động và tự sinh.
    /// </summary>
    public async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Guid> ownerIds;
        await using (var catalogueScope = scopeFactory.CreateAsyncScope())
        {
            ownerIds = await catalogueScope.ServiceProvider.GetRequiredService<IRecurringGenerationSystemRepository>()
                .GetAutoGenerateOwnerIdsAsync(cancellationToken);
        }

        foreach (var ownerId in ownerIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<Guid> templateIds;
            try
            {
                await using var ownerScope = scopeFactory.CreateAsyncScope();
                ownerScope.ServiceProvider.GetRequiredService<CurrentUserContext>().InitializeBackgroundOwner(ownerId);
                templateIds = await ownerScope.ServiceProvider.GetRequiredService<IRecurringTransactionTemplateService>()
                    .GetActiveAutoGenerateTemplateIdsAsync(cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Recurring generation could not list templates for owner {OwnerId}", ownerId);
                continue;
            }

            foreach (var templateId in templateIds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await using var templateScope = scopeFactory.CreateAsyncScope();
                    templateScope.ServiceProvider.GetRequiredService<CurrentUserContext>().InitializeBackgroundOwner(ownerId);
                    await templateScope.ServiceProvider.GetRequiredService<IRecurringTransactionTemplateService>()
                        .GenerateExpectedTransactionsAsync(templateId, null, cancellationToken);
                }
                catch (Exception) when (!cancellationToken.IsCancellationRequested)
                {
                    // Logged with the template ID by the service; keep going with the next template.
                }
            }
        }
    }
}
