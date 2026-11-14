using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using Moq;
using Shared.Contracts.Exceptions;
using EntityNotFoundException = Shared.Contracts.Exceptions.EntityNotFoundException;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

/// <summary>
///     Contains test cases for the CreateAsync method of RecurringTransactionTemplateService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho phương thức CreateAsync của RecurringTransactionTemplateService. (VI)
/// </summary>
public partial class RecurringTransactionTemplateServiceTests
{
    private static RecurringTransactionTemplateCreateRequest NewCreateRequest(bool autoGenerate = true) => new()
    {
        AccountId = GenerationHarness.AccountId,
        Name = "Internet",
        Amount = 300m,
        TransactionType = RecurringTransactionType.Expense,
        Frequency = RecurrenceFrequency.Monthly,
        StartDate = D(2026, 3, 1),
        AutoGenerate = autoGenerate,
        DaysInAdvance = 30
    };

    /// <summary>
    ///     (EN) Creating an auto-generate template starts its first window at StartDate and returns the next slot.<br />
    ///     (VI) Tạo mẫu tự sinh bắt đầu cửa sổ đầu tiên tại StartDate và trả slot tiếp theo.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldGenerateFirstWindow_FromStartDate()
    {
        // Arrange
        var harness = new GenerationHarness();

        // Act
        var result = await harness.Service(D(2026, 3, 15)).CreateAsync(NewCreateRequest());

        // Assert
        harness.CreatedDates.Should().Equal(D(2026, 3, 1), D(2026, 4, 1));
        harness.Created.Should().OnlyContain(e => e.RecurringTransactionTemplateId == result!.Id);
        harness.Template!.NextExecutionDate.Should().Be(D(2026, 5, 1));
        result!.NextExecutionDate.Should().Be(D(2026, 5, 1));
    }

    /// <summary>
    ///     (EN) A template without auto-generate is created with no periods.<br />
    ///     (VI) Mẫu không tự sinh được tạo mà không có kỳ nào.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldCreateNoPeriods_WhenAutoGenerateIsFalse()
    {
        // Arrange
        var harness = new GenerationHarness();

        // Act
        var result = await harness.Service().CreateAsync(NewCreateRequest(false));

        // Assert
        result.Should().NotBeNull();
        harness.Created.Should().BeEmpty();
        result!.NextExecutionDate.Should().Be(D(2026, 3, 1));
    }

