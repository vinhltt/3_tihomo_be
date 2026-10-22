using System.Linq.Expressions;
using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.DTOs.Transaction;
using CoreFinance.Application.Enums;
using CoreFinance.Application.Interfaces;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

public partial class ExpectedTransactionServiceTests
{
    /// <summary>
    ///     (EN) Payment exposes correctly classified actual amounts and uses the adjusted amount and exact period date.<br />
    ///     (VI) Ghi nhận trả đúng phân loại thu/chi, dùng số đã chỉnh và chính xác ngày của kỳ.
    /// </summary>
    [Theory]
    [InlineData(RecurringTransactionType.Income, -1, CategoryType.Income)]
    [InlineData(RecurringTransactionType.Income, 0, CategoryType.Income)]
    [InlineData(RecurringTransactionType.Expense, -1, CategoryType.Expense)]
    [InlineData(RecurringTransactionType.Expense, 7, CategoryType.Expense)]
    public async Task RecordPaymentAsync_ShouldExposeActualAndConfirmedPeriod_WhenIncomeOrExpenseIsPending(
        RecurringTransactionType type, int days, CategoryType categoryType)
    {
        // Arrange
        var expected = Expected(1, days, amount: 150.75m, type: type);
        expected.ExpectedDate = expected.ExpectedDate.AddHours(20);
        expected.IsAdjusted = true;
        expected.OriginalAmount = 100m;
        expected.AdjustmentReason = "Updated bill";
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest());
        var view = await fixture.Service.GetByIdAsync(expected.Id);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Success);
        result.ExpectedTransactionId.Should().Be(expected.Id);
        result.ActualTransaction.Should().NotBeNull();
        var actual = result.ActualTransaction!;
        actual.Id.Should().NotBeEmpty();
        actual.AccountId.Should().Be(AccountId);
        actual.TransactionDate.Should().Be(Today.AddDays(days).AddHours(20));
        actual.RevenueAmount.Should().Be(type == RecurringTransactionType.Income ? 150.75m : 0m);
        actual.SpentAmount.Should().Be(type == RecurringTransactionType.Expense ? 150.75m : 0m);
        actual.CategoryType.Should().Be(categoryType);
        actual.CategorySummary.Should().Be("Utilities");
        actual.Description.Should().Be("Electricity bill");
        result.ActualTransactionId.Should().Be(actual.Id);
        view.Should().NotBeNull();
        view!.ActualTransactionId.Should().Be(actual.Id);
        view.Status.Should().Be(ExpectedTransactionStatus.Confirmed);
        view.DisplayStatus.Should().Be(1);
        view.ProcessedAt.Should().NotBeNull();
        view.OriginalAmount.Should().Be(100m);
    }

    /// <summary>
    ///     (EN) An unbound period uses the chosen account for its actual without changing the period's nullable account.<br />
    ///     (VI) Kỳ chưa gắn tài khoản dùng tài khoản được chọn cho giao dịch thực nhưng không đổi tài khoản null của kỳ.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldUseFallbackWithoutBindingPeriod_WhenAccountIsNotConfigured()
    {
        // Arrange
        var expected = Expected(1, -3);
        expected.AccountId = null;
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id,
            new RecordPaymentRequest { AccountId = AccountId });
        var view = await fixture.Service.GetByIdAsync(expected.Id);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Success);
        result.ActualTransaction!.AccountId.Should().Be(AccountId);
        expected.AccountId.Should().BeNull();
        view!.AccountId.Should().BeNull();
        view.Status.Should().Be(ExpectedTransactionStatus.Confirmed);
    }

    /// <summary>
    ///     (EN) Re-selecting the configured account is permitted and does not reinterpret it as an override.<br />
    ///     (VI) Chọn lại tài khoản đã cấu hình được chấp nhận và không bị hiểu là đổi tài khoản.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldAcceptConfiguredAccount_WhenRequestSelectsSameAccount()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id,
            new RecordPaymentRequest { AccountId = AccountId });

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Success);
        result.ActualTransaction!.AccountId.Should().Be(AccountId);
        expected.AccountId.Should().Be(AccountId);
    }

    /// <summary>
    ///     (EN) A configured account cannot be overridden even with another available same-owner account.<br />
    ///     (VI) Không được thay tài khoản cấu hình kể cả bằng tài khoản cùng chủ hiện có.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldReturnInvalidRequest_WhenRequestOverridesConfiguredAccount()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.Accounts.Add(new Account { Id = Id(50), UserId = OwnerId, CurrentBalance = 8000m });

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id,
            new RecordPaymentRequest { AccountId = Id(50) });

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.InvalidRequest);
        expected.AccountId.Should().Be(AccountId);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ActualTransactionId.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Transfer periods cannot create a one-sided actual transaction.<br />
    ///     (VI) Kỳ chuyển khoản không được tạo giao dịch thực một phía.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldReturnInvalidRequest_WhenPeriodIsTransfer()
    {
        // Arrange
        var expected = Expected(1, type: RecurringTransactionType.Transfer);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest());

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.InvalidRequest);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ActualTransactionId.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) An unbound period requires a nonempty selected account ID.<br />
    ///     (VI) Kỳ chưa gắn tài khoản bắt buộc chọn ID tài khoản không rỗng.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RecordPaymentAsync_ShouldReturnInvalidRequest_WhenFallbackAccountIsMissingOrEmpty(bool empty)
    {
        // Arrange
        var expected = Expected(1);
        expected.AccountId = null;
        var fixture = CreateFixture(expected);
        var request = new RecordPaymentRequest { AccountId = empty ? Guid.Empty : null };

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, request);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.InvalidRequest);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.AccountId.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Missing or mismatched-owner accounts are rejected by the service's reference guard, not isolation proof.<br />
    ///     (VI) Guard tham chiếu dịch vụ từ chối tài khoản thiếu hoặc khác chủ, không chứng minh cách ly dữ liệu.
    /// </summary>
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task RecordPaymentAsync_ShouldReturnNotFound_WhenAccountReferenceIsMissingOrForeign(
        bool fallback, bool foreign)
    {
        // Arrange
        var expected = Expected(1);
        if (fallback) expected.AccountId = null;
        var fixture = CreateFixture(expected);
        if (foreign) fixture.Accounts.Single().UserId = OtherOwnerId;
        else fixture.Accounts.Clear();

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id,
            new RecordPaymentRequest { AccountId = fallback ? AccountId : null });

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.NotFound);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ActualTransactionId.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Non-positive period amounts cannot become actual financial amounts.<br />
    ///     (VI) Số tiền kỳ không dương không được trở thành số tiền giao dịch thực.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RecordPaymentAsync_ShouldReturnInvalidRequest_WhenAmountIsNotPositive(int amount)
    {
        // Arrange
        var expected = Expected(1, amount: amount);
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest());

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.InvalidRequest);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Every terminal state rejects payment without changing its existing link or timestamp.<br />
    ///     (VI) Mọi trạng thái kết thúc từ chối ghi nhận mà không thay liên kết hay thời điểm hiện có.
    /// </summary>
    [Theory]
    [InlineData(ExpectedTransactionStatus.Confirmed)]
    [InlineData(ExpectedTransactionStatus.Cancelled)]
    [InlineData(ExpectedTransactionStatus.Completed)]
    public async Task RecordPaymentAsync_ShouldReturnConflict_WhenPeriodIsNotPending(ExpectedTransactionStatus status)
    {
        // Arrange
        var expected = Expected(1, status: status);
        expected.ActualTransactionId = status == ExpectedTransactionStatus.Confirmed ? ActualId : null;
        expected.ProcessedAt = Today.AddDays(-1);
        var originalDate = expected.ExpectedDate;
        var originalLink = expected.ActualTransactionId;
        var originalProcessedAt = expected.ProcessedAt;
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest());

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Conflict);
        expected.Status.Should().Be(status);
        expected.ExpectedDate.Should().Be(originalDate);
        expected.ActualTransactionId.Should().Be(originalLink);
        expected.ProcessedAt.Should().Be(originalProcessedAt);
        expected.ExpectedAmount.Should().Be(100m);
        expected.OriginalAmount.Should().BeNull();
        expected.IsAdjusted.Should().BeFalse();
        expected.AdjustmentReason.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) A pending period with an existing actual link cannot create a replacement actual.<br />
    ///     (VI) Kỳ chờ có liên kết thực hiện có không được tạo giao dịch thực thay thế.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldReturnConflict_WhenPendingPeriodAlreadyHasActualLink()
    {
        // Arrange
        var expected = Expected(1);
        expected.ActualTransactionId = ActualId;
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest());

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Conflict);
        expected.ActualTransactionId.Should().Be(ActualId);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Unavailable expected references return not found without creating an actual.<br />
    ///     (VI) Tham chiếu kỳ không được trả về báo không tìm thấy mà không tạo giao dịch thực.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldReturnNotFound_WhenExpectedReferenceIsUnavailable()
    {
        // Arrange
        var fixture = CreateFixture();

        // Act
        var result = await fixture.Service.RecordPaymentAsync(Id(1), new RecordPaymentRequest());

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.NotFound);
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) An actual creation error is an explicit persistence failure, never a successful payment.<br />
    ///     (VI) Lỗi tạo giao dịch thực trả lỗi lưu trữ rõ ràng, không bao giờ là ghi nhận thành công.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldReturnPersistenceError_WhenActualCreationFails()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.ActualRepository.Setup(r => r.CreateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Insert failed"));

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest());

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.PersistenceError);
        result.ActualTransaction.Should().BeNull();
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ActualTransactionId.Should().BeNull();
    }

    /// <summary>
    ///     (EN) A failed commit cannot expose a successful payment; database undo requires real-database verification.<br />
    ///     (VI) Commit lỗi không được trả ghi nhận thành công; việc hoàn tác cần kiểm chứng bằng cơ sở dữ liệu thật.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldReturnPersistenceError_WhenCommitFails()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.Transaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Commit failed"));

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest());

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.PersistenceError);
        result.ActualTransaction.Should().BeNull();
    }

    /// <summary>
    ///     (EN) A bound account does not make a null request or an explicitly empty account ID valid.<br />
    ///     (VI) Tài khoản đã gắn không làm yêu cầu null hay ID tài khoản rỗng được truyền rõ ràng trở nên hợp lệ.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RecordPaymentAsync_ShouldRejectInvalidRequest_WhenBoundPeriodReceivesNullOrEmptyAccount(
        bool nullRequest)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        var request = nullRequest ? null : new RecordPaymentRequest { AccountId = Guid.Empty };

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, request!);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.InvalidRequest);
        result.ActualTransaction.Should().BeNull();
        result.ActualTransactionId.Should().BeNull();
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ActualTransactionId.Should().BeNull();
        expected.ProcessedAt.Should().BeNull();
        expected.ExpectedAmount.Should().Be(100m);
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) A null or identity-less actual result cannot confirm a period or expose payment success.<br />
    ///     (VI) Kết quả giao dịch thực null hoặc thiếu ID không được xác nhận kỳ hay trả thanh toán thành công.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RecordPaymentAsync_ShouldReturnPersistenceError_WhenActualResultIsNullOrHasEmptyId(bool emptyId)
    {
        // Arrange
        var expected = Expected(1);
        var creator = new Mock<ITransactionService>();
        creator.Setup(service => service.CreateAsync(It.IsAny<TransactionCreateRequest>()))
            .ReturnsAsync(emptyId ? new TransactionViewModel { Id = Guid.Empty } : null);
        var fixture = new ServiceFixture(_mapper, [expected], transactionService: creator.Object);

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest());

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.PersistenceError);
        result.ActualTransaction.Should().BeNull();
        result.ActualTransactionId.Should().BeNull();
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ActualTransactionId.Should().BeNull();
        expected.ProcessedAt.Should().BeNull();
    }

    /// <summary>
    ///     (EN) A loaded tombstone is not processable, even before payment input is evaluated.<br />
    ///     (VI) Kỳ xóa mềm được tải không được xử lý, kể cả trước khi kiểm tra đầu vào thanh toán.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldReturnNotFoundWithoutCreatingActual_WhenPeriodIsSoftDeleted()
    {
        // Arrange
        var expected = Expected(1);
        expected.IsDeleted = "20261008120000";
        var fixture = CreateFixture(expected);

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, null!);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.NotFound);
        expected.IsDeleted.Should().Be("20261008120000");
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ExpectedAmount.Should().Be(100m);
        expected.ActualTransactionId.Should().BeNull();
        expected.ProcessedAt.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) A zero affected period update cannot return a successful actual payment.<br />
    ///     (VI) Cập nhật kỳ không ảnh hưởng bản ghi nào không được trả thanh toán thực thành công.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldReturnPersistenceErrorWithoutActual_WhenPeriodUpdateAffectsNoRows()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.ExpectedRepository.Setup(repository => repository.UpdateAsync(It.IsAny<ExpectedTransaction>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest());

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.PersistenceError);
        result.ActualTransaction.Should().BeNull();
        result.ActualTransactionId.Should().BeNull();
    }

    /// <summary>
    ///     (EN) Disposal after a successful commit preserves success and the actual link so clients must not pay again.<br />
    ///     (VI) Dispose sau commit thành công giữ kết quả thành công và liên kết thực để client không thanh toán lại.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldPreserveCommittedPayment_WhenTransactionDisposalFails()
    {
        // Arrange
        var expected = Expected(1, amount: 150.75m);
        var fixture = CreateFixture(expected);
        fixture.Transaction.Setup(transaction => transaction.DisposeAsync())
            .Returns(() => ValueTask.FromException(new InvalidOperationException("Disposal failed")));

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest());
        var view = await fixture.Service.GetByIdAsync(expected.Id);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Success);
        result.ActualTransaction.Should().NotBeNull();
        var actual = result.ActualTransaction!;
        actual.Id.Should().Be(fixture.CreatedActualTransactions.Single().Id);
        actual.SpentAmount.Should().Be(150.75m);
        actual.RevenueAmount.Should().Be(0m);
        result.ActualTransactionId.Should().Be(actual.Id);
        expected.ActualTransactionId.Should().Be(actual.Id);
        view!.Status.Should().Be(ExpectedTransactionStatus.Confirmed);
        view.ActualTransactionId.Should().Be(actual.Id);
        view.ProcessedAt.Should().NotBeNull();
    }

    /// <summary>
    ///     (EN) A cancelled request propagates its cancellation instead of returning a persistence outcome.<br />
    ///     (VI) Yêu cầu đã hủy truyền lỗi hủy ra thay vì trả kết quả lỗi lưu trữ.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldPropagateCancellation_WhenRequestTokenIsCancelled()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        // Act
        var act = () => fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest(), cancellation.Token);

        // Assert
        var failure = await act.Should().ThrowAsync<OperationCanceledException>();
        failure.Which.CancellationToken.Should().Be(cancellation.Token);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ActualTransactionId.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Dependency cancellation with an uncancelled request is classified as persistence failure.<br />
    ///     (VI) Lỗi hủy từ dependency khi yêu cầu chưa hủy được phân loại là lỗi lưu trữ.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldReturnPersistenceError_WhenDependencyCancelsWithoutRequestCancellation()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.ExpectedRepository.Setup(repository => repository.UpdateAsync(It.IsAny<ExpectedTransaction>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest(),
            CancellationToken.None);

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.PersistenceError);
        result.ActualTransaction.Should().BeNull();
        result.ActualTransactionId.Should().BeNull();
    }

    /// <summary>
    ///     (EN) Repository authorization failures propagate rather than being hidden as payment persistence errors.<br />
    ///     (VI) Lỗi xác thực từ repository được truyền ra thay vì bị che thành lỗi lưu trữ thanh toán.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldPropagateUnauthorizedAccess_WhenRepositoryRejectsOwner()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        var error = new UnauthorizedAccessException();
        fixture.ExpectedRepository.Setup(repository => repository.GetByIdAsync(expected.Id,
                It.IsAny<Expression<Func<ExpectedTransaction, object>>[]>()))
            .ThrowsAsync(error);

        // Act
        var act = () => fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest());

        // Assert
        var failure = await act.Should().ThrowAsync<UnauthorizedAccessException>();
        failure.Which.Should().BeSameAs(error);
        expected.Status.Should().Be(ExpectedTransactionStatus.Pending);
        expected.ActualTransactionId.Should().BeNull();
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) A concurrency exception produces Conflict without exposing a successful actual transaction.<br />
    ///     (VI) Ngoại lệ cạnh tranh trả Conflict mà không lộ giao dịch thực thành công.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ShouldReturnConflict_WhenPeriodUpdateHasConcurrencyConflict()
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        fixture.ExpectedRepository.Setup(repository => repository.UpdateAsync(It.IsAny<ExpectedTransaction>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Stale period"));

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest());

        // Assert
        result.Outcome.Should().Be(ExpectedTransactionActionOutcome.Conflict);
        result.ActualTransaction.Should().BeNull();
        result.ActualTransactionId.Should().BeNull();
    }

    /// <summary>
    ///     (EN) Only an actual-link unique violation is Conflict; other constraints and SQL states are persistence errors.<br />
    ///     (VI) Chỉ vi phạm unique liên kết thực là Conflict; constraint khác và SQL state khác là lỗi lưu trữ.
    /// </summary>
    [Theory]
    [InlineData(PostgresErrorCodes.UniqueViolation, "ux_expected_actual_transaction", ExpectedTransactionActionOutcome.Conflict)]
    [InlineData(PostgresErrorCodes.UniqueViolation, "ux_other_constraint", ExpectedTransactionActionOutcome.PersistenceError)]
    [InlineData(PostgresErrorCodes.ForeignKeyViolation, "ux_expected_actual_transaction", ExpectedTransactionActionOutcome.PersistenceError)]
    public async Task RecordPaymentAsync_ShouldClassifyDatabaseException_WhenConstraintOrSqlStateDiffers(
        string sqlState, string constraint, ExpectedTransactionActionOutcome outcome)
    {
        // Arrange
        var expected = Expected(1);
        var fixture = CreateFixture(expected);
        var postgresError = new PostgresException("Write rejected", "ERROR", "ERROR", sqlState,
            constraintName: constraint);
        fixture.ExpectedRepository.Setup(repository => repository.UpdateAsync(It.IsAny<ExpectedTransaction>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Write failed", postgresError));

        // Act
        var result = await fixture.Service.RecordPaymentAsync(expected.Id, new RecordPaymentRequest());

        // Assert
        result.Outcome.Should().Be(outcome);
        result.ActualTransaction.Should().BeNull();
        result.ActualTransactionId.Should().BeNull();
    }
}
