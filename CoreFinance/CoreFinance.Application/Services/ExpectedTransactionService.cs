using AutoMapper;
using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.DTOs.Transaction;
using CoreFinance.Application.Enums;
using CoreFinance.Application.Interfaces;
using CoreFinance.Application.Services.Base;
using CoreFinance.Application.Utilities;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.Constants;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.UnitOfWorks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Contracts.Exceptions;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.DTOs;
using Shared.EntityFramework.EntityFrameworkUtilities;
using Shared.EntityFramework.Enums;

namespace CoreFinance.Application.Services;

/// <inheritdoc cref="IExpectedTransactionService" />
public class ExpectedTransactionService(
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<ExpectedTransactionService> logger,
    ITransactionService transactionService,
    BusinessCalendar calendar)
    : BaseService<ExpectedTransaction, ExpectedTransactionCreateRequest,
            ExpectedTransactionUpdateRequest, ExpectedTransactionViewModel, Guid>(mapper, unitOfWork, logger),
        IExpectedTransactionService
{
    private const int ReminderWindowDays = 7;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private static readonly ExpectedTransactionCreateRequestValidator CreateValidator = new();
    private static readonly ExpectedTransactionUpdateRequestValidator UpdateValidator = new();
    private static readonly RecordPaymentRequestValidator PaymentValidator = new();

    // No Include or entity-to-view mapping here: the shared projection masks foreign navigation fields in SQL.
    private IQueryable<ExpectedTransactionViewModel> Query(DateTime todayUtc7)
    {
        return Mapper.ProjectTo<ExpectedTransactionViewModel>(
            _unitOfWork.Repository<ExpectedTransaction, Guid>().GetNoTrackingEntities(), new { todayUtc7 });
    }

    /// <inheritdoc />
    /// <remarks>See <see cref="IExpectedTransactionService" /> for owner-safe projection and runtime display-state semantics.</remarks>
    public override async Task<IEnumerable<ExpectedTransactionViewModel>?> GetAllDtoAsync()
    {
        return await Query(calendar.Today).OrderBy(t => t.ExpectedDate).ThenBy(t => t.Id).ToListAsync();
    }

    /// <inheritdoc />
    /// <remarks>See <see cref="IExpectedTransactionService" /> for owner-safe projection and missing-identifier semantics.</remarks>
    public override Task<ExpectedTransactionViewModel?> GetByIdAsync(Guid id)
    {
        return Query(calendar.Today).SingleOrDefaultAsync(t => t.Id == id);
    }

    /// <inheritdoc />
    public async Task<IBasePaging<ExpectedTransactionViewModel>?> GetPagingAsync(IFilterBodyRequest request)
    {
        var query = Query(calendar.Today);
        if (!string.IsNullOrEmpty(request.SearchValue))
        {
            var search = request.SearchValue.ToLower();
            query = query.Where(t =>
                (t.Description != null && t.Description.ToLower().Contains(search)) ||
                (t.Category != null && t.Category.ToLower().Contains(search)) ||
                (t.TemplateName != null && t.TemplateName.ToLower().Contains(search)));
        }

        var orders = request.Orders?.ToList() ?? [];
        if (orders.Count == 0)
            orders.Add(new SortDescriptor { Field = nameof(ExpectedTransactionViewModel.ExpectedDate), Direction = SortDirection.Asc });
        if (!orders.Any(o => string.Equals(o.Field, nameof(ExpectedTransactionViewModel.Id), StringComparison.OrdinalIgnoreCase)))
            orders.Add(new SortDescriptor { Field = nameof(ExpectedTransactionViewModel.Id), Direction = SortDirection.Asc });
        request.Orders = orders;

        // DisplayStatus is already a SQL expression, so search, status/date/template filters and count precede paging.
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
    public async Task<IReadOnlyList<ExpectedTransactionViewModel>> GetRemindersAsync(
        CancellationToken cancellationToken = default)
    {
        var today = calendar.Today;
        var lastDay = today.AddDays(ReminderWindowDays);
        return await Query(today)
            .Where(t => t.Status == ExpectedTransactionStatus.Pending && t.ExpectedDate.Date <= lastDay)
            .OrderBy(t => t.ExpectedDate).ThenBy(t => t.Id).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ExpectedTransactionViewModel>> GetPendingTransactionsAsync(Guid userId)
    {
        return await Query(calendar.Today)
            .Where(t => t.UserId == userId && t.Status == ExpectedTransactionStatus.Pending)
            .OrderBy(t => t.ExpectedDate).ThenBy(t => t.Id).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ExpectedTransactionViewModel>> GetUpcomingTransactionsAsync(Guid userId, int days = 30)
    {
        var today = calendar.Today;
        var lastDay = today.AddDays(days);
        return await Query(today)
            .Where(t => t.UserId == userId && t.Status == ExpectedTransactionStatus.Pending &&
                        t.ExpectedDate.Date >= today && t.ExpectedDate.Date <= lastDay)
            .OrderBy(t => t.ExpectedDate).ThenBy(t => t.Id).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ExpectedTransactionViewModel>> GetTransactionsByTemplateAsync(Guid templateId)
    {
        return await Query(calendar.Today).Where(t => t.RecurringTransactionTemplateId == templateId)
            .OrderBy(t => t.ExpectedDate).ThenBy(t => t.Id).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ExpectedTransactionViewModel>> GetTransactionsByAccountAsync(Guid accountId)
    {
        return await Query(calendar.Today).Where(t => t.AccountId == accountId)
            .OrderBy(t => t.ExpectedDate).ThenBy(t => t.Id).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ExpectedTransactionViewModel>> GetTransactionsByDateRangeAsync(Guid userId,
        DateTime startDate, DateTime endDate)
    {
        var firstDay = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Unspecified);
        var lastDay = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Unspecified);
        return await Query(calendar.Today)
            .Where(t => t.UserId == userId && t.ExpectedDate.Date >= firstDay && t.ExpectedDate.Date <= lastDay)
            .OrderBy(t => t.ExpectedDate).ThenBy(t => t.Id).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<decimal> GetCashFlowForecastAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        var transactions = await GetTransactionsByDateRangeAsync(userId, startDate, endDate);
        return transactions.Where(t => t.Status == ExpectedTransactionStatus.Pending)
            .Sum(t => t.TransactionType == RecurringTransactionType.Income ? t.ExpectedAmount :
                t.TransactionType == RecurringTransactionType.Expense ? -t.ExpectedAmount : 0);
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, decimal>> GetCategoryForecastAsync(Guid userId, DateTime startDate,
        DateTime endDate)
    {
        var transactions = await GetTransactionsByDateRangeAsync(userId, startDate, endDate);
        return transactions
            .Where(t => t.Status == ExpectedTransactionStatus.Pending && !string.IsNullOrEmpty(t.Category))
            .GroupBy(t => t.Category!)
            .ToDictionary(g => g.Key,
                g => g.Sum(t => t.TransactionType == RecurringTransactionType.Income ? t.ExpectedAmount : -t.ExpectedAmount));
    }

    /// <inheritdoc />
    /// <remarks>See <see cref="IExpectedTransactionService" /> for Pending-only creation, owner and atomicity guarantees.</remarks>
    public override async Task<ExpectedTransactionViewModel?> CreateAsync(ExpectedTransactionCreateRequest request)
    {
        CreateValidator.ValidateAndThrow(request);
        await ValidateReferencesAsync(request);
        await using var trans = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var entity = Mapper.Map<ExpectedTransaction>(request);
            entity.GeneratedAt = DateTime.UtcNow;
            if (await _unitOfWork.Repository<ExpectedTransaction, Guid>().CreateAsync(entity) == 0)
                throw new CreateFailedException(CoreFinanceMessages.ExpectedTransactionCreateFailed);
            await _unitOfWork.SaveChangesAsync();
            var result = await GetByIdAsync(entity.Id)
                         ?? throw new CreateFailedException(CoreFinanceMessages.CreatedExpectedTransactionReadFailed);
            await trans.CommitAsync();
            return result;
        }
        catch
        {
            await trans.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    /// <inheritdoc />
    /// <remarks>See <see cref="IExpectedTransactionService" /> for all-or-nothing bulk creation and empty-input behavior.</remarks>
    public override async Task<IEnumerable<ExpectedTransactionViewModel>?> CreateAsync(
        List<ExpectedTransactionCreateRequest> request)
    {
        if (request.Count == 0) return Array.Empty<ExpectedTransactionViewModel>();
        foreach (var item in request)
        {
            CreateValidator.ValidateAndThrow(item);
            await ValidateReferencesAsync(item);
        }

        await using var trans = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var entities = Mapper.Map<List<ExpectedTransaction>>(request);
            var now = DateTime.UtcNow;
            foreach (var entity in entities) entity.GeneratedAt = now;
            if (await _unitOfWork.Repository<ExpectedTransaction, Guid>().CreateAsync(entities) == 0)
                throw new CreateFailedException(CoreFinanceMessages.ExpectedTransactionsCreateFailed);
            await _unitOfWork.SaveChangesAsync();
            var ids = entities.Select(e => e.Id).ToArray();
            var result = await Query(calendar.Today).Where(t => ids.Contains(t.Id))
                .OrderBy(t => t.ExpectedDate).ThenBy(t => t.Id).ToListAsync();
            await trans.CommitAsync();
            return result;
        }
        catch
        {
            await trans.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task ValidateReferencesAsync(ExpectedTransactionCreateRequest request)
    {
        if (request.AccountId is { } accountId &&
            !await _unitOfWork.Repository<Account, Guid>().GetNoTrackingEntities().AnyAsync(a => a.Id == accountId))
            throw new EntityNotFoundException(CoreFinanceMessages.AccountNotFound);
        if (request.RecurringTransactionTemplateId is { } templateId &&
            !await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>().GetNoTrackingEntities()
                .AnyAsync(t => t.Id == templateId))
            throw new EntityNotFoundException(CoreFinanceMessages.RecurringTransactionTemplateNotFound);
    }

    /// <inheritdoc />
    /// <remarks>See <see cref="IExpectedTransactionService" /> for Pending guards, first-adjustment history and original xmin.</remarks>
    public override async Task<ExpectedTransactionViewModel?> UpdateAsync(Guid id, ExpectedTransactionUpdateRequest request)
    {
        if (id != request.Id) throw new KeyNotFoundException(CoreFinanceMessages.RouteAndRequestIdsMustMatch);
        UpdateValidator.ValidateAndThrow(request);
        await using var trans = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var entity = await _unitOfWork.Repository<ExpectedTransaction, Guid>().GetByIdAsync(id);
            if (entity == null || entity.IsDeleted != null)
                throw new EntityNotFoundException(CoreFinanceMessages.ExpectedTransactionNotFound);
            if (entity.Status != ExpectedTransactionStatus.Pending || entity.ActualTransactionId.HasValue ||
                (request.Status.HasValue && request.Status != entity.Status))
                throw new ExpectedTransactionConflictException(CoreFinanceMessages.PendingTransactionEditRequired);

            if (request.ExpectedDate is { } expectedDate && expectedDate != entity.ExpectedDate &&
                entity.RecurringTransactionTemplateId is { } templateId)
            {
                var frequency = await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>()
                    .GetNoTrackingEntities()
                    .Where(t => t.Id == templateId && t.IsDeleted == null)
                    .Select(t => (RecurrenceFrequency?)t.Frequency)
                    .SingleOrDefaultAsync();
                if (frequency is { } templateFrequency &&
                    BusinessCalendar.SlotKey(templateFrequency, expectedDate) !=
                    BusinessCalendar.SlotKey(templateFrequency, entity.ExpectedDate))
                    throw new ValidationException(CoreFinanceMessages.ExpectedDateMustStayInSlot);
            }

            if (request.ExpectedAmount is { } amount && amount != entity.ExpectedAmount)
            {
                if (!ValidReason(request.AdjustmentReason))
                    throw new ValidationException(CoreFinanceMessages.AmountChangeReasonRequired);
                ApplyAdjustment(entity, amount, request.AdjustmentReason!);
            }
            if (request.ExpectedDate.HasValue) entity.ExpectedDate = request.ExpectedDate.Value;
            if (request.Description != null) entity.Description = request.Description;
            if (request.Category != null) entity.Category = request.Category;
            if (request.Notes != null) entity.Notes = request.Notes;

            // Keep the loaded entity and original shadow xmin, including date-only and same-value writes.
            if (await _unitOfWork.Repository<ExpectedTransaction, Guid>().UpdateAsync(entity) == 0)
                throw new UpdateFailedException(CoreFinanceMessages.ExpectedTransactionUpdateFailed);
            await _unitOfWork.SaveChangesAsync();
            var result = await GetByIdAsync(id)
                         ?? throw new UpdateFailedException(CoreFinanceMessages.UpdatedExpectedTransactionReadFailed);
            await trans.CommitAsync();
            return result;
        }
        catch
        {
            await trans.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    /// <inheritdoc />
    public Task<ExpectedTransactionActionResult> RecordPaymentAsync(Guid expectedTransactionId,
        RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        return ExecutePendingActionAsync(expectedTransactionId, async (entity, token) =>
        {
            if (request == null || !PaymentValidator.Validate(request).IsValid)
                return Failure(entity.Id, ExpectedTransactionActionOutcome.InvalidRequest, CoreFinanceErrorCodes.InvalidRequest, CoreFinanceMessages.ValidPaymentRequestRequired);
            if (entity.TransactionType is not (RecurringTransactionType.Income or RecurringTransactionType.Expense))
                return Failure(entity.Id, ExpectedTransactionActionOutcome.InvalidRequest, CoreFinanceErrorCodes.UnsupportedTransactionType, CoreFinanceMessages.PaymentTransactionTypeUnsupported);
            if (entity.ExpectedAmount <= 0)
                return Failure(entity.Id, ExpectedTransactionActionOutcome.InvalidRequest, CoreFinanceErrorCodes.InvalidAmount, CoreFinanceMessages.ExpectedAmountMustBePositive);
            if (entity.AccountId.HasValue && request.AccountId.HasValue && entity.AccountId != request.AccountId)
                return Failure(entity.Id, ExpectedTransactionActionOutcome.InvalidRequest, CoreFinanceErrorCodes.AccountOverrideNotAllowed, CoreFinanceMessages.BoundAccountCannotBeOverridden);

            var accountId = entity.AccountId ?? request.AccountId;
            if (!accountId.HasValue || accountId == Guid.Empty)
                return Failure(entity.Id, ExpectedTransactionActionOutcome.InvalidRequest, CoreFinanceErrorCodes.AccountRequired, CoreFinanceMessages.PaymentAccountRequired);
            if (!await _unitOfWork.Repository<Account, Guid>().GetNoTrackingEntities()
                    .AnyAsync(a => a.Id == accountId.Value && a.UserId == entity.UserId, token))
                return Failure(entity.Id, ExpectedTransactionActionOutcome.NotFound, CoreFinanceErrorCodes.AccountNotFound, CoreFinanceMessages.AccountNotFound);

            token.ThrowIfCancellationRequested();
            // Do not mutate the tracked period before CreateAsync: its SaveChanges must not advance our original xmin.
            var actual = await transactionService.CreateAsync(new TransactionCreateRequest
            {
                AccountId = accountId.Value,
                UserId = entity.UserId,
                TransactionDate = entity.ExpectedDate,
                RevenueAmount = entity.TransactionType == RecurringTransactionType.Income ? entity.ExpectedAmount : 0,
                SpentAmount = entity.TransactionType == RecurringTransactionType.Expense ? entity.ExpectedAmount : 0,
                CategoryType = entity.TransactionType == RecurringTransactionType.Income ? CategoryType.Income : CategoryType.Expense,
                CategorySummary = entity.Category,
                Description = entity.Description
            });
            if (actual == null || actual.Id == Guid.Empty)
                throw new CreateFailedException(CoreFinanceMessages.ActualTransactionCreateFailed);

            Confirm(entity, actual.Id);
            return Success(entity, CoreFinanceMessages.PaymentRecorded, actual);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ExpectedTransactionActionResult> ConfirmExpectedTransactionAsync(Guid expectedTransactionId,
        Guid actualTransactionId, CancellationToken cancellationToken = default)
    {
        return ExecutePendingActionAsync(expectedTransactionId, async (entity, token) =>
        {
            if (actualTransactionId == Guid.Empty)
                return Failure(entity.Id, ExpectedTransactionActionOutcome.InvalidRequest, CoreFinanceErrorCodes.InvalidActualTransaction, CoreFinanceMessages.ActualTransactionIdRequired);
            var actual = await _unitOfWork.Repository<Transaction, Guid>().GetByIdNoTrackingAsync(actualTransactionId);
            if (actual == null || actual.IsDeleted != null || actual.UserId != entity.UserId)
                return Failure(entity.Id, ExpectedTransactionActionOutcome.NotFound, CoreFinanceErrorCodes.ActualTransactionNotFound, CoreFinanceMessages.ActualTransactionNotFound);

            // Ignore only the soft-delete filter; the repository's explicit owner predicate remains in the query.
            // The database unique index also closes races and reserves links held by legacy tombstones.
            if (await _unitOfWork.Repository<ExpectedTransaction, Guid>().GetNoTrackingEntities().IgnoreQueryFilters()
                    .AnyAsync(t => t.Id != entity.Id && t.ActualTransactionId == actualTransactionId, token))
                return Failure(entity.Id, ExpectedTransactionActionOutcome.Conflict, CoreFinanceErrorCodes.AlreadyLinked, CoreFinanceMessages.ActualTransactionAlreadyLinked);

            Confirm(entity, actualTransactionId);
            return Success(entity, CoreFinanceMessages.ExpectedTransactionConfirmed);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ExpectedTransactionActionResult> CancelExpectedTransactionAsync(Guid expectedTransactionId, string reason,
        CancellationToken cancellationToken = default)
    {
        return ExecutePendingActionAsync(expectedTransactionId, (entity, _) =>
        {
            if (!ValidReason(reason))
                return ValueTask.FromResult(Failure(entity.Id, ExpectedTransactionActionOutcome.InvalidRequest, CoreFinanceErrorCodes.InvalidReason, CoreFinanceMessages.ReasonRequired));
            entity.Status = ExpectedTransactionStatus.Cancelled;
            entity.AdjustmentReason = reason;
            entity.ProcessedAt = DateTime.UtcNow;
            return ValueTask.FromResult(Success(entity, CoreFinanceMessages.ExpectedTransactionCancelled));
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ExpectedTransactionActionResult> AdjustExpectedTransactionAsync(Guid expectedTransactionId, decimal newAmount,
        string reason, CancellationToken cancellationToken = default)
    {
        return ExecutePendingActionAsync(expectedTransactionId, (entity, _) =>
        {
            if (newAmount <= 0)
                return ValueTask.FromResult(Failure(entity.Id, ExpectedTransactionActionOutcome.InvalidRequest, CoreFinanceErrorCodes.InvalidAmount, CoreFinanceMessages.ExpectedAmountMustBePositive));
            if (!ValidReason(reason))
                return ValueTask.FromResult(Failure(entity.Id, ExpectedTransactionActionOutcome.InvalidRequest, CoreFinanceErrorCodes.InvalidReason, CoreFinanceMessages.ReasonRequired));
            ApplyAdjustment(entity, newAmount, reason);
            return ValueTask.FromResult(Success(entity, CoreFinanceMessages.ExpectedTransactionAdjusted));
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ExpectedTransactionActionResult> DeletePendingAsync(Guid expectedTransactionId,
        CancellationToken cancellationToken = default)
    {
        return ExecutePendingActionAsync(expectedTransactionId, (entity, _) =>
        {
            // Update the tracked tombstone with its original xmin, never reattach or reload after the Pending guard.
            entity.IsDeleted = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return ValueTask.FromResult(Success(entity, CoreFinanceMessages.ExpectedTransactionDeleted));
        }, cancellationToken);
    }

    private async Task<ExpectedTransactionActionResult> ExecutePendingActionAsync(Guid id,
        Func<ExpectedTransaction, CancellationToken, ValueTask<ExpectedTransactionActionResult>> action,
        CancellationToken cancellationToken)
    {
        ExpectedTransactionActionResult? committedResult = null;
        try
        {
            await using var trans = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var entity = await _unitOfWork.Repository<ExpectedTransaction, Guid>().GetByIdAsync(id);
                if (entity == null || entity.IsDeleted != null)
                    return Failure(id, ExpectedTransactionActionOutcome.NotFound, CoreFinanceErrorCodes.ExpectedTransactionNotFound, CoreFinanceMessages.ExpectedTransactionNotFound);
                if (entity.Status != ExpectedTransactionStatus.Pending || entity.ActualTransactionId.HasValue)
                    return Failure(id, ExpectedTransactionActionOutcome.Conflict, CoreFinanceErrorCodes.StateConflict, CoreFinanceMessages.PendingTransactionRequired);

                var result = await action(entity, cancellationToken);
                if (result.Outcome != ExpectedTransactionActionOutcome.Success) return result;
                if (await _unitOfWork.Repository<ExpectedTransaction, Guid>().UpdateAsync(entity, cancellationToken) == 0)
                    throw new UpdateFailedException(CoreFinanceMessages.ExpectedTransactionUpdateFailed);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await trans.CommitAsync(cancellationToken);
                committedResult = result;
                return result;
            }
            catch
            {
                // Rollback must still run if the HTTP request has already been cancelled.
                await trans.RollbackAsync(CancellationToken.None);
                throw;
            }
        }
        catch (Exception ex) when (committedResult != null)
        {
            // A disposal failure after a successful commit must not report that the payment was rolled back.
            logger.LogError(ex, "Error disposing committed expected transaction action {ExpectedTransactionId}", id);
            return committedResult;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrent expected transaction action {ExpectedTransactionId}", id);
            return Failure(id, ExpectedTransactionActionOutcome.Conflict, CoreFinanceErrorCodes.ConcurrencyConflict, CoreFinanceMessages.PeriodChanged);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
               { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ux_expected_actual_transaction" })
        {
            logger.LogWarning(ex, "Actual transaction link conflict for expected transaction {ExpectedTransactionId}", id);
            return Failure(id, ExpectedTransactionActionOutcome.Conflict, CoreFinanceErrorCodes.AlreadyLinked, CoreFinanceMessages.ActualTransactionAlreadyLinked);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed expected transaction action {ExpectedTransactionId}", id);
            return Failure(id, ExpectedTransactionActionOutcome.PersistenceError, CoreFinanceErrorCodes.PersistenceError, CoreFinanceMessages.OperationCouldNotBeSaved);
        }
    }

    private static bool ValidReason(string? reason) => !string.IsNullOrWhiteSpace(reason) && reason.Length <= 500;

    private static void ApplyAdjustment(ExpectedTransaction entity, decimal amount, string reason)
    {
        if (!entity.IsAdjusted) entity.OriginalAmount = entity.ExpectedAmount;
        entity.ExpectedAmount = amount;
        entity.IsAdjusted = true;
        entity.AdjustmentReason = reason;
    }

    private static void Confirm(ExpectedTransaction entity, Guid actualTransactionId)
    {
        entity.Status = ExpectedTransactionStatus.Confirmed;
        entity.ActualTransactionId = actualTransactionId;
        entity.ProcessedAt = DateTime.UtcNow;
    }

    private static ExpectedTransactionActionResult Success(ExpectedTransaction entity, string message,
        TransactionViewModel? actual = null) => new()
    {
        Outcome = ExpectedTransactionActionOutcome.Success,
        Code = CoreFinanceErrorCodes.Success,
        Message = message,
        ExpectedTransactionId = entity.Id,
        ActualTransactionId = entity.ActualTransactionId,
        ActualTransaction = actual
    };

    private static ExpectedTransactionActionResult Failure(Guid id, ExpectedTransactionActionOutcome outcome,
        string code, string message) => new()
    {
        Outcome = outcome,
        Code = code,
        Message = message,
        ExpectedTransactionId = id
    };
}
