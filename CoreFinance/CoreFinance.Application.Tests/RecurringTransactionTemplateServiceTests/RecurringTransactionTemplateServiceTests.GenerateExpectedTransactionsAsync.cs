using CoreFinance.Domain.Enums;
using FluentAssertions;
using Shared.Contracts.Exceptions;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

public partial class RecurringTransactionTemplateServiceTests
{
    /// <summary>
    ///     (EN) A monthly template starting on the 31st generates 12 periods on each month's last valid day, never drifting
    ///     to the 28th after February.<br />
    ///     (VI) Mẫu hằng tháng bắt đầu ngày 31 sinh 12 kỳ vào ngày hợp lệ cuối cùng mỗi tháng, không trôi về ngày 28 sau
    ///     tháng 2.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldAnchorMonthEnd_WhenStartDateIs31st()
    {
        // Arrange
        var harness = new GenerationHarness { Template = NewTemplate(D(2026, 1, 31), daysInAdvance: 30) };

        // Act
        await harness.Service(D(2026, 12, 1)).GenerateExpectedTransactionsAsync(harness.Template.Id);

        // Assert
        harness.CreatedDates.Should().Equal(D(2026, 1, 31), D(2026, 2, 28), D(2026, 3, 31), D(2026, 4, 30),
            D(2026, 5, 31), D(2026, 6, 30), D(2026, 7, 31), D(2026, 8, 31), D(2026, 9, 30), D(2026, 10, 31),
            D(2026, 11, 30), D(2026, 12, 31));
        harness.Template.NextExecutionDate.Should().Be(D(2027, 1, 31));
    }

    /// <summary>
    ///     (EN) Quarterly and semi-annual schedules from the 31st clamp per target month on the StartDate grid.<br />
    ///     (VI) Lịch quý và nửa năm từ ngày 31 được kẹp theo từng tháng đích trên lưới StartDate.
    /// </summary>
    [Theory]
    [InlineData(RecurrenceFrequency.Quarterly, new[] { "2026-01-31", "2026-04-30", "2026-07-31", "2026-10-31" })]
    [InlineData(RecurrenceFrequency.SemiAnnually, new[] { "2026-01-31", "2026-07-31" })]
    public async Task GenerateExpectedTransactionsAsync_ShouldClampMultiMonthSchedules_OnStartDateGrid(
        RecurrenceFrequency frequency, string[] expected)
    {
        // Arrange
        var harness = new GenerationHarness { Template = NewTemplate(D(2026, 1, 31), frequency, daysInAdvance: 30) };

        // Act
        await harness.Service(D(2026, 12, 1)).GenerateExpectedTransactionsAsync(harness.Template.Id);

        // Assert
        harness.CreatedDates.Should().Equal(expected.Select(DateTime.Parse));
        harness.Template.NextExecutionDate.Should().Be(D(2027, 1, 31));
    }

    /// <summary>
    ///     (EN) An annual Feb-29 schedule falls back to Feb 28 in common years, returns to Feb 29 in the next leap year, and
    ///     years of downtime are caught up without a cap.<br />
    ///     (VI) Lịch hằng năm ngày 29/2 lùi về 28/2 ở năm thường, trở lại 29/2 ở năm nhuận kế tiếp, và nhiều năm gián đoạn
    ///     được sinh bù không giới hạn.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldRecoverLeapDayAndCatchUpDowntime_WhenAnnual()
    {
        // Arrange
        var harness = new GenerationHarness
            { Template = NewTemplate(D(2024, 2, 29), RecurrenceFrequency.Annually, daysInAdvance: 1) };

        // Act
        await harness.Service(D(2028, 3, 1)).GenerateExpectedTransactionsAsync(harness.Template.Id);

        // Assert
        harness.CreatedDates.Should().Equal(D(2024, 2, 29), D(2025, 2, 28), D(2026, 2, 28), D(2027, 2, 28),
            D(2028, 2, 29));
        harness.Template.NextExecutionDate.Should().Be(D(2029, 2, 28));
    }

