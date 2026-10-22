using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Domain.Constants;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Contracts.Exceptions;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) A date-only update preserves amount history, owner, references, and pending lifecycle state.<br />
    ///     (VI) PUT chỉ đổi ngày giữ lịch sử tiền, chủ, tham chiếu và trạng thái chờ.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldPreserveOmittedFields_WhenOnlyDateChanges()
    {
        // Arrange
        var expected = Expected(1, amount: 120m);
        expected.IsAdjusted = true;
        expected.OriginalAmount = 100m;
        expected.AdjustmentReason = "Meter reading";
        var fixture = CreateFixture(expected);
        fixture.Templates.Single().Frequency = RecurrenceFrequency.Monthly;
        var request = new ExpectedTransactionUpdateRequest { Id = expected.Id, ExpectedDate = Today.AddDays(-1) };

        // Act
        var view = await fixture.Service.UpdateAsync(expected.Id, request);

        // Assert
        view.Should().NotBeNull();
        view!.ExpectedDate.Should().Be(Today.AddDays(-1));
        view.DisplayStatus.Should().Be(4);
        view.DaysUntilDue.Should().Be(-1);
        view.Status.Should().Be(ExpectedTransactionStatus.Pending);
        view.UserId.Should().Be(OwnerId);
        view.AccountId.Should().Be(AccountId);
        view.RecurringTransactionTemplateId.Should().Be(TemplateId);
        view.ExpectedAmount.Should().Be(120m);
        view.OriginalAmount.Should().Be(100m);
        view.IsAdjusted.Should().BeTrue();
        view.AdjustmentReason.Should().Be("Meter reading");
        view.Description.Should().Be("Electricity bill");
        view.Category.Should().Be("Utilities");
        view.Notes.Should().Be("Keep the receipt");
        view.ProcessedAt.Should().BeNull();
        view.ActualTransactionId.Should().BeNull();
    }

    /// <summary>
    ///     (EN) Amount updates use the adjustment rules and retain the first original amount across subsequent PUTs.<br />
    ///     (VI) PUT số tiền dùng quy tắc điều chỉnh và giữ số gốc đầu tiên qua các lần tiếp theo.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldKeepFirstOriginalAmount_WhenAmountChangesRepeatedly()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);

        // Act
        var first = await fixture.Service.UpdateAsync(expected.Id, new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id, ExpectedAmount = 150.75m, AdjustmentReason = "New reading"
        });
        var second = await fixture.Service.UpdateAsync(expected.Id, new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id, ExpectedAmount = 80.25m, AdjustmentReason = "Corrected reading"
        });

        // Assert
        first!.ExpectedAmount.Should().Be(150.75m);
        first.OriginalAmount.Should().Be(100m);
        first.IsAdjusted.Should().BeTrue();
        second!.ExpectedAmount.Should().Be(80.25m);
        second.OriginalAmount.Should().Be(100m);
        second.IsAdjusted.Should().BeTrue();
        second.AdjustmentReason.Should().Be("Corrected reading");
        second.Status.Should().Be(ExpectedTransactionStatus.Pending);
        second.ProcessedAt.Should().BeNull();
    }

    /// <summary>
    ///     (EN) Terminal periods reject date edits even when the request omits status and the period has no actual link.<br />
    ///     (VI) Kỳ kết thúc từ chối sửa ngày kể cả khi yêu cầu không gửi trạng thái và kỳ không có liên kết thực.
    /// </summary>
    [Theory]
    [InlineData(ExpectedTransactionStatus.Confirmed)]
    [InlineData(ExpectedTransactionStatus.Cancelled)]
    [InlineData(ExpectedTransactionStatus.Completed)]
    public async Task UpdateAsync_ShouldRejectTerminalPeriod_WhenRequestEditsDateWithoutStatus(
        ExpectedTransactionStatus status)
    {
        // Arrange
        var expected = Expected(1, status: status);
        expected.ActualTransactionId = status == ExpectedTransactionStatus.Confirmed ? ActualId : null;
        expected.ProcessedAt = Today.AddDays(-1);
        var originalDate = expected.ExpectedDate;
        var originalLink = expected.ActualTransactionId;
        var originalProcessedAt = expected.ProcessedAt;
        var fixture = CreateFixture(expected);
        var request = new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id, Status = null, ExpectedDate = Today.AddDays(7)
        };

        // Act
        Func<Task> act = () => fixture.Service.UpdateAsync(expected.Id, request);

        // Assert
        await act.Should().ThrowAsync<ExpectedTransactionConflictException>();
        expected.Status.Should().Be(status);
        expected.ActualTransactionId.Should().Be(originalLink);
        expected.ProcessedAt.Should().Be(originalProcessedAt);
        expected.ExpectedDate.Should().Be(originalDate);
        expected.ExpectedAmount.Should().Be(100m);
        expected.OriginalAmount.Should().BeNull();
        expected.IsAdjusted.Should().BeFalse();
        expected.AdjustmentReason.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Generic updates cannot replace dedicated status-transition commands.<br />
    ///     (VI) PUT không được thay command chuyển trạng thái chuyên biệt.
    /// </summary>
    [Theory]
    [InlineData(ExpectedTransactionStatus.Confirmed)]
    [InlineData(ExpectedTransactionStatus.Cancelled)]
    [InlineData(ExpectedTransactionStatus.Completed)]
    public async Task UpdateAsync_ShouldRejectStatusChange_WhenPendingPeriodIsUpdated(ExpectedTransactionStatus status)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        var request = new ExpectedTransactionUpdateRequest { Id = expected.Id, Status = status };

        // Act
        Func<Task> act = () => fixture.Service.UpdateAsync(expected.Id, request);

        // Assert
        await act.Should().ThrowAsync<ExpectedTransactionConflictException>();
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ProcessedAt.Should().BeNull();
        expected.ActualTransactionId.Should().BeNull();
    }

    /// <summary>
    ///     (EN) Non-positive amount updates do not alter stored amounts or adjustment history.<br />
    ///     (VI) PUT số tiền không dương không thay số tiền hay lịch sử điều chỉnh.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateAsync_ShouldRejectNonPositiveAmount_WhenAmountIsProvided(int amount)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        var request = new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id, ExpectedAmount = amount, AdjustmentReason = "New reading"
        };

        // Act
        Func<Task> act = () => fixture.Service.UpdateAsync(expected.Id, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        expected.ExpectedAmount.Should().Be(100m);
        expected.OriginalAmount.Should().BeNull();
        expected.IsAdjusted.Should().BeFalse();
    }

    /// <summary>
    ///     (EN) Changing an amount requires a nonblank reason.<br />
    ///     (VI) Thay số tiền bắt buộc có lý do không trống.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public async Task UpdateAsync_ShouldRejectMissingReason_WhenAmountChanges(string? reason)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        var request = new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id, ExpectedAmount = 120m, AdjustmentReason = reason
        };

        // Act
        Func<Task> act = () => fixture.Service.UpdateAsync(expected.Id, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        expected.ExpectedAmount.Should().Be(100m);
        expected.OriginalAmount.Should().BeNull();
    }

    /// <summary>
    ///     (EN) Route/body ID mismatches preserve the existing key-not-found exception contract.<br />
    ///     (VI) ID route/body lệch giữ hợp đồng KeyNotFoundException hiện hành.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldRejectMismatchedId_WhenRouteAndBodyDiffer()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);

        // Act
        Func<Task> act = () => fixture.Service.UpdateAsync(expected.Id,
            new ExpectedTransactionUpdateRequest { Id = Id(2), ExpectedDate = Today.AddDays(7) });

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
        expected.ExpectedDate.Should().Be(Today);
    }

    /// <summary>
    ///     (EN) Missing expected references remain explicit entity-not-found failures.<br />
    ///     (VI) Tham chiếu kỳ thiếu giữ lỗi không tìm thấy thực thể rõ ràng.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldThrowEntityNotFound_WhenExpectedReferenceIsUnavailable()
    {
        // Arrange
        var fixture = CreateFixture();

        // Act
        Func<Task> act = () => fixture.Service.UpdateAsync(Id(1), new ExpectedTransactionUpdateRequest { Id = Id(1) });

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    /// <summary>
    ///     (EN) A zero affected-row result retains the update-failed exception contract.<br />
    ///     (VI) Kết quả không có bản ghi bị ảnh hưởng giữ hợp đồng ngoại lệ cập nhật thất bại.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldThrowUpdateFailedException_WhenRepositoryReportsNoUpdate()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.Templates.Single().Frequency = RecurrenceFrequency.Monthly;
        fixture.ExpectedRepository.Setup(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        Func<Task> act = () => fixture.Service.UpdateAsync(expected.Id,
            new ExpectedTransactionUpdateRequest { Id = expected.Id, ExpectedDate = Today.AddDays(1) });

        // Assert
        await act.Should().ThrowAsync<UpdateFailedException>();
    }

    /// <summary>
    ///     (EN) Update persistence failures propagate without a fabricated success response.<br />
    ///     (VI) Lỗi lưu trữ PUT được truyền ra, không tạo phản hồi thành công giả.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldPropagatePersistenceFailure_WhenWriteFails()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        var error = new DbUpdateException("Update failed");
        fixture.ExpectedRepository.Setup(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(error);

        // Act
        Func<Task> act = () => fixture.Service.UpdateAsync(expected.Id,
            new ExpectedTransactionUpdateRequest { Id = expected.Id, Description = "Updated bill" });

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    /// <summary>
    ///     (EN) A same-amount edit can change text without creating or replacing adjustment history.<br />
    ///     (VI) Chỉnh sửa cùng số tiền được đổi text nhưng không tạo mới hay thay lịch sử điều chỉnh.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateAsync_ShouldPreserveAmountHistoryWhileEditingText_WhenAmountIsUnchanged(bool adjusted)
    {
        // Arrange
        var expected = Expected(1, amount: 120m);
        expected.IsAdjusted = adjusted;
        expected.OriginalAmount = adjusted ? 100m : null;
        expected.AdjustmentReason = adjusted ? "Existing reading" : null;
        var fixture = CreateFixture(expected);
        var request = new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id,
            Status = ExpectedTransactionStatus.Pending,
            ExpectedAmount = 120m,
            AdjustmentReason = "Same-value input",
            Description = "Corrected electricity description",
            Category = "Energy",
            Notes = ""
        };

        // Act
        var view = await fixture.Service.UpdateAsync(expected.Id, request);

        // Assert
        view!.ExpectedAmount.Should().Be(120m);
        view.OriginalAmount.Should().Be(adjusted ? 100m : (decimal?)null);
        view.IsAdjusted.Should().Be(adjusted);
        view.AdjustmentReason.Should().Be(adjusted ? "Existing reading" : null);
        view.ExpectedDate.Should().Be(Today);
        view.Description.Should().Be("Corrected electricity description");
        view.Category.Should().Be("Energy");
        view.Notes.Should().BeEmpty();
        view.Status.Should().Be(ExpectedTransactionStatus.Pending);
        view.ActualTransactionId.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) A legacy Pending period with an actual link cannot be edited through generic updates.<br />
    ///     (VI) Kỳ Pending cũ đã có liên kết thực không được chỉnh sửa qua PUT thông thường.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldRejectPendingLinkedPeriod_WithoutChangingStoredFields()
    {
        // Arrange
        var expected = Expected(1);
        expected.ActualTransactionId = ActualId;
        var fixture = CreateFixture(expected);
        var request = new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id, ExpectedAmount = 150.75m, AdjustmentReason = "New bill",
            ExpectedDate = Today.AddDays(7), Description = "Replacement"
        };

        // Act
        Func<Task> act = () => fixture.Service.UpdateAsync(expected.Id, request);

        // Assert
        await act.Should().ThrowAsync<ExpectedTransactionConflictException>();
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ActualTransactionId.Should().Be(ActualId);
        expected.ExpectedAmount.Should().Be(100m);
        expected.ExpectedDate.Should().Be(Today);
        expected.Description.Should().Be("Electricity bill");
        expected.OriginalAmount.Should().BeNull();
        expected.IsAdjusted.Should().BeFalse();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) A loaded expected tombstone is not found for editing and keeps its stored financial fields.<br />
    ///     (VI) Kỳ xóa mềm được tải được xem là không tìm thấy khi chỉnh sửa và giữ các trường tài chính đã lưu.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldRejectSoftDeletedPeriod_WithoutChangingStoredFields()
    {
        // Arrange
        var expected = Expected(1);
        expected.IsDeleted = "20261008120000";
        var fixture = CreateFixture(expected);
        var request = new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id, ExpectedAmount = 150.75m, AdjustmentReason = "New bill",
            ExpectedDate = Today.AddDays(7)
        };

        // Act
        Func<Task> act = () => fixture.Service.UpdateAsync(expected.Id, request);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
        expected.IsDeleted.Should().Be("20261008120000");
        expected.ExpectedAmount.Should().Be(100m);
        expected.ExpectedDate.Should().Be(Today);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ActualTransactionId.Should().BeNull();
        expected.OriginalAmount.Should().BeNull();
        expected.IsAdjusted.Should().BeFalse();
    }

    /// <summary>
    ///     (EN) The maximum 500-character reason is accepted for an amount edit with first-original preservation.<br />
    ///     (VI) Lý do tối đa 500 ký tự được chấp nhận khi sửa tiền và giữ số gốc đầu tiên.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldAcceptMaximumLengthReason_WhenAmountChanges()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        var reason = new string('r', 500);

        // Act
        var view = await fixture.Service.UpdateAsync(expected.Id, new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id, ExpectedAmount = 150.75m, AdjustmentReason = reason
        });

        // Assert
        view!.ExpectedAmount.Should().Be(150.75m);
        view.OriginalAmount.Should().Be(100m);
        view.IsAdjusted.Should().BeTrue();
        view.AdjustmentReason.Should().Be(reason);
        view.Status.Should().Be(ExpectedTransactionStatus.Pending);
        view.ActualTransactionId.Should().BeNull();
    }

    /// <summary>
    ///     (EN) A 501-character reason rejects the complete update without changing existing adjustment history.<br />
    ///     (VI) Lý do 501 ký tự từ chối toàn bộ PUT mà không thay lịch sử điều chỉnh hiện có.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldRejectOverlongReason_WithoutChangingStoredFields()
    {
        // Arrange
        var expected = Expected(1, amount: 120m);
        expected.IsAdjusted = true;
        expected.OriginalAmount = 100m;
        expected.AdjustmentReason = "Existing reading";
        var fixture = CreateFixture(expected);
        var request = new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id, ExpectedAmount = 150.75m, AdjustmentReason = new string('r', 501),
            ExpectedDate = Today.AddDays(7)
        };

        // Act
        Func<Task> act = () => fixture.Service.UpdateAsync(expected.Id, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        expected.ExpectedAmount.Should().Be(120m);
        expected.OriginalAmount.Should().Be(100m);
        expected.IsAdjusted.Should().BeTrue();
        expected.AdjustmentReason.Should().Be("Existing reading");
        expected.ExpectedDate.Should().Be(Today);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
    }

    /// <summary>
    ///     (EN) Moving a monthly period across months rejects the whole edit before changing money or metadata.<br />
    ///     (VI) Dời kỳ hàng tháng sang tháng khác từ chối toàn bộ chỉnh sửa trước khi thay số tiền hay metadata.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateAsync_ShouldRejectEntireEditBeforeMutation_WhenMonthlyDateMovesToAnotherMonth(bool adjusted)
    {
        // Arrange
        var expected = Expected(1, amount: 120m);
        expected.ExpectedDate = new DateTime(2026, 1, 31);
        expected.IsAdjusted = adjusted;
        expected.OriginalAmount = adjusted ? 100m : null;
        expected.AdjustmentReason = adjusted ? "Existing reading" : null;
        var before = new
        {
            expected.ExpectedDate, expected.ExpectedAmount, expected.OriginalAmount, expected.IsAdjusted,
            expected.AdjustmentReason, expected.Description, expected.Category, expected.Notes,
            expected.Status, expected.ProcessedAt, expected.ActualTransactionId, expected.UserId,
            expected.AccountId, expected.RecurringTransactionTemplateId, expected.UpdatedAt
        };
        var fixture = CreateFixture(expected);
        fixture.Templates.Single().Frequency = RecurrenceFrequency.Monthly;
        var request = new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id, ExpectedDate = new DateTime(2026, 2, 2), ExpectedAmount = 150m,
            AdjustmentReason = "Replacement reading", Description = "Replacement bill",
            Category = "Replacement category", Notes = "Replacement notes"
        };

        // Act
        Func<Task> act = () => fixture.Service.UpdateAsync(expected.Id, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(CoreFinanceMessages.ExpectedDateMustStayInSlot);
        expected.Should().BeEquivalentTo(before);
        fixture.ExpectedRepository.Verify(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>(),
            It.IsAny<CancellationToken>()), Times.Never);
        fixture.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.Transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        fixture.Transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    ///     (EN) A monthly period can move within its month while applying the supplied amount and notes.<br />
    ///     (VI) Kỳ hàng tháng được dời trong cùng tháng và áp dụng số tiền cùng ghi chú được gửi.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldApplyEdit_WhenMonthlyDateStaysInSameMonth()
    {
        // Arrange
        var expected = Expected(1);
        expected.ExpectedDate = new DateTime(2026, 1, 31);
        var fixture = CreateFixture(expected);
        fixture.Templates.Single().Frequency = RecurrenceFrequency.Monthly;
        var request = new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id, ExpectedDate = new DateTime(2026, 1, 25), ExpectedAmount = 150m,
            AdjustmentReason = "New reading", Notes = "Updated receipt"
        };

        // Act
        var view = await fixture.Service.UpdateAsync(expected.Id, request);

        // Assert
        view.Should().NotBeNull();
        view!.ExpectedDate.Should().Be(new DateTime(2026, 1, 25));
        view.ExpectedAmount.Should().Be(150m);
        view.OriginalAmount.Should().Be(100m);
        view.IsAdjusted.Should().BeTrue();
        view.AdjustmentReason.Should().Be("New reading");
        view.Notes.Should().Be("Updated receipt");
        view.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ExpectedDate.Should().Be(view.ExpectedDate);
        fixture.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        fixture.Transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        fixture.Transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    ///     (EN) A period without a recurring template remains free to move across calendar months.<br />
    ///     (VI) Kỳ không gắn mẫu định kỳ vẫn được tự do dời sang tháng khác.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldAllowCrossMonthMove_WhenPeriodHasNoTemplate()
    {
        // Arrange
        var expected = Expected(1);
        expected.ExpectedDate = new DateTime(2026, 1, 31);
        expected.RecurringTransactionTemplateId = null;
        var fixture = CreateFixture(expected);
        var request = new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id, ExpectedDate = new DateTime(2026, 2, 2)
        };

        // Act
        var view = await fixture.Service.UpdateAsync(expected.Id, request);

        // Assert
        view!.ExpectedDate.Should().Be(new DateTime(2026, 2, 2));
        view.RecurringTransactionTemplateId.Should().BeNull();
        expected.ExpectedDate.Should().Be(view.ExpectedDate);
        expected.ExpectedAmount.Should().Be(100m);
        fixture.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        fixture.Transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        fixture.Transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    ///     (EN) Day-based schedules reject a move to another day even within the same month, without partial edits.<br />
    ///     (VI) Lịch theo ngày từ chối dời sang ngày khác dù cùng tháng và không chỉnh sửa một phần.
    /// </summary>
    [Theory]
    [InlineData(RecurrenceFrequency.Daily)]
    [InlineData(RecurrenceFrequency.Weekly)]
    [InlineData(RecurrenceFrequency.Biweekly)]
    [InlineData(RecurrenceFrequency.Custom)]
    public async Task UpdateAsync_ShouldRejectEntireEdit_WhenDayBasedDateMovesToAnotherDay(RecurrenceFrequency frequency)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.Templates.Single().Frequency = frequency;
        var request = new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id, ExpectedDate = Today.AddDays(1), ExpectedAmount = 150m,
            AdjustmentReason = "New reading", Notes = "Replacement notes"
        };

        // Act
        Func<Task> act = () => fixture.Service.UpdateAsync(expected.Id, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(CoreFinanceMessages.ExpectedDateMustStayInSlot);
        expected.ExpectedDate.Should().Be(Today);
        expected.ExpectedAmount.Should().Be(100m);
        expected.OriginalAmount.Should().BeNull();
        expected.IsAdjusted.Should().BeFalse();
        expected.AdjustmentReason.Should().BeNull();
        expected.Notes.Should().Be("Keep the receipt");
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        fixture.ExpectedRepository.Verify(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>(),
            It.IsAny<CancellationToken>()), Times.Never);
        fixture.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.Transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        fixture.Transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    ///     (EN) A missing or deleted template imposes no generation-slot restriction on an existing period.<br />
    ///     (VI) Mẫu không tồn tại hoặc đã xóa không giới hạn slot sinh kỳ của kỳ hiện có.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateAsync_ShouldAllowCrossMonthMove_WhenTemplateIsMissingOrDeleted(bool deleted)
    {
        // Arrange
        var expected = Expected(1);
        expected.ExpectedDate = new DateTime(2026, 1, 31);
        var fixture = CreateFixture(expected);
        fixture.Templates.Single().Frequency = RecurrenceFrequency.Monthly;
        if (deleted) fixture.Templates.Single().IsDeleted = "20260131000000";
        else fixture.Templates.Clear();
        var request = new ExpectedTransactionUpdateRequest
        {
            Id = expected.Id, ExpectedDate = new DateTime(2026, 2, 2)
        };

        // Act
        var view = await fixture.Service.UpdateAsync(expected.Id, request);

        // Assert
        view!.ExpectedDate.Should().Be(new DateTime(2026, 2, 2));
        view.RecurringTransactionTemplateId.Should().Be(TemplateId);
        expected.ExpectedDate.Should().Be(view.ExpectedDate);
        expected.ExpectedAmount.Should().Be(100m);
        fixture.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        fixture.Transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        fixture.Transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
