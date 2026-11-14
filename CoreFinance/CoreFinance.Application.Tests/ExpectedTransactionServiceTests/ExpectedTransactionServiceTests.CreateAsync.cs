using System.Globalization;
using System.Linq.Expressions;
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

    private static ExpectedTransactionCreateRequest InvalidCreateRequest(string kind)
    {
        var request = new ExpectedTransactionCreateRequest { ExpectedDate = Today, ExpectedAmount = 100m };
        switch (kind)
        {
            case "emptyTemplate": request.RecurringTransactionTemplateId = Guid.Empty; break;
            case "emptyAccount": request.AccountId = Guid.Empty; break;
            case "terminalStatus": request.Status = ExpectedTransactionStatus.Confirmed; break;
            case "zeroAmount": request.ExpectedAmount = 0; break;
            case "defaultDate": request.ExpectedDate = default; break;
            default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }

        return request;
    }

    /// <summary>
    ///     (EN) Characterization: single creation reports the first validator failure message and writes nothing.<br />
    ///     (VI) Đặc tả hiện trạng: tạo đơn lẻ trả message lỗi validator đầu tiên và không ghi gì.
    /// </summary>
    // The default-message literals below were captured under en-US from FluentValidation 12.0.0.
    [Theory]
    [InlineData("emptyTemplate", "RecurringTransactionTemplateId", CoreFinanceMessages.TemplateIdMustNotBeEmpty)]
    [InlineData("emptyAccount", "AccountId", CoreFinanceMessages.AccountIdMustNotBeEmpty)]
    [InlineData("terminalStatus", "Status", CoreFinanceMessages.NewExpectedTransactionsMustBePending)]
    [InlineData("zeroAmount", "ExpectedAmount", "'Expected Amount' must be greater than '0'.")]
    [InlineData("defaultDate", "ExpectedDate", "'Expected Date' must not be empty.")]
    public async Task CreateAsync_ShouldReportFirstValidatorMessageWithoutWriting_WhenRequestIsInvalid(
        string kind, string propertyName, string message)
    {
        // Arrange
        var fixture = CreateFixture();
        var request = InvalidCreateRequest(kind);
        var previousCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("en-US");

        try
        {
            // Act
            Func<Task> act = () => fixture.Service.CreateAsync(request);

            // Assert
            var failure = (await act.Should().ThrowAsync<ValidationException>()).Which;
            failure.Errors.First().PropertyName.Should().Be(propertyName);
            failure.Errors.First().ErrorMessage.Should().Be(message);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }

        fixture.ExpectedRepository.Verify(r => r.CreateAsync(It.IsAny<ExpectedTransaction>(),
            It.IsAny<CancellationToken>()), Times.Never);
        fixture.ExpectedRepository.Verify(r => r.CreateAsync(It.IsAny<List<ExpectedTransaction>>()), Times.Never);
        fixture.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.CreatedExpectedTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Characterization: batch creation reports the failing item's first validator message and writes nothing.<br />
    ///     (VI) Đặc tả hiện trạng: tạo batch trả message lỗi validator đầu tiên của mục sai và không ghi gì.
    /// </summary>
    // The default-message literals below were captured under en-US from FluentValidation 12.0.0.
    [Theory]
    [InlineData("emptyTemplate", "RecurringTransactionTemplateId", CoreFinanceMessages.TemplateIdMustNotBeEmpty)]
    [InlineData("emptyAccount", "AccountId", CoreFinanceMessages.AccountIdMustNotBeEmpty)]
    [InlineData("terminalStatus", "Status", CoreFinanceMessages.NewExpectedTransactionsMustBePending)]
    [InlineData("zeroAmount", "ExpectedAmount", "'Expected Amount' must be greater than '0'.")]
    [InlineData("defaultDate", "ExpectedDate", "'Expected Date' must not be empty.")]
    public async Task CreateAsync_ShouldReportFirstValidatorMessageWithoutWriting_WhenSecondBatchItemIsInvalid(
        string kind, string propertyName, string message)
    {
        // Arrange
        var fixture = CreateFixture();
        var requests = ValidBatch();
        requests[1] = InvalidCreateRequest(kind);
        var previousCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("en-US");

        try
        {
            // Act
            Func<Task> act = () => fixture.Service.CreateAsync(requests);

            // Assert
            var failure = (await act.Should().ThrowAsync<ValidationException>()).Which;
            failure.Errors.First().PropertyName.Should().Be(BatchItemKey(1, propertyName));
            failure.Errors.First().ErrorMessage.Should().Be(message);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }

        fixture.ExpectedRepository.Verify(r => r.CreateAsync(It.IsAny<List<ExpectedTransaction>>()), Times.Never);
        fixture.ExpectedRepository.Verify(r => r.CreateAsync(It.IsAny<ExpectedTransaction>(),
            It.IsAny<CancellationToken>()), Times.Never);
        fixture.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.CreatedExpectedTransactions.Should().BeEmpty();
    }

    // FluentValidation 12.0.0 prefixes a collection rule built with RuleForEach(x => x) by the lambda parameter name,
    // so the key of the second item's Amount is "x[1].ExpectedAmount" (observed, not assumed).
    private static string BatchItemKey(int index, string property) => $"x[{index}].{property}";

    /// <summary>
    ///     (EN) With several invalid batch items, every invalid item is reported with its index in one exception, in item order.<br />
    ///     (VI) Khi nhiều mục batch sai, mọi mục sai được trả kèm chỉ số trong một ngoại lệ, theo thứ tự mục.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldReportEveryInvalidItem_WhenSeveralBatchItemsAreInvalid()
    {
        // Arrange
        var fixture = CreateFixture();
        var requests = new List<ExpectedTransactionCreateRequest>
        {
            InvalidCreateRequest("emptyAccount"), InvalidCreateRequest("terminalStatus")
        };

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(requests);

        // Assert
        var failure = (await act.Should().ThrowAsync<ValidationException>()).Which;
        failure.Errors.Select(error => (error.PropertyName, error.ErrorMessage)).Should().Equal(
            (BatchItemKey(0, "AccountId"), CoreFinanceMessages.AccountIdMustNotBeEmpty),
            (BatchItemKey(1, "Status"), CoreFinanceMessages.NewExpectedTransactionsMustBePending));
        fixture.ExpectedRepository.Verify(r => r.CreateAsync(It.IsAny<List<ExpectedTransaction>>()), Times.Never);
        fixture.CreatedExpectedTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) A batch above the size limit reports exactly one limit error even when its items are invalid, and writes nothing.<br />
    ///     (VI) Batch vượt giới hạn kích thước chỉ trả đúng một lỗi giới hạn dù các mục sai, và không ghi gì.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldReportOnlyBatchLimitWithoutWriting_WhenBatchExceedsMaxSize()
    {
        // Arrange
        var fixture = CreateFixture();
        var requests = Enumerable.Range(0, CoreFinanceMessages.MaxBatchSize + 1)
            .Select(_ => InvalidCreateRequest("zeroAmount")).ToList();

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(requests);

        // Assert
        var failure = (await act.Should().ThrowAsync<ValidationException>()).Which;
        failure.Errors.Should().ContainSingle().Which.ErrorMessage.Should().Be(CoreFinanceMessages.BatchSizeExceeded);
        fixture.AccountRepository.Verify(r => r.GetNoTrackingEntities(It.IsAny<Expression<Func<Account, object>>[]>()),
            Times.Never);
        fixture.ExpectedRepository.Verify(r => r.CreateAsync(It.IsAny<List<ExpectedTransaction>>()), Times.Never);
        fixture.UnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.CreatedExpectedTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) A valid batch of exactly the maximum size is created.<br />
    ///     (VI) Batch hợp lệ đúng bằng kích thước tối đa được tạo.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldCreateAllPeriods_WhenBatchHasExactlyMaxSize()
    {
        // Arrange
        var fixture = CreateFixture();
        var requests = Enumerable.Range(0, CoreFinanceMessages.MaxBatchSize)
            .Select(offset => new ExpectedTransactionCreateRequest
            {
                AccountId = AccountId, ExpectedDate = Today.AddDays(offset), ExpectedAmount = 10m
            }).ToList();

        // Act
        var result = await fixture.Service.CreateAsync(requests);

        // Assert
        result.Should().HaveCount(CoreFinanceMessages.MaxBatchSize);
        fixture.CreatedExpectedTransactions.Should().HaveCount(CoreFinanceMessages.MaxBatchSize);
    }

    /// <summary>
    ///     (EN) Format validation of the whole batch runs before any reference lookup: a missing account on item 0 does not hide a
    ///     format error on item 1, and no reference query is executed.<br />
    ///     (VI) Kiểm tra định dạng cả batch chạy trước mọi truy vấn tham chiếu: item 0 thiếu account không che lỗi định dạng của
    ///     item 1, và không truy vấn tham chiếu nào được thực thi.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldThrowValidationWithoutReferenceQuery_WhenFirstItemReferenceIsMissingAndSecondFormatIsInvalid()
    {
        // Arrange
        var fixture = CreateFixture();
        var requests = ValidBatch();
        requests[0].AccountId = Id(50);
        requests[1].ExpectedAmount = 0;

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(requests);

        // Assert
        var failure = (await act.Should().ThrowAsync<ValidationException>()).Which;
        failure.Errors.Select(error => error.PropertyName).Should().Equal(BatchItemKey(1, "ExpectedAmount"));
        fixture.AccountRepository.Verify(r => r.GetNoTrackingEntities(It.IsAny<Expression<Func<Account, object>>[]>()),
            Times.Never);
        fixture.TemplateRepository.Verify(
            r => r.GetNoTrackingEntities(It.IsAny<Expression<Func<RecurringTransactionTemplate, object>>[]>()),
            Times.Never);
        fixture.ExpectedRepository.Verify(r => r.CreateAsync(It.IsAny<List<ExpectedTransaction>>()), Times.Never);
        fixture.CreatedExpectedTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Reference lookup for a batch is set-based: many items sharing one account query the Account source once and
    ///     the Template source at most once, instead of once per item.<br />
    ///     (VI) Tra cứu tham chiếu cho batch theo tập: nhiều mục dùng chung một account chỉ truy vấn nguồn Account một lần và
    ///     nguồn Template tối đa một lần, thay vì mỗi mục một lần.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldQueryAccountSourceOnce_WhenManyBatchItemsShareOneAccount()
    {
        // Arrange
        var fixture = CreateFixture();
        var requests = Enumerable.Range(0, 5)
            .Select(offset => new ExpectedTransactionCreateRequest
            {
                AccountId = AccountId,
                RecurringTransactionTemplateId = TemplateId,
                ExpectedDate = Today.AddDays(offset),
                ExpectedAmount = 10m + offset
            }).ToList();

        // Act
        var result = await fixture.Service.CreateAsync(requests);

        // Assert
        result.Should().HaveCount(5);
        fixture.AccountRepository.Verify(r => r.GetNoTrackingEntities(It.IsAny<Expression<Func<Account, object>>[]>()),
            Times.Once);
        fixture.TemplateRepository.Verify(
            r => r.GetNoTrackingEntities(It.IsAny<Expression<Func<RecurringTransactionTemplate, object>>[]>()),
            Times.AtMostOnce);
    }

    /// <summary>
    ///     (EN) Reference exception order is unchanged: items are walked in order, so a missing template on item 0 wins over
    ///     a missing account on item 1.<br />
    ///     (VI) Thứ tự ngoại lệ tham chiếu không đổi: duyệt mục theo thứ tự nên template thiếu ở mục 0 được báo trước account
    ///     thiếu ở mục 1.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldReportFirstItemTemplateNotFound_WhenFirstTemplateAndSecondAccountAreMissing()
    {
        // Arrange
        var fixture = CreateFixture();
        var requests = ValidBatch();
        requests[0].AccountId = null;
        requests[0].RecurringTransactionTemplateId = Id(50);
        requests[1].AccountId = Id(50);

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(requests);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage(CoreFinanceMessages.RecurringTransactionTemplateNotFound);
        fixture.CreatedExpectedTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) Within one item the account is checked before the template, so an item missing both reports the account.<br />
    ///     (VI) Trong cùng một mục, account được kiểm tra trước template nên mục thiếu cả hai báo account.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldReportAccountNotFound_WhenSameBatchItemMissesAccountAndTemplate()
    {
        // Arrange
        var fixture = CreateFixture();
        var requests = ValidBatch();
        requests[0].AccountId = Id(50);
        requests[0].RecurringTransactionTemplateId = Id(51);

        // Act
        Func<Task> act = () => fixture.Service.CreateAsync(requests);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage(CoreFinanceMessages.AccountNotFound);
        fixture.CreatedExpectedTransactions.Should().BeEmpty();
    }
}
