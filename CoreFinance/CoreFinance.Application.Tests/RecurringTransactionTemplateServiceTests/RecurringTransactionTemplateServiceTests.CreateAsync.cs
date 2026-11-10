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
    ///     (EN) An invalid request is rejected before any transaction, lock, account lookup or write.<br />
    ///     (VI) Yêu cầu không hợp lệ bị từ chối trước mọi transaction, khóa, tra cứu tài khoản hay ghi.
    /// </summary>
    [Theory]
    [InlineData("", 300, 30, 0, nameof(RecurringTransactionTemplateCreateRequest.Name))]
    [InlineData(null, 300, 30, 0, nameof(RecurringTransactionTemplateCreateRequest.Name))]
    [InlineData("Internet", 0, 30, 0, nameof(RecurringTransactionTemplateCreateRequest.Amount))]
    [InlineData("Internet", 300, 0, 0, nameof(RecurringTransactionTemplateCreateRequest.DaysInAdvance))]
    [InlineData("Internet", 300, 30, 99, nameof(RecurringTransactionTemplateCreateRequest.Frequency))]
    public async Task CreateAsync_ShouldThrowValidationExceptionWithoutWriting_WhenRequestIsInvalid(
        string? name, int amount, int daysInAdvance, int frequency, string invalidProperty)
    {
        // Arrange
        var harness = new GenerationHarness();
        var request = NewCreateRequest();
        request.Name = name;
        request.Amount = amount;
        request.DaysInAdvance = daysInAdvance;
        request.Frequency = (RecurrenceFrequency)frequency;

        // Act
        var act = () => harness.Service().CreateAsync(request);

        // Assert
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