    /// <summary>
    ///     (EN) An account that is missing or owned by someone else is rejected before the template is created.<br />
    ///     (VI) Tài khoản không tồn tại hoặc thuộc người khác bị từ chối trước khi tạo mẫu.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldReject_WhenAccountIsNotOwned()
    {
        // Arrange
        var harness = new GenerationHarness();
        harness.OwnedAccounts.Clear();

        // Act
        var act = () => harness.Service().CreateAsync(NewCreateRequest());

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
        harness.Template.Should().BeNull();
        harness.Created.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) When the template row is not written, creation fails and rolls back without generating.<br />
    ///     (VI) Khi không ghi được dòng mẫu, việc tạo thất bại và rollback mà không sinh kỳ.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldThrowCreateFailedException_WhenRepositoryReturnsZeroAffectedCount()
    {
        // Arrange
        var harness = new GenerationHarness();
        harness.TemplateRepo
            .Setup(r => r.CreateAsync(It.IsAny<RecurringTransactionTemplate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var act = () => harness.Service().CreateAsync(NewCreateRequest());

        // Assert
        await act.Should().ThrowAsync<CreateFailedException>();
        harness.Created.Should().BeEmpty();
    }

    /// <summary>
    ///     (EN) A supplied cursor skips earlier StartDate slots and generates only its remaining first window.<br />
    ///     (VI) Cursor được truyền bỏ qua các kỳ StartDate trước đó và chỉ sinh phần còn lại của cửa sổ đầu tiên.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldGenerateFromSuppliedCursor_WhenCursorDiffersFromStartDate()
    {
        // Arrange
        var harness = new GenerationHarness();
        var request = NewCreateRequest();
        request.NextExecutionDate = D(2026, 4, 1);

        // Act
        var result = await harness.Service().CreateAsync(request);

        // Assert
        harness.CreatedDates.Should().Equal(D(2026, 4, 1));
        harness.Created.Should().OnlyContain(period => period.RecurringTransactionTemplateId == result!.Id);
        harness.Template!.StartDate.Should().Be(D(2026, 3, 1));
        harness.Template.NextExecutionDate.Should().Be(D(2026, 5, 1));
        result!.NextExecutionDate.Should().Be(D(2026, 5, 1));
    }

    /// <summary>
    ///     (EN) A blank name is rejected before any transaction, lock, account lookup or write.<br />
    ///     (VI) Tên trống bị từ chối trước mọi transaction, khóa, tra cứu tài khoản hay ghi.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task CreateAsync_ShouldThrowValidationExceptionWithoutWriting_WhenNameIsBlank(string? name)
    {
        // Arrange
        var request = NewCreateRequest();
        request.Name = name;

        // Act and Assert
        await AssertRejectedWithoutWritingAsync(request, nameof(RecurringTransactionTemplateCreateRequest.Name));
    }

    /// <summary>
    ///     (EN) An out-of-range amount, frequency, type, custom interval or advance window is rejected before any
    ///     transaction, lock, account lookup or write; only the named rule is broken in each case.<br />
    ///     (VI) Số tiền, tần suất, loại, chu kỳ tùy chỉnh hoặc số ngày sinh trước nằm ngoài miền hợp lệ bị từ chối
    ///     trước mọi transaction, khóa, tra cứu tài khoản hay ghi; mỗi trường hợp chỉ vi phạm đúng một rule.
    /// </summary>
    [Theory]
    [InlineData(nameof(RecurringTransactionTemplateCreateRequest.Amount), 0)]
    [InlineData(nameof(RecurringTransactionTemplateCreateRequest.DaysInAdvance), 0)]
    [InlineData(nameof(RecurringTransactionTemplateCreateRequest.DaysInAdvance), 366)]
    [InlineData(nameof(RecurringTransactionTemplateCreateRequest.Frequency), 99)]
    [InlineData(nameof(RecurringTransactionTemplateCreateRequest.TransactionType), 99)]
    [InlineData(nameof(RecurringTransactionTemplateCreateRequest.CustomIntervalDays), 0)]
    [InlineData(nameof(RecurringTransactionTemplateCreateRequest.CustomIntervalDays), -1)]
    public async Task CreateAsync_ShouldThrowValidationExceptionWithoutWriting_WhenNumericFieldIsOutOfRange(
        string invalidProperty, int value)
    {
        // Arrange
        var request = NewCreateRequest();
        switch (invalidProperty)
        {
            case nameof(RecurringTransactionTemplateCreateRequest.Amount): request.Amount = value; break;
            case nameof(RecurringTransactionTemplateCreateRequest.DaysInAdvance): request.DaysInAdvance = value; break;
            case nameof(RecurringTransactionTemplateCreateRequest.Frequency):
                request.Frequency = (RecurrenceFrequency)value;
                break;
            case nameof(RecurringTransactionTemplateCreateRequest.TransactionType):
                request.TransactionType = (RecurringTransactionType)value;
                break;
            case nameof(RecurringTransactionTemplateCreateRequest.CustomIntervalDays):
                request.CustomIntervalDays = value;
                break;
            default: throw new ArgumentOutOfRangeException(nameof(invalidProperty), invalidProperty, null);
        }

        // Act and Assert
        await AssertRejectedWithoutWritingAsync(request, invalidProperty);
    }

    /// <summary>
    ///     (EN) An empty account ID is rejected before any transaction, lock, account lookup or write.<br />
    ///     (VI) ID tài khoản rỗng bị từ chối trước mọi transaction, khóa, tra cứu tài khoản hay ghi.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldThrowValidationExceptionWithoutWriting_WhenAccountIdIsEmpty()
    {
        // Arrange
        var request = NewCreateRequest();
        request.AccountId = Guid.Empty;

        // Act and Assert
        await AssertRejectedWithoutWritingAsync(request, nameof(RecurringTransactionTemplateCreateRequest.AccountId));
    }

    /// <summary>
    ///     (EN) An unset start date is rejected before any transaction, lock, account lookup or write.<br />
    ///     (VI) Ngày bắt đầu chưa đặt bị từ chối trước mọi transaction, khóa, tra cứu tài khoản hay ghi.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldThrowValidationExceptionWithoutWriting_WhenStartDateIsUnset()
    {
        // Arrange
        var request = NewCreateRequest();
        request.StartDate = default;

        // Act and Assert
        await AssertRejectedWithoutWritingAsync(request, nameof(RecurringTransactionTemplateCreateRequest.StartDate));
    }

    private static async Task AssertRejectedWithoutWritingAsync(RecurringTransactionTemplateCreateRequest request,
        string invalidProperty)
    {
        var harness = new GenerationHarness();

        var act = () => harness.Service().CreateAsync(request);

        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().Contain(error => error.PropertyName == invalidProperty);
        harness.UnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        harness.UnitOfWork.Verify(u => u.AcquireOwnerResourceLockAsync(It.IsAny<string>(), It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
        harness.AccountRepo.Verify(r => r.GetNoTrackingEntities(), Times.Never);
        harness.TemplateRepo.Verify(r => r.CreateAsync(It.IsAny<RecurringTransactionTemplate>(),
            It.IsAny<CancellationToken>()), Times.Never);
        harness.Created.Should().BeEmpty();
    }
}
