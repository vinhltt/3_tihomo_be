using System.Globalization;
using CoreFinance.Api.BackgroundServices;
using CoreFinance.Api.Configuration;
using CoreFinance.Api.Tests.Helpers;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CoreFinance.Api.Tests.Integration;

/// <summary>
///     (EN) Exact UTC+7 deadlines and configuration rejection at the actual Program startup boundary.<br />
///     (VI) Kiểm tra deadline UTC+7 chính xác và từ chối cấu hình tại ranh giới khởi động Program thực.
/// </summary>
[Trait("Category", "Characterization")]
public sealed class RecurringGenerationScheduleTests
{
    /// <summary>
    ///     (EN) A 00:05 local schedule uses today's slot only before it; equality advances to tomorrow.<br />
    ///     (VI) Lịch 00:05 địa phương chỉ dùng mốc hôm nay trước giờ chạy; bằng giờ chạy phải chuyển sang ngày mai.
    /// </summary>
    [Theory]
    [InlineData("2026-10-13T17:04:59.9999999+00:00", "2026-10-13T17:05:00+00:00")]
    [InlineData("2026-10-13T17:05:00+00:00", "2026-10-14T17:05:00+00:00")]
    [InlineData("2026-10-13T17:05:00.0000001+00:00", "2026-10-14T17:05:00+00:00")]
    public void NextRunUtc_ShouldReturnExactDeadline_WhenBeforeEqualOrAfterConfigured0005(
        string nowUtc, string expectedUtc)
    {
        // Arrange
        var now = DateTimeOffset.Parse(nowUtc, CultureInfo.InvariantCulture);
        var expected = DateTimeOffset.Parse(expectedUtc, CultureInfo.InvariantCulture);
        var runAtLocalTime = TimeSpan.FromMinutes(5);

        // Act
        var nextRun = RecurringTransactionGenerationService.NextRunUtc(now, runAtLocalTime);

        // Assert
        AssertDeadline(nextRun, now, expected);
    }

    /// <summary>
    ///     (EN) Crossing UTC+7 midnight, not UTC midnight, preserves the next 00:05 local slot.<br />
    ///     (VI) Qua nửa đêm UTC+7, không phải nửa đêm UTC, vẫn giữ đúng mốc 00:05 địa phương kế tiếp.
    /// </summary>
    [Theory]
    [InlineData("2026-10-13T16:59:59.9999999+00:00")]
    [InlineData("2026-10-13T17:00:00+00:00")]
    [InlineData("2026-10-13T17:00:00.0000001+00:00")]
    public void NextRunUtc_ShouldReturnExactDeadline_WhenCrossingUtcPlusSevenMidnight(string nowUtc)
    {
        // Arrange
        var now = DateTimeOffset.Parse(nowUtc, CultureInfo.InvariantCulture);
        var expected = new DateTimeOffset(2026, 10, 13, 17, 5, 0, TimeSpan.Zero);
        var runAtLocalTime = TimeSpan.FromMinutes(5);

        // Act
        var nextRun = RecurringTransactionGenerationService.NextRunUtc(now, runAtLocalTime);

        // Assert
        AssertDeadline(nextRun, now, expected);
    }

    /// <summary>
    ///     (EN) Both valid daily-time endpoints retain tick precision and a strictly future deadline within 24 hours.<br />
    ///     (VI) Hai biên giờ chạy hợp lệ giữ độ chính xác tick và deadline luôn ở tương lai trong vòng 24 giờ.
    /// </summary>
    [Theory]
    [InlineData("00:00:00", "2026-10-13T16:59:59.9999999+00:00", "2026-10-13T17:00:00+00:00")]
    [InlineData("00:00:00", "2026-10-13T17:00:00+00:00", "2026-10-14T17:00:00+00:00")]
    [InlineData("00:00:00", "2026-10-13T17:00:00.0000001+00:00", "2026-10-14T17:00:00+00:00")]
    [InlineData("23:59:59.9999999", "2026-10-14T16:59:59.9999998+00:00", "2026-10-14T16:59:59.9999999+00:00")]
    [InlineData("23:59:59.9999999", "2026-10-14T16:59:59.9999999+00:00", "2026-10-15T16:59:59.9999999+00:00")]
    [InlineData("23:59:59.9999999", "2026-10-14T17:00:00+00:00", "2026-10-15T16:59:59.9999999+00:00")]
    public void NextRunUtc_ShouldReturnExactDeadline_WhenAtValidDailyTimeBoundaries(
        string configuredTime, string nowUtc, string expectedUtc)
    {
        // Arrange
        var now = DateTimeOffset.Parse(nowUtc, CultureInfo.InvariantCulture);
        var expected = DateTimeOffset.Parse(expectedUtc, CultureInfo.InvariantCulture);
        var runAtLocalTime = TimeSpan.Parse(configuredTime, CultureInfo.InvariantCulture);

        // Act
        var nextRun = RecurringTransactionGenerationService.NextRunUtc(now, runAtLocalTime);

        // Assert
        AssertDeadline(nextRun, now, expected);
    }

