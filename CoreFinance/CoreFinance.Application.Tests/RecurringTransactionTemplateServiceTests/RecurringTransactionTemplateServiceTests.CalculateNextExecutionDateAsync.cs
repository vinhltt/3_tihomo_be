using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Moq;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

public partial class RecurringTransactionTemplateServiceTests
{
    private static RecurringTransactionTemplateServiceTestsNextDate NextDateFixture(RecurringTransactionTemplate? template)
    {
        var repo = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(template);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(repo.Object);
        return new RecurringTransactionTemplateServiceTestsNextDate(CreateService(unitOfWork.Object));
    }

    private sealed record RecurringTransactionTemplateServiceTestsNextDate(
        Application.Services.RecurringTransactionTemplateService Service);

    /// <summary>
    ///     (EN) The next execution date is the slot after the cursor on the StartDate grid: month-based schedules clamp to
    ///     the start day per month, day-based ones add their interval (custom without interval adds one day).<br />
    ///     (VI) Ngày thực hiện tiếp theo là slot sau cursor trên lưới StartDate: lịch theo tháng kẹp về ngày bắt đầu theo
    ///     từng tháng, lịch theo ngày cộng khoảng của nó (custom không có khoảng thì cộng một ngày).
    /// </summary>
    [Theory]
    [InlineData(RecurrenceFrequency.Daily, null, "2026-01-01", "2026-01-10", "2026-01-11")]
    [InlineData(RecurrenceFrequency.Weekly, null, "2026-01-01", "2026-01-10", "2026-01-17")]
    [InlineData(RecurrenceFrequency.Biweekly, null, "2026-01-01", "2026-01-10", "2026-01-24")]
    [InlineData(RecurrenceFrequency.Custom, 10, "2026-01-01", "2026-01-10", "2026-01-20")]
    [InlineData(RecurrenceFrequency.Custom, null, "2026-01-01", "2026-01-10", "2026-01-11")]
    [InlineData(RecurrenceFrequency.Monthly, null, "2026-01-31", "2026-01-31", "2026-02-28")]
    [InlineData(RecurrenceFrequency.Monthly, null, "2026-01-31", "2026-02-28", "2026-03-31")]
    [InlineData(RecurrenceFrequency.Quarterly, null, "2026-01-31", "2026-01-31", "2026-04-30")]
    [InlineData(RecurrenceFrequency.SemiAnnually, null, "2026-01-31", "2026-01-31", "2026-07-31")]
    [InlineData(RecurrenceFrequency.Annually, null, "2024-02-29", "2024-02-29", "2025-02-28")]
    public async Task CalculateNextExecutionDateAsync_ShouldReturnNextGridSlot(RecurrenceFrequency frequency,
        int? customIntervalDays, string startDate, string cursor, string expected)
    {
        // Arrange
        var template = NewTemplate(DateTime.Parse(startDate), frequency, DateTime.Parse(cursor));
        template.CustomIntervalDays = customIntervalDays;

        // Act
        var result = await NextDateFixture(template).Service.CalculateNextExecutionDateAsync(template.Id);

        // Assert
        result.Should().Be(DateTime.Parse(expected));
    }

    /// <summary>
    ///     (EN) A missing template is reported as not found.<br />
    ///     (VI) Mẫu không tồn tại được báo là không tìm thấy.
    /// </summary>
    [Fact]
    public async Task CalculateNextExecutionDateAsync_ShouldThrowArgumentException_WhenTemplateNotFound()
    {
        // Act
        var act = () => NextDateFixture(null).Service.CalculateNextExecutionDateAsync(Guid.CreateVersion7());

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
