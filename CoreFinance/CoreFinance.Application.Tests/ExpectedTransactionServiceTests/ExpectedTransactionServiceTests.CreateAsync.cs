using CoreFinance.Application.DTOs.ExpectedTransaction;
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
    ///     (EN) Optional references remain nullable through creation and reads, and every created period starts pending.<br />
    ///     (VI) Tham chiếu tùy chọn giữ null qua tạo và đọc; mọi kỳ mới bắt đầu ở trạng thái chờ.
    /// </summary>
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task CreateAsync_ShouldPreserveNullableReferencesAndPendingState_WhenReferencesAreOptional(
        bool account, bool template)
    {
        // Arrange
        var fixture = CreateFixture();
        var request = new ExpectedTransactionCreateRequest
        {
            AccountId = account ? AccountId : null,
            RecurringTransactionTemplateId = template ? TemplateId : null,
            ExpectedDate = Today.AddDays(7),
            ExpectedAmount = 100.50m,
            TransactionType = RecurringTransactionType.Expense,
            Description = "Monthly subscription"
        };

        // Act
        var result = await fixture.Service.CreateAsync(request);
        var view = await fixture.Service.GetByIdAsync(result!.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        view.Should().NotBeNull();
        view!.Status.Should().Be(ExpectedTransactionStatus.Pending);
        view.DisplayStatus.Should().Be(0);
        view.DaysUntilDue.Should().Be(7);
        view.AccountId.Should().Be(account ? AccountId : null);
        view.RecurringTransactionTemplateId.Should().Be(template ? TemplateId : null);
        view.ActualTransactionId.Should().BeNull();
        view.ProcessedAt.Should().BeNull();
        view.IsAdjusted.Should().BeFalse();
        view.OriginalAmount.Should().BeNull();
        fixture.CreatedExpectedTransactions.Single().AccountId.Should().Be(account ? AccountId : null);
        fixture.CreatedExpectedTransactions.Single().RecurringTransactionTemplateId.Should()
            .Be(template ? TemplateId : null);
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Terminal creation requests cannot bypass the command lifecycle.<br />
    ///     (VI) Yêu cầu tạo trạng thái kết thúc không được vượt qua vòng đời command.
    /// </summary>
    [Theory]
    [InlineData(ExpectedTransactionStatus.Confirmed)]
    [InlineData(ExpectedTransactionStatus.Cancelled)]
    [InlineData(ExpectedTransactionStatus.Completed)]
    public async Task CreateAsync_ShouldRejectTerminalStatus_WhenStatusIsNotPending(ExpectedTransactionStatus status)
    {
        // Arrange
        var fixture = CreateFixture();
        var request = new ExpectedTransactionCreateRequest
        {
            ExpectedDate = Today, ExpectedAmount = 100m, Status = status
        };

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        fixture.CreatedExpectedTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Explicit empty reference IDs are invalid rather than being silently converted to null.<br />
    ///     (VI) ID tham chiếu rỗng được truyền rõ ràng không hợp lệ, không được âm thầm đổi thành null.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateAsync_ShouldRejectEmptyReferenceId_WhenReferenceIsProvided(bool account)
    {
        // Arrange
        var fixture = CreateFixture();
        var request = new ExpectedTransactionCreateRequest
        {
            ExpectedDate = Today,
            ExpectedAmount = 100m,
            AccountId = account ? Guid.Empty : null,
            RecurringTransactionTemplateId = account ? null : Guid.Empty
        };

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        fixture.CreatedExpectedTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) An unavailable reference is rejected by the service guard; the mock makes no ownership claim.<br />
    ///     (VI) Guard dịch vụ từ chối tham chiếu không được trả về; mock không chứng minh quyền sở hữu.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateAsync_ShouldRejectUnavailableReference_WhenAccountOrTemplateIsNotReturned(bool account)
    {
        // Arrange
        var fixture = CreateFixture();
        var request = new ExpectedTransactionCreateRequest
        {
            ExpectedDate = Today,
            ExpectedAmount = 100m,
            AccountId = account ? Id(50) : null,
            RecurringTransactionTemplateId = account ? null : Id(50)
        };

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
        fixture.CreatedExpectedTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) New expected amounts must be positive.<br />
    ///     (VI) Số tiền kỳ mới bắt buộc dương.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateAsync_ShouldRejectNonPositiveAmount_WhenAmountIsInvalid(int amount)
    {
        // Arrange
        var fixture = CreateFixture();
        var request = new ExpectedTransactionCreateRequest { ExpectedDate = Today, ExpectedAmount = amount };

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        fixture.CreatedExpectedTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) A zero affected-row result retains the existing create-failed exception contract.<br />
    ///     (VI) Không có bản ghi bị ảnh hưởng giữ hợp đồng ngoại lệ tạo thất bại hiện hành.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldThrowCreateFailedException_WhenRepositoryReportsNoCreation()
    {
        // Arrange
        var fixture = CreateFixture();
        fixture.ExpectedRepository.Setup(r => r.CreateAsync(It.IsAny<ExpectedTransaction>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        var request = new ExpectedTransactionCreateRequest { ExpectedDate = Today, ExpectedAmount = 100m };

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<CreateFailedException>();
    }

    /// <summary>
    ///     (EN) Persistence exceptions propagate rather than being converted to a successful creation.<br />
    ///     (VI) Ngoại lệ lưu trữ được truyền ra thay vì đổi thành tạo thành công.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldPropagatePersistenceFailure_WhenInsertFails()
    {
        // Arrange
        var fixture = CreateFixture();
        var error = new DbUpdateException("Insert failed");
        fixture.ExpectedRepository.Setup(r => r.CreateAsync(It.IsAny<ExpectedTransaction>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(error);
        var request = new ExpectedTransactionCreateRequest { ExpectedDate = Today, ExpectedAmount = 100m };

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    /// <summary>
    ///     (EN) A failed commit keeps the create exception contract; undoing inserts is a real-database concern.<br />
    ///     (VI) Commit lỗi giữ hợp đồng ngoại lệ khi tạo; hoàn tác insert cần cơ sở dữ liệu thật.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldPropagateCommitFailure_WhenCommitFails()
    {
        // Arrange
        var fixture = CreateFixture();
        var error = new InvalidOperationException("Commit failed");
        fixture.Transaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(error);
        var request = new ExpectedTransactionCreateRequest { ExpectedDate = Today, ExpectedAmount = 100m };

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static List<ExpectedTransactionCreateRequest> ValidBatch() =>
    [
        new()
        {
            AccountId = AccountId,
            RecurringTransactionTemplateId = TemplateId,
            ExpectedDate = Today.AddDays(7),
            ExpectedAmount = 150.75m,
            TransactionType = RecurringTransactionType.Expense,
            Status = ExpectedTransactionStatus.Pending
        },
        new()
        {
            ExpectedDate = Today.AddDays(-1),
            ExpectedAmount = 25.50m,
            TransactionType = RecurringTransactionType.Income,
            Status = ExpectedTransactionStatus.Pending
        }
    ];

    /// <summary>
    ///     (EN) Batch creation returns only the new identities, ordered by date rather than request or repository order.<br />
    ///     (VI) Tạo batch chỉ trả các ID mới, sắp theo ngày thay vì thứ tự yêu cầu hay repository.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldReturnOnlyCreatedPeriodsInDateOrder_WhenBatchIsValid()
    {
        // Arrange
        var fixture = CreateFixture(Expected(10, -30));
        var requests = ValidBatch();

        // Act
        var result = (await fixture.Service.CreateAsync(requests))!.ToList();

        // Assert
        result.Select(period => period.Id).Should().Equal(Id(1001), Id(1000));
        result.Select(period => period.ExpectedDate).Should().Equal(Today.AddDays(-1), Today.AddDays(7));
        result.Select(period => period.ExpectedAmount).Should().Equal(25.50m, 150.75m);
        result.Should().OnlyContain(period => period.Status == ExpectedTransactionStatus.Pending &&
            period.ActualTransactionId == null && period.ProcessedAt == null);
        result[0].AccountId.Should().BeNull();
        result[0].RecurringTransactionTemplateId.Should().BeNull();
        result[1].AccountId.Should().Be(AccountId);
        result[1].RecurringTransactionTemplateId.Should().Be(TemplateId);
        fixture.CreatedActualTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Empty batches return no periods and do not accidentally return existing rows.<br />
    ///     (VI) Batch rỗng không trả kỳ nào và không trả nhầm các bản ghi hiện có.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldReturnEmptyWithoutCreatingPeriods_WhenBatchIsEmpty()
    {
        // Arrange
        var fixture = CreateFixture(Expected(10));
        List<ExpectedTransactionCreateRequest> requests = [];

        // Act
        var result = await fixture.Service.CreateAsync(requests);

        // Assert
        result.Should().BeEmpty();
        fixture.CreatedExpectedTransactions.Should().BeEmpty();
        fixture.ExpectedTransactions.Select(period => period.Id).Should().Equal(Id(10));
    }

    /// <summary>
    ///     (EN) An unavailable reference on the second item rejects the entire input batch before any insert is accepted.<br />
    ///     (VI) Tham chiếu không được trả về ở mục thứ hai từ chối toàn bộ đầu vào batch trước khi chấp nhận insert.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateAsync_ShouldRejectWholeBatchBeforeCreation_WhenSecondReferenceIsUnavailable(bool account)
    {
        // Arrange
        var fixture = CreateFixture(Expected(10));
        var requests = ValidBatch();
        requests[1].AccountId = account ? Id(50) : null;
        requests[1].RecurringTransactionTemplateId = account ? null : Id(50);

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(requests);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
        fixture.CreatedExpectedTransactions.Should().BeEmpty();
        fixture.ExpectedTransactions.Select(period => period.Id).Should().Equal(Id(10));
    }

    /// <summary>
    ///     (EN) A terminal second item cannot create the valid first item as a partial success.<br />
    ///     (VI) Mục thứ hai có trạng thái kết thúc không được tạo mục đầu hợp lệ thành kết quả thành công một phần.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldRejectWholeBatchBeforeCreation_WhenSecondStatusIsTerminal()
    {
        // Arrange
        var fixture = CreateFixture(Expected(10));
        var requests = ValidBatch();
        requests[1].Status = ExpectedTransactionStatus.Confirmed;

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(requests);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        fixture.CreatedExpectedTransactions.Should().BeEmpty();
        fixture.ExpectedTransactions.Select(period => period.Id).Should().Equal(Id(10));
    }

    /// <summary>
    ///     (EN) A batch insert affecting zero rows fails instead of returning created periods.<br />
    ///     (VI) Insert batch không ảnh hưởng bản ghi nào thất bại thay vì trả các kỳ đã tạo.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldThrowCreateFailedException_WhenBatchRepositoryReportsNoCreation()
    {
        // Arrange
        var fixture = CreateFixture();
        fixture.ExpectedRepository.Setup(repository => repository.CreateAsync(It.IsAny<List<ExpectedTransaction>>()))
            .ReturnsAsync(0);
        var requests = ValidBatch();

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(requests);

        // Assert
        await act.Should().ThrowAsync<CreateFailedException>();
    }

    /// <summary>
    ///     (EN) A batch save failure propagates; mocked rows cannot establish durable transaction rollback.<br />
    ///     (VI) Lỗi lưu batch được truyền ra; các dòng mock không chứng minh rollback transaction bền vững.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldPropagatePersistenceFailure_WhenBatchSaveFails()
    {
        // Arrange
        var fixture = CreateFixture();
        var error = new DbUpdateException("Batch save failed");
        fixture.UnitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(error);
        var requests = ValidBatch();

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(requests);

        // Assert
        var failure = await act.Should().ThrowAsync<DbUpdateException>();
        failure.Which.Should().BeSameAs(error);
    }
}
