using System.Linq.Expressions;
using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using Moq;
using Shared.Contracts.Exceptions;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

// Tests for the UpdateAsync method of RecurringTransactionTemplateService
public partial class RecurringTransactionTemplateServiceTests
{
    /// <summary>
    ///     (EN) An amount-only edit keeps the schedule and cursor, and already generated periods are not regenerated.<br />
    ///     (VI) Chỉ sửa số tiền thì giữ nguyên lịch và cursor, các kỳ đã sinh không bị sinh lại.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldKeepScheduleAndCursor_WhenOnlyAmountChanges()
    {
        // Arrange
        var template = NewTemplate(D(2026, 1, 31), cursor: D(2026, 4, 30), daysInAdvance: 30);
        var harness = new GenerationHarness { Template = template };
        harness.ExistingDates.AddRange([D(2026, 1, 31), D(2026, 2, 28), D(2026, 3, 31)]);

        // Act
        var result = await harness.Service(D(2026, 3, 15))
            .UpdateAsync(template.Id, new RecurringTransactionTemplateUpdateRequest { Id = template.Id, Amount = 250m });

        // Assert
        result!.Amount.Should().Be(250m);
        template.Frequency.Should().Be(RecurrenceFrequency.Monthly);
        template.StartDate.Should().Be(D(2026, 1, 31));
        template.NextExecutionDate.Should().Be(D(2026, 4, 30));
        harness.Created.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Switching quarterly to monthly fills the missing months from today without reopening processed history;
    ///     a month that already has a period (even a future one) keeps it.<br />
    ///     (VI) Đổi từ quý sang tháng sinh bù các tháng thiếu từ hôm nay mà không mở lại lịch sử đã xử lý; tháng đã có kỳ
    ///     (kể cả kỳ tương lai) được giữ nguyên.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldFillMissingMonths_WhenQuarterlyBecomesMonthly()
    {
        // Arrange
        var template = NewTemplate(D(2026, 1, 31), RecurrenceFrequency.Quarterly, D(2026, 4, 30), 100);
        var harness = new GenerationHarness { Template = template };
        harness.ExistingDates.AddRange([D(2026, 1, 31), D(2026, 4, 30)]);

        // Act
        await harness.Service(D(2026, 2, 1)).UpdateAsync(template.Id,
            new RecurringTransactionTemplateUpdateRequest { Id = template.Id, Frequency = RecurrenceFrequency.Monthly });

        // Assert
        harness.CreatedDates.Should().Equal(D(2026, 2, 28), D(2026, 3, 31));
        template.NextExecutionDate.Should().Be(D(2026, 5, 31));
    }

    /// <summary>
    ///     (EN) A new StartDate moves the schedule to the new grid; a month already holding a period is not duplicated.<br />
    ///     (VI) StartDate mới dời lịch sang lưới mới; tháng đã có kỳ không bị sinh trùng.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldFollowNewGrid_WhenStartDateChanges()
    {
        // Arrange
        var template = NewTemplate(D(2026, 1, 31), cursor: D(2026, 4, 30), daysInAdvance: 30);
        var harness = new GenerationHarness { Template = template };
        harness.ExistingDates.AddRange([D(2026, 1, 31), D(2026, 2, 28), D(2026, 3, 31)]);

        // Act
        await harness.Service(D(2026, 3, 15)).UpdateAsync(template.Id,
            new RecurringTransactionTemplateUpdateRequest { Id = template.Id, StartDate = D(2026, 3, 10) });

        // Assert
        harness.CreatedDates.Should().Equal(D(2026, 4, 10));
        template.NextExecutionDate.Should().Be(D(2026, 5, 10));
    }

    /// <summary>
    ///     (EN) Disabling auto-generate saves the edit and generates nothing.<br />
    ///     (VI) Tắt tự sinh thì lưu chỉnh sửa và không sinh gì.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldNotGenerate_WhenAutoGenerateIsDisabled()
    {
        // Arrange
        var template = NewTemplate(D(2026, 1, 1), daysInAdvance: 365);
        var harness = new GenerationHarness { Template = template };

        // Act
        var result = await harness.Service().UpdateAsync(template.Id,
            new RecurringTransactionTemplateUpdateRequest { Id = template.Id, AutoGenerate = false, Name = "Water" });

        // Assert
        result!.AutoGenerate.Should().BeFalse();
        harness.Created.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) A missing template or a route/body ID mismatch fails and rolls back without writing.<br />
    ///     (VI) Mẫu không tồn tại hoặc ID route/body không khớp thì thất bại và rollback, không ghi gì.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldFailWithoutWriting_WhenTemplateMissingOrIdMismatch()
    {
        // Arrange
        var harness = new GenerationHarness();
        var id = Guid.CreateVersion7();

        // Act
        var missing = () => harness.Service().UpdateAsync(id, new RecurringTransactionTemplateUpdateRequest { Id = id });
        var mismatch = () => harness.Service()
            .UpdateAsync(id, new RecurringTransactionTemplateUpdateRequest { Id = Guid.CreateVersion7() });

        // Assert
        await missing.Should().ThrowAsync<EntityNotFoundException>();
        await mismatch.Should().ThrowAsync<KeyNotFoundException>();
        harness.Created.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Changing a custom interval starts at the earlier of the cursor and today, preserving occupied slots.<br />
    ///     (VI) Đổi khoảng custom bắt đầu tại ngày sớm hơn giữa cursor và hôm nay, giữ các kỳ đã có.
    /// </summary>
    [Theory]
    [InlineData(5, new[] { 5, 10, 15, 25 })]
    [InlineData(25, new[] { 15, 25 })]
    public async Task UpdateAsync_ShouldUseEarlierCursorAndNewInterval_WhenCustomIntervalChanges(
        int cursorDay, int[] expectedDays)
    {
        // Arrange
        var template = NewTemplate(D(2026, 3, 1), RecurrenceFrequency.Custom, D(2026, 3, cursorDay),
            daysInAdvance: 10);
        template.CustomIntervalDays = 2;
        var harness = new GenerationHarness { Template = template };
        harness.ExistingDates.AddRange([D(2026, 3, 3), D(2026, 3, 20)]);

        // Act
        await harness.Service().UpdateAsync(template.Id,
            new RecurringTransactionTemplateUpdateRequest { Id = template.Id, CustomIntervalDays = 5 });

        // Assert
        harness.CreatedDates.Should().Equal(expectedDays.Select(day => D(2026, 3, day)));
        template.NextExecutionDate.Should().Be(D(2026, 3, 30));
        template.CustomIntervalDays.Should().Be(5);
        harness.ExistingDates.Should().Equal(D(2026, 3, 3), D(2026, 3, 20));
    }

    /// <summary>
    ///     (EN) A zero affected update fails instead of generating periods or returning success.<br />
    ///     (VI) Cập nhật không ảnh hưởng bản ghi nào thất bại thay vì sinh kỳ hay trả thành công.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldThrowUpdateFailedException_WhenRepositoryReportsNoUpdate()
    {
        // Arrange
        var template = NewTemplate(Today, RecurrenceFrequency.Daily);
        var harness = new GenerationHarness { Template = template };
        harness.TemplateRepo
            .Setup(repository => repository.UpdateAsync(It.IsAny<RecurringTransactionTemplate>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var act = () => harness.Service().UpdateAsync(template.Id,
            new RecurringTransactionTemplateUpdateRequest { Id = template.Id, Amount = 250m });

        // Assert
        await act.Should().ThrowAsync<UpdateFailedException>();
        harness.Created.Should().BeEmpty();
        template.NextExecutionDate.Should().Be(Today);
    }

    /// <summary>
    ///     (EN) An invalid edit is rejected before any transaction, lock, lookup or write, and leaves the template untouched.<br />
    ///     (VI) Chỉnh sửa không hợp lệ bị từ chối trước mọi transaction, khóa, tra cứu hay ghi, và mẫu không bị đổi.
    /// </summary>
    [Theory]
    [InlineData(0, null, null, nameof(RecurringTransactionTemplateUpdateRequest.Amount))]
    [InlineData(null, 0, null, nameof(RecurringTransactionTemplateUpdateRequest.DaysInAdvance))]
    [InlineData(null, null, 99, nameof(RecurringTransactionTemplateUpdateRequest.Frequency))]
    public async Task UpdateAsync_ShouldThrowValidationExceptionWithoutWriting_WhenRequestIsInvalid(
        int? amount, int? daysInAdvance, int? frequency, string invalidProperty)
    {
        // Arrange
        var template = NewTemplate(D(2026, 1, 1), daysInAdvance: 30);
        var harness = new GenerationHarness { Template = template };
        var request = new RecurringTransactionTemplateUpdateRequest
        {
            Id = template.Id,
            Amount = amount,
            DaysInAdvance = daysInAdvance,
            Frequency = frequency.HasValue ? (RecurrenceFrequency)frequency.Value : null
        };

        // Act
        var act = () => harness.Service().UpdateAsync(template.Id, request);

        // Assert
        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().Contain(error => error.PropertyName == invalidProperty);
        AssertNothingStartedOrWritten(harness);
        template.Amount.Should().Be(100m);
        template.DaysInAdvance.Should().Be(30);
        template.Frequency.Should().Be(RecurrenceFrequency.Monthly);
    }

    /// <summary>
    ///     (EN) An empty-string name is an invalid edit at the service seam (only null means "keep the name").<br />
    ///     (VI) Tên là chuỗi rỗng là chỉnh sửa không hợp lệ ở tầng service (chỉ null mới nghĩa là "giữ tên").
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldRejectEmptyName_WhenNameIsEmptyString()
    {
        // Arrange
        var template = NewTemplate(D(2026, 1, 1));
        var harness = new GenerationHarness { Template = template };
        var request = new RecurringTransactionTemplateUpdateRequest { Id = template.Id, Name = "" };

        // Act
        var act = () => harness.Service().UpdateAsync(template.Id, request);

        // Assert
        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().Contain(error =>
                error.PropertyName == nameof(RecurringTransactionTemplateUpdateRequest.Name));
        AssertNothingStartedOrWritten(harness);
        template.Name.Should().Be("Electricity");
    }

    private static void AssertNothingStartedOrWritten(GenerationHarness harness)
    {
        harness.UnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        harness.UnitOfWork.Verify(u => u.AcquireOwnerResourceLockAsync(It.IsAny<string>(), It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
        harness.TemplateRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(),
            It.IsAny<Expression<Func<RecurringTransactionTemplate, object>>[]>()), Times.Never);
        harness.TemplateRepo.Verify(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>(),
            It.IsAny<CancellationToken>()), Times.Never);
        harness.Created.Should().BeEmpty();
    }
}
