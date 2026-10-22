using AutoMapper;
using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Application.Interfaces;
using CoreFinance.Application.Services.Base;
using CoreFinance.Application.Utilities;
using CoreFinance.Domain.Constants;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Exceptions;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.DTOs;
using Shared.EntityFramework.EntityFrameworkUtilities;

namespace CoreFinance.Application.Services;

/// <inheritdoc cref="IRecurringTransactionTemplateService" />
public class RecurringTransactionTemplateService(
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<RecurringTransactionTemplateService> logger,
    BusinessCalendar businessCalendar,
    IServiceScopeFactory serviceScopeFactory)
    : BaseService<RecurringTransactionTemplate, RecurringTransactionTemplateCreateRequest,
            RecurringTransactionTemplateUpdateRequest, RecurringTransactionTemplateViewModel, Guid>(mapper, unitOfWork,
            logger),
        IRecurringTransactionTemplateService
{
    // Lock namespace shared by template writers and generation; the resulting key must stay recurring:{owner}:{template}.
    private const string TemplateLockResource = "recurring";

    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<IBasePaging<RecurringTransactionTemplateViewModel>?> GetPagingAsync(IFilterBodyRequest request)
    {
        var query = Mapper.ProjectTo<RecurringTransactionTemplateViewModel>(
            _unitOfWork.Repository<RecurringTransactionTemplate, Guid>()
                .GetNoTrackingEntities());

        if (!string.IsNullOrEmpty(request.SearchValue))
            query = query.Where(t => (t.Name != null && t.Name.ToLower().Contains(request.SearchValue.ToLower())) ||
                                     (t.Description != null &&
                                      t.Description.ToLower().Contains(request.SearchValue.ToLower())) ||
                                     (t.Category != null &&
                                      t.Category.ToLower().Contains(request.SearchValue.ToLower())));

        // Clamp pageIndex: ToPagingAsync throws when the page is past the end. Count after Filter, which it applies internally.
        var total = await query.Filter(request.Filter).CountAsync();
        var pageSize = Math.Max(1, request.Pagination?.PageSize ?? 10);
        var lastPage = total > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 1;
        request.Pagination = new Pagination
        {
            PageIndex = Math.Clamp(request.Pagination?.PageIndex ?? 1, 1, lastPage),
            PageSize = pageSize
        };

        return await query.ToPagingAsync(request);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RecurringTransactionTemplateViewModel>> GetActiveTemplatesAsync()
    {
        var query = _unitOfWork.Repository<RecurringTransactionTemplate, Guid>()
            .GetNoTrackingEntities()
            .Where(t => t.IsActive);

        return await Mapper.ProjectTo<RecurringTransactionTemplateViewModel>(query).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RecurringTransactionTemplateViewModel>> GetTemplatesByAccountAsync(Guid accountId)
    {
        var query = _unitOfWork.Repository<RecurringTransactionTemplate, Guid>()
            .GetNoTrackingEntities()
            .Where(t => t.AccountId == accountId);

        return await Mapper.ProjectTo<RecurringTransactionTemplateViewModel>(query).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> GetActiveAutoGenerateTemplateIdsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>()
            .GetNoTrackingEntities()
            .Where(t => t.IsActive && t.AutoGenerate)
            .OrderBy(t => t.Id)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ToggleActiveStatusAsync(Guid templateId, bool isActive,
        CancellationToken cancellationToken = default)
    {
        await using var trans = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _unitOfWork.AcquireOwnerResourceLockAsync(TemplateLockResource, templateId, cancellationToken);
            var template = await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>().GetByIdAsync(templateId, cancellationToken);
            if (template == null)
            {
                await trans.RollbackAsync(CancellationToken.None);
                return false;
            }

            template.IsActive = isActive;
            await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>().UpdateAsync(template, cancellationToken);
            await trans.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await trans.RollbackAsync(CancellationToken.None);
            logger.LogError(ex, "Error toggling active status for template {TemplateId}", templateId);
            return false;
        }
        catch
        {
            await trans.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        await using var trans = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _unitOfWork.AcquireOwnerResourceLockAsync(TemplateLockResource, templateId, cancellationToken);
            var template = await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>().GetByIdAsync(templateId, cancellationToken);
            if (template == null)
            {
                await trans.RollbackAsync(CancellationToken.None);
                return false;
            }

            await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>().DeleteSoftAsync(template);
            await trans.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await trans.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<DateTime> CalculateNextExecutionDateAsync(Guid templateId)
    {
        var template = await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>()
            .GetByIdAsync(templateId);

        if (template == null)
            throw new ArgumentException(CoreFinanceMessages.TemplateNotFound, nameof(templateId));

        return BusinessCalendar.NextSlot(template.StartDate, template.Frequency, template.CustomIntervalDays,
            template.NextExecutionDate);
    }

    /// <inheritdoc />
    public async Task GenerateExpectedTransactionsAsync(Guid templateId, int? daysInAdvance = null,
        CancellationToken cancellationToken = default)
    {
        await using var trans = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _unitOfWork.AcquireOwnerResourceLockAsync(TemplateLockResource, templateId, cancellationToken);
            var template = await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>().GetByIdAsync(templateId, cancellationToken);
            if (template != null)
                await GenerateLockedAsync(template, daysInAdvance ?? template.DaysInAdvance, cancellationToken);
            await trans.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await trans.RollbackAsync(CancellationToken.None);
            if (ex is not OperationCanceledException)
                logger.LogError(ex, "Error generating expected transactions for template {TemplateId}", templateId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task GenerateExpectedTransactionsForAllActiveTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var templateIds = await GetActiveAutoGenerateTemplateIdsAsync(cancellationToken);
        var failed = 0;
        foreach (var templateId in templateIds)
        {
            // Fresh scope per template: a rolled-back template never leaves EF state behind for the next one.
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            try
            {
                await scope.ServiceProvider.GetRequiredService<IRecurringTransactionTemplateService>()
                    .GenerateExpectedTransactionsAsync(templateId, null, cancellationToken);
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                failed++; // already logged by the single-template run
            }
        }

        if (failed > 0)
            throw new InvalidOperationException(
                string.Format(CoreFinanceMessages.GenerationFailuresFormat, failed, templateIds.Count));
    }

    /// <inheritdoc />
    /// <remarks>See <see cref="IRecurringTransactionTemplateService" /> for single-template creation and generation guarantees.</remarks>
    public override async Task<RecurringTransactionTemplateViewModel?> CreateAsync(
        RecurringTransactionTemplateCreateRequest request)
    {
        await using var trans = await _unitOfWork.BeginTransactionAsync();
        try
        {
            await EnsureOwnedAccountAsync(request.AccountId, CancellationToken.None);

            var template = Mapper.Map(request, new RecurringTransactionTemplate());
            template.Id = Guid.CreateVersion7();
            if (template.NextExecutionDate == default) template.NextExecutionDate = template.StartDate;

            await _unitOfWork.AcquireOwnerResourceLockAsync(TemplateLockResource, template.Id);
            if (await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>().CreateAsync(template) == 0)
                throw new CreateFailedException(CoreFinanceMessages.EntityCreateFailed);

            await GenerateLockedAsync(template, template.DaysInAdvance, CancellationToken.None);
            await trans.CommitAsync();
            return Mapper.Map<RecurringTransactionTemplateViewModel>(template);
        }
        catch
        {
            await trans.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    /// <inheritdoc />
    /// <remarks>See <see cref="IRecurringTransactionTemplateService" /> for cursor, atomicity and history-preservation semantics.</remarks>
    public override async Task<RecurringTransactionTemplateViewModel?> UpdateAsync(Guid id,
        RecurringTransactionTemplateUpdateRequest request)
    {
        await using var trans = await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (!id.Equals(request.Id))
                throw new KeyNotFoundException();

            await _unitOfWork.AcquireOwnerResourceLockAsync(TemplateLockResource, id);
            var template = await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>().GetByIdAsync(id)
                           ?? throw new EntityNotFoundException(string.Format(CoreFinanceMessages.EntityNotFoundFormat, id));

            var (oldFrequency, oldInterval, oldStart, oldCursor) =
                (template.Frequency, template.CustomIntervalDays, template.StartDate, template.NextExecutionDate);
            Mapper.Map(request, template);

            var scheduleChanged = template.Frequency != oldFrequency ||
                                  (template.Frequency == RecurrenceFrequency.Custom &&
                                   template.CustomIntervalDays != oldInterval);
            if (template.StartDate.Date != oldStart.Date)
                template.NextExecutionDate = template.StartDate.Date;
            else if (scheduleChanged)
                template.NextExecutionDate = oldCursor.Date < businessCalendar.Today ? oldCursor.Date : businessCalendar.Today;

            if (await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>().UpdateAsync(template) == 0)
                throw new UpdateFailedException(CoreFinanceMessages.EntityUpdateFailed);

            await GenerateLockedAsync(template, template.DaysInAdvance, CancellationToken.None);
            await trans.CommitAsync();
            return Mapper.Map<RecurringTransactionTemplateViewModel>(template);
        }
        catch
        {
            await trans.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    /// <summary>
    ///     (EN) Generates missing periods for a locked, tracked template inside the ambient transaction. Slots come from the
    ///     grid anchored at StartDate, starting at the first slot on or after the cursor. A slot that already has a period
    ///     (any status, soft-deleted included) is kept and skipped. Generation runs through today (UTC+7) + window days and
    ///     EndDate inclusive, with no cap on catch-up, then saves the next unprocessed slot as the cursor.<br />
    ///     (VI) Sinh các kỳ còn thiếu cho mẫu đã khóa và được theo dõi trong transaction hiện hành. Slot lấy từ lưới neo
    ///     StartDate, bắt đầu ở slot đầu tiên từ cursor trở đi. Slot đã có kỳ (mọi trạng thái, kể cả xóa mềm) được giữ và bỏ
    ///     qua. Sinh đến hết hôm nay (UTC+7) + số ngày cửa sổ và EndDate (bao gồm), không giới hạn sinh bù, rồi lưu slot
    ///     chưa xử lý kế tiếp làm cursor.
    /// </summary>
    private async Task GenerateLockedAsync(RecurringTransactionTemplate template, int windowDays,
        CancellationToken cancellationToken)
    {
        if (!template.IsActive || !template.AutoGenerate)
            return;

        await EnsureOwnedAccountAsync(template.AccountId, cancellationToken);

        var windowEnd = businessCalendar.Today.AddDays(windowDays);
        var expectedRepo = _unitOfWork.Repository<ExpectedTransaction, Guid>();
        // Ignore only the soft-delete filter so tombstones keep reserving their slots; the owner predicate remains.
        var known = (await expectedRepo.GetNoTrackingEntities().IgnoreQueryFilters()
                .Where(e => e.RecurringTransactionTemplateId == template.Id)
                .Select(e => e.ExpectedDate)
                .ToListAsync(cancellationToken))
            .Select(d => BusinessCalendar.SlotKey(template.Frequency, d))
            .ToHashSet();

        var slot = BusinessCalendar.FirstSlotOnOrAfter(template.StartDate, template.Frequency,
            template.CustomIntervalDays, template.NextExecutionDate);
        while (slot <= windowEnd && (template.EndDate == null || slot <= template.EndDate.Value.Date))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (known.Add(BusinessCalendar.SlotKey(template.Frequency, slot)))
                await expectedRepo.CreateAsync(new ExpectedTransaction
                {
                    Id = Guid.CreateVersion7(),
                    RecurringTransactionTemplateId = template.Id,
                    AccountId = template.AccountId,
                    ExpectedDate = slot,
                    ExpectedAmount = template.Amount,
                    Description = template.Description,
                    TransactionType = template.TransactionType,
                    Category = template.Category,
                    Status = ExpectedTransactionStatus.Pending,
                    GeneratedAt = DateTime.UtcNow
                }, cancellationToken);

            var next = BusinessCalendar.NextSlot(template.StartDate, template.Frequency, template.CustomIntervalDays,
                slot);
            if (next <= slot)
                throw new InvalidOperationException(string.Format(CoreFinanceMessages.ScheduleDoesNotAdvanceFormat, template.Id));
            slot = next;
        }

        template.NextExecutionDate = slot;
        await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>().UpdateAsync(template, cancellationToken);
    }

    /// <summary>
    ///     (EN) Fails unless the account exists, is not deleted and belongs to the current owner. No account is allowed.<br />
    ///     (VI) Thất bại trừ khi tài khoản tồn tại, chưa xóa và thuộc owner hiện tại. Không có tài khoản thì được phép.
    /// </summary>
    private async Task EnsureOwnedAccountAsync(Guid? accountId, CancellationToken cancellationToken)
    {
        if (accountId is not { } id) return;
        if (!await _unitOfWork.Repository<Account, Guid>().GetNoTrackingEntities()
                .AnyAsync(a => a.Id == id, cancellationToken))
            throw new EntityNotFoundException(string.Format(CoreFinanceMessages.AccountNotFoundFormat, id));
    }
}