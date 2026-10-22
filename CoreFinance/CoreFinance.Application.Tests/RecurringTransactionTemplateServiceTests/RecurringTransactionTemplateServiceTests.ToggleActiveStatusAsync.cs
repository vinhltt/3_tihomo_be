using CoreFinance.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

public partial class RecurringTransactionTemplateServiceTests
{
    /// <summary>
    ///     (EN) Toggling an existing template persists the new state and commits.<br />
    ///     (VI) Bật/tắt mẫu hiện có thì lưu trạng thái mới và commit.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ToggleActiveStatusAsync_ShouldPersistNewState_WhenTemplateExists(bool isActive)
    {
        // Arrange
        var template = NewTemplate(D(2026, 1, 1));
        template.IsActive = !isActive;
        var harness = new GenerationHarness { Template = template };

        // Act
        var result = await harness.Service().ToggleActiveStatusAsync(template.Id, isActive);

        // Assert
        result.Should().BeTrue();
        template.IsActive.Should().Be(isActive);
    }

    /// <summary>
    ///     (EN) A missing template returns false and writes nothing.<br />
    ///     (VI) Mẫu không tồn tại trả về false và không ghi gì.
    /// </summary>
    [Fact]
    public async Task ToggleActiveStatusAsync_ShouldReturnFalse_WhenTemplateNotFound()
    {
        // Arrange
        var harness = new GenerationHarness();

        // Act
        var result = await harness.Service().ToggleActiveStatusAsync(Guid.CreateVersion7(), false);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    ///     (EN) A persistence failure returns false and rolls back.<br />
    ///     (VI) Lỗi lưu trữ trả về false và rollback.
    /// </summary>
    [Fact]
    public async Task ToggleActiveStatusAsync_ShouldReturnFalseAndRollback_WhenUpdateFails()
    {
        // Arrange
        var harness = new GenerationHarness { Template = NewTemplate(D(2026, 1, 1)) };
        harness.TemplateRepo
            .Setup(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));

        // Act
        var result = await harness.Service().ToggleActiveStatusAsync(harness.Template.Id, false);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    ///     (EN) Cancellation while acquiring the template lock propagates without toggling the stored state.<br />
    ///     (VI) Hủy khi lấy khóa mẫu được truyền ra mà không đổi trạng thái đã lưu.
    /// </summary>
    [Fact]
    public async Task ToggleActiveStatusAsync_ShouldPropagateCancellationWithoutChangingState_WhenCancelled()
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
        var act = () => harness.Service().ToggleActiveStatusAsync(template.Id, false, cancellation.Token);

        // Assert
        var failure = await act.Should().ThrowAsync<OperationCanceledException>();
        failure.Which.CancellationToken.Should().Be(cancellation.Token);
        template.IsActive.Should().BeTrue();
        template.NextExecutionDate.Should().Be(Today);
        harness.Created.Should().BeEmpty();
    }
}