    /// <summary>
    ///     (EN) Actual Program startup rejects out-of-day times even when execution is disabled, with a useful diagnostic.<br />
    ///     (VI) Khởi động Program thực từ chối giờ ngoài miền một ngày ngay cả khi tắt job và báo lỗi cấu hình hữu ích.
    /// </summary>
    [Theory]
    [InlineData("-00:00:00.0000001", true)]
    [InlineData("-00:00:00.0000001", false)]
    [InlineData("-01:00:00", true)]
    [InlineData("-01:00:00", false)]
    [InlineData("1.00:00:00", true)]
    [InlineData("1.00:00:00", false)]
    [InlineData("2.00:00:00", true)]
    [InlineData("2.00:00:00", false)]
    public async Task ProgramStartup_ShouldRejectOutOfDayRunAtLocalTime_WhenEnabledOrDisabled(
        string configuredTime, bool enabled)
    {
        // Arrange
        await using var factory = CreateStartupFactory(configuredTime, enabled);

        // Act
        Action start = () =>
        {
            using var client = factory.CreateClient();
        };

        // Assert
        var exception = start.Should().Throw<OptionsValidationException>().Which;
        exception.OptionsType.Should().Be(typeof(RecurringGenerationSettings));
        var diagnostic = string.Join("; ", exception.Failures);
        diagnostic.Should().Contain(RecurringGenerationSettings.SectionName)
            .And.Contain(nameof(RecurringGenerationSettings.RunAtLocalTime));
        diagnostic.Should().MatchRegex(@"(?i)(?:00:00|(?<!\d)0(?!\d)|zero)",
            "the diagnostic must identify the lower daily-time bound without pinning its formatting");
        diagnostic.Should().MatchRegex(@"(?i)(?:(?<!\d)24(?!\d)|1\.00:00|\b(?:one|1)\s*day)",
            "the diagnostic must identify the one-day upper bound without pinning its wording");
    }

    /// <summary>
    ///     (EN) Actual Program startup accepts both valid endpoints and the existing 00:05 schedule without changing settings.<br />
    ///     (VI) Khởi động Program thực chấp nhận hai biên hợp lệ và lịch 00:05 hiện hữu mà không đổi cấu hình.
    /// </summary>
    [Theory]
    [InlineData("00:00:00", true)]
    [InlineData("00:00:00", false)]
    [InlineData("00:05:00", true)]
    [InlineData("00:05:00", false)]
    [InlineData("23:59:59.9999999", true)]
    [InlineData("23:59:59.9999999", false)]
    public async Task ProgramStartup_ShouldBootWithUnchangedValidRunAtLocalTime_WhenEnabledOrDisabled(
        string configuredTime, bool enabled)
    {
        // Arrange
        await using var factory = CreateStartupFactory(configuredTime, enabled);

        // Act
        using var client = factory.CreateClient();

        // Assert
        factory.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted.IsCancellationRequested
            .Should().BeTrue("the actual application must complete startup, not merely resolve options");
        var settings = factory.Services.GetRequiredService<IOptions<RecurringGenerationSettings>>().Value;
        settings.RunAtLocalTime.Should().Be(TimeSpan.Parse(configuredTime, CultureInfo.InvariantCulture));
        settings.Enabled.Should().Be(enabled);
    }

    private static void AssertDeadline(DateTimeOffset nextRun, DateTimeOffset now, DateTimeOffset expected)
    {
        nextRun.Should().Be(expected);
        nextRun.Offset.Should().Be(TimeSpan.Zero);
        nextRun.Should().BeAfter(now);
        (nextRun - now).Should().BeLessThanOrEqualTo(TimeSpan.FromDays(1));
    }

    private static WebApplicationFactory<Program> CreateStartupFactory(string configuredTime, bool enabled)
    {
        // Reuse the adjacent hosts' JWT/environment setup; Testing skips PostgreSQL registration and migrations.
        TestJwt.Configure();
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"{RecurringGenerationSettings.SectionName}:{nameof(RecurringGenerationSettings.RunAtLocalTime)}"] = configuredTime,
                    [$"{RecurringGenerationSettings.SectionName}:{nameof(RecurringGenerationSettings.Enabled)}"] = enabled.ToString(),
                    ["Serilog:MinimumLevel:Default"] = "Fatal",
                    ["Serilog:MinimumLevel:Override:Microsoft"] = "Fatal",
                    ["OtelSettings:TracesSamplerArg"] = "0"
                }));
            builder.ConfigureServices(services =>
            {
                // Isolate startup validation, not generation/persistence. Leave Program's options pipeline and
                // IStartupValidator intact: resolving options explicitly must not be what rejects invalid startup.
                foreach (var descriptor in services.Where(descriptor =>
                             descriptor.ServiceType == typeof(IHostedService) &&
                             descriptor.ImplementationType == typeof(RecurringTransactionGenerationService)).ToArray())
                    services.Remove(descriptor);
                services.RemoveMassTransitHostedService();
                // Actual worker execution, durable no-mutation and scope disposal belong to the PostgreSQL gates.
            });
        });
    }
}