    /// <summary>
    ///     (EN) A cursor drifted by the old add-a-month logic realigns to the StartDate grid; the existing legacy period in
    ///     the same month is kept, not duplicated or rewritten.<br />
    ///     (VI) Cursor bị trôi do logic cộng tháng cũ được căn lại theo lưới StartDate; kỳ cũ cùng tháng được giữ, không
    ///     sinh trùng hay ghi lại.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldRealignDriftedCursor_AndKeepExistingSlot()
    {
        // Arrange
        var harness = new GenerationHarness
            { Template = NewTemplate(D(2026, 1, 31), cursor: D(2026, 3, 28)) };
        harness.ExistingDates.AddRange([D(2026, 1, 31), D(2026, 2, 28), D(2026, 3, 28).AddHours(20)]);

        // Act
        await harness.Service(D(2026, 5, 15)).GenerateExpectedTransactionsAsync(harness.Template.Id);

        // Assert
        harness.CreatedDates.Should().Equal(D(2026, 4, 30));
        harness.Template.NextExecutionDate.Should().Be(D(2026, 5, 31));
    }

    /// <summary>
    ///     (EN) EndDate is inclusive: a period on the EndDate is generated, nothing after it.<br />
    ///     (VI) EndDate được tính bao gồm: kỳ đúng EndDate được sinh, không sinh sau đó.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldIncludeEndDate_AndStopAfterIt()
    {
        // Arrange
        var template = NewTemplate(D(2026, 1, 1));
        template.EndDate = D(2026, 3, 1);
        var harness = new GenerationHarness { Template = template };

        // Act
        await harness.Service(D(2026, 6, 1)).GenerateExpectedTransactionsAsync(template.Id);

        // Assert
        harness.CreatedDates.Should().Equal(D(2026, 1, 1), D(2026, 2, 1), D(2026, 3, 1));
        template.NextExecutionDate.Should().Be(D(2026, 4, 1));
    }

    /// <summary>
    ///     (EN) Day-based schedules keep plain day arithmetic from the stored cursor, and a date that already has a period
    ///     (cancelled or soft-deleted included) is never generated again.<br />
    ///     (VI) Lịch theo ngày giữ phép cộng ngày từ cursor đã lưu, và ngày đã có kỳ (kể cả đã hủy hoặc xóa mềm) không bao
    ///     giờ được sinh lại.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldKeepDayArithmetic_AndSkipExistingDates_WhenWeekly()
    {
        // Arrange
        var harness = new GenerationHarness
            { Template = NewTemplate(D(2026, 1, 1), RecurrenceFrequency.Weekly, cursor: D(2026, 1, 5)) };
        harness.ExistingDates.AddRange([D(2026, 1, 6), D(2026, 1, 12).AddHours(23)]);

        // Act
        await harness.Service(D(2026, 1, 19)).GenerateExpectedTransactionsAsync(harness.Template.Id);

        // Assert
        harness.CreatedDates.Should().Equal(D(2026, 1, 5), D(2026, 1, 19));
        harness.Template.NextExecutionDate.Should().Be(D(2026, 1, 26));
    }

    /// <summary>
    ///     (EN) Custom frequency without an interval falls back to one day.<br />
    ///     (VI) Tần suất custom không có khoảng ngày dùng mặc định một ngày.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldUseOneDay_WhenCustomIntervalIsNull()
    {
        // Arrange
        var harness = new GenerationHarness { Template = NewTemplate(D(2026, 1, 1), RecurrenceFrequency.Custom) };

        // Act
        await harness.Service(D(2026, 1, 3)).GenerateExpectedTransactionsAsync(harness.Template.Id);

        // Assert
        harness.CreatedDates.Should().Equal(D(2026, 1, 1), D(2026, 1, 2), D(2026, 1, 3));
        harness.Template.NextExecutionDate.Should().Be(D(2026, 1, 4));
    }

    /// <summary>
    ///     (EN) A custom interval that cannot advance is rejected before inserting periods or advancing the cursor.<br />
    ///     (VI) Khoảng custom không thể tiến bị từ chối trước khi tạo kỳ hay dời cursor.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldRejectWithoutInserts_WhenCustomIntervalIsZero()
    {
        // Arrange
        var template = NewTemplate(D(2026, 1, 1), RecurrenceFrequency.Custom);
        template.CustomIntervalDays = 0;
        var harness = new GenerationHarness { Template = template };

        // Act
        var act = () => harness.Service(D(2026, 1, 3)).GenerateExpectedTransactionsAsync(template.Id);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        harness.Created.Should().BeEmpty();
        template.NextExecutionDate.Should().Be(D(2026, 1, 1));
    }

