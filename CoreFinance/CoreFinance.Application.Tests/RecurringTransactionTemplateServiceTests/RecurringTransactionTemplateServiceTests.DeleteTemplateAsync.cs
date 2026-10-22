using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using Moq;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

public partial class RecurringTransactionTemplateServiceTests
{
    /// <summary>
    ///     (EN) Deletion selects the template soft-delete boundary and leaves generated period history unchanged.<br />
    ///     (VI) Xóa chọn biên xóa mềm của mẫu và giữ nguyên lịch sử các kỳ đã sinh.
    /// </summary>
    [Fact]
    public async Task DeleteTemplateAsync_ShouldSoftDeleteTemplateWithoutChangingPeriods_WhenTemplateExists()
    {
        // Arrange
        var template = NewTemplate(Today.AddDays(-1), RecurrenceFrequency.Daily);
        var harness = new GenerationHarness { Template = template };
        var service = harness.Service();
        await service.GenerateExpectedTransactionsAsync(template.Id);
        var confirmed = harness.Created[0];
        confirmed.Status = ExpectedTransactionStatus.Confirmed;
        confirmed.ActualTransactionId = Guid.CreateVersion7();
        confirmed.IsAdjusted = true;
        confirmed.OriginalAmount = 100m;
        confirmed.ExpectedAmount = 150.75m;
        confirmed.AdjustmentReason = "Corrected bill";
        harness.Created[1].IsDeleted = "20260315100000";
        var history = harness.Created.Select(period => new
        {
            period.Id, period.RecurringTransactionTemplateId, period.AccountId, period.ExpectedDate,
            period.ExpectedAmount, period.Status, period.ActualTransactionId, period.IsAdjusted,
            period.OriginalAmount, period.AdjustmentReason, period.IsDeleted
        }).ToArray();
        var cursor = template.NextExecutionDate;

        // Act
        var result = await service.DeleteTemplateAsync(template.Id);

        // Assert
        result.Should().BeTrue();
        // The fixture models the repository tombstone, not durable deletion or transactional rollback.
        template.IsDeleted.Should().NotBeNullOrWhiteSpace();
        template.NextExecutionDate.Should().Be(cursor);
        harness.Created.Select(period => new
        {
            period.Id, period.RecurringTransactionTemplateId, period.AccountId, period.ExpectedDate,
            period.ExpectedAmount, period.Status, period.ActualTransactionId, period.IsAdjusted,
            period.OriginalAmount, period.AdjustmentReason, period.IsDeleted
        }).Should().BeEquivalentTo(history, options => options.WithStrictOrdering());
    }

    /// <summary>
    ///     (EN) A missing template is not reported as a successful deletion.<br />
    ///     (VI) Mẫu không được trả về không được báo xóa thành công.
    /// </summary>
    [Fact]
    public async Task DeleteTemplateAsync_ShouldReturnFalse_WhenTemplateIsMissing()
    {
        // Arrange
        var harness = new GenerationHarness();

        // Act
        var result = await harness.Service().DeleteTemplateAsync(Guid.CreateVersion7());

        // Assert
        result.Should().BeFalse();
        harness.Created.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) A soft-delete persistence exception propagates without a successful deletion result.<br />
    ///     (VI) Ngoại lệ lưu trữ khi xóa mềm được truyền ra, không trả kết quả xóa thành công.
    /// </summary>
    [Fact]
    public async Task DeleteTemplateAsync_ShouldPropagatePersistenceFailure_WhenSoftDeleteFails()
    {
        // Arrange
        var template = NewTemplate(Today);
        var harness = new GenerationHarness { Template = template };
        var error = new InvalidOperationException("Delete failed");
        harness.TemplateRepo.Setup(repository => repository.DeleteSoftAsync(It.IsAny<RecurringTransactionTemplate>()))
            .ThrowsAsync(error);

        // Act
        var act = () => harness.Service().DeleteTemplateAsync(template.Id);

        // Assert
        var failure = await act.Should().ThrowAsync<InvalidOperationException>();
        failure.Which.Should().BeSameAs(error);
        template.IsDeleted.Should().BeNull();
        template.NextExecutionDate.Should().Be(Today);
        harness.Created.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Cancellation while acquiring the lock propagates before the template is deleted.<br />
    ///     (VI) Hủy khi lấy khóa được truyền ra trước khi mẫu bị xóa.
    /// </summary>
    [Fact]
    public async Task DeleteTemplateAsync_ShouldPropagateCancellationWithoutDeletingTemplate_WhenCancelled()
    {
        // Arrange
        var template = NewTemplate(Today);
        var harness = new GenerationHarness { Template = template };
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        harness.UnitOfWork.Setup(unitOfWork =>
                unitOfWork.AcquireOwnerResourceLockAsync("recurring", template.Id, It.IsAny<CancellationToken>()))
            .Returns<string, Guid, CancellationToken>((_, _, token) =>
            {
                token.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            });

        // Act
        var act = () => harness.Service().DeleteTemplateAsync(template.Id, cancellation.Token);

        // Assert
        var failure = await act.Should().ThrowAsync<OperationCanceledException>();
        failure.Which.CancellationToken.Should().Be(cancellation.Token);
        template.IsDeleted.Should().BeNull();
        template.NextExecutionDate.Should().Be(Today);
        harness.Created.Should().BeEmpty();
    }
}