    /// <summary>
    ///     (EN) Inactive or non-auto templates generate nothing and keep their cursor.<br />
    ///     (VI) Mẫu không hoạt động hoặc không tự sinh không sinh gì và giữ nguyên cursor.
    /// </summary>
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task GenerateExpectedTransactionsAsync_ShouldGenerateNothing_WhenInactiveOrNotAuto(bool isActive,
        bool autoGenerate)
    {
        // Arrange
        var template = NewTemplate(D(2026, 1, 1));
        template.IsActive = isActive;
        template.AutoGenerate = autoGenerate;
        var harness = new GenerationHarness { Template = template };

        // Act
        await harness.Service(D(2026, 6, 1)).GenerateExpectedTransactionsAsync(template.Id);

        // Assert
        harness.Created.Should().BeEmpty();
        template.NextExecutionDate.Should().Be(D(2026, 1, 1));
    }

    /// <summary>
    ///     (EN) A template whose account is unavailable generates nothing; repository ownership is not proven here.<br />
    ///     (VI) Mẫu có tài khoản không được trả về không sinh kỳ; kiểm thử này không chứng minh owner ở repository.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldGenerateNothing_WhenAccountIsNotOwned()
    {
        // Arrange
        var harness = new GenerationHarness { Template = NewTemplate(D(2026, 1, 1)) };
        harness.OwnedAccounts.Clear();

        // Act
        var act = () => harness.Service(D(2026, 6, 1)).GenerateExpectedTransactionsAsync(harness.Template.Id);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
        harness.Created.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Cancellation propagates before a period is created or the cursor advances.<br />
    ///     (VI) Việc hủy lan truyền trước khi tạo kỳ hay dời cursor.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldRollbackAndRethrow_WhenCancelled()
    {
        // Arrange
        var harness = new GenerationHarness { Template = NewTemplate(D(2026, 1, 1), RecurrenceFrequency.Daily) };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = () => harness.Service(D(2026, 6, 1))
            .GenerateExpectedTransactionsAsync(harness.Template.Id, null, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        harness.Created.Should().BeEmpty();
        harness.Template.NextExecutionDate.Should().Be(D(2026, 1, 1));
    }

    /// <summary>
    ///     (EN) An explicit zero- or one-day window overrides the template's configured seven-day window inclusively.<br />
    ///     (VI) Cửa sổ truyền rõ ràng không hoặc một ngày ghi đè cửa sổ bảy ngày của mẫu và gồm cả biên cuối.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task GenerateExpectedTransactionsAsync_ShouldUseExplicitInclusiveWindow_WhenOverrideIsProvided(
        int daysInAdvance)
    {
        // Arrange
        var template = NewTemplate(Today, RecurrenceFrequency.Daily, daysInAdvance: 7);
        var harness = new GenerationHarness { Template = template };

        // Act
        await harness.Service().GenerateExpectedTransactionsAsync(template.Id, daysInAdvance);

        // Assert
        harness.CreatedDates.Should().Equal(Enumerable.Range(0, daysInAdvance + 1).Select(day => Today.AddDays(day)));
        template.NextExecutionDate.Should().Be(Today.AddDays(daysInAdvance + 1));
    }

    /// <summary>
    ///     (EN) An unavailable generation target cannot create periods.<br />
    ///     (VI) Mục tiêu sinh kỳ không được trả về không thể tạo kỳ.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldCreateNoPeriods_WhenTemplateIsMissing()
    {
        // Arrange
        var harness = new GenerationHarness();

        // Act
        await harness.Service().GenerateExpectedTransactionsAsync(Guid.CreateVersion7());

        // Assert
        harness.Created.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) An unbound template can generate its window without binding an account to any period.<br />
    ///     (VI) Mẫu chưa gắn tài khoản sinh được cửa sổ mà không gắn tài khoản vào các kỳ.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsAsync_ShouldKeepPeriodsUnbound_WhenTemplateHasNoAccount()
    {
        // Arrange
        var template = NewTemplate(Today, RecurrenceFrequency.Daily, daysInAdvance: 1);
        template.AccountId = null;
        var harness = new GenerationHarness { Template = template };
        harness.OwnedAccounts.Clear();

        // Act
        await harness.Service().GenerateExpectedTransactionsAsync(template.Id);

        // Assert
        harness.CreatedDates.Should().Equal(Today, Today.AddDays(1));
        harness.Created.Should().OnlyContain(period => period.AccountId == null);
        template.NextExecutionDate.Should().Be(Today.AddDays(2));
    }
}
