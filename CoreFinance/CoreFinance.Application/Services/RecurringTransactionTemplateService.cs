using AutoMapper;
using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Application.Interfaces;
using CoreFinance.Application.Services.Base;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.DTOs;
using Shared.EntityFramework.EntityFrameworkUtilities;

namespace CoreFinance.Application.Services;

/// <summary>
///     (EN) Service for managing recurring transaction templates.<br />
///     (VI) Dịch vụ quản lý các mẫu giao dịch định kỳ.
/// </summary>
public class RecurringTransactionTemplateService(
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<RecurringTransactionTemplateService> logger)
    : BaseService<RecurringTransactionTemplate, RecurringTransactionTemplateCreateRequest,
            RecurringTransactionTemplateUpdateRequest, RecurringTransactionTemplateViewModel, Guid>(mapper, unitOfWork,
            logger),
        IRecurringTransactionTemplateService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    ///     (EN) Gets a paginated list of recurring transaction templates based on a filter request.<br />
    ///     (VI) Lấy danh sách mẫu giao dịch định kỳ có phân trang dựa trên yêu cầu lọc.
    /// </summary>
    /// <param name="request">The filter request body.</param>
    /// <returns>A paginated list of recurring transaction template view models.</returns>
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

    /// <summary>
    ///     (EN) Gets a list of active recurring transaction templates for a user.<br />
    ///     (VI) Lấy danh sách các mẫu giao dịch định kỳ đang hoạt động cho người dùng.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A list of active recurring transaction templates.</returns>
    public async Task<IEnumerable<RecurringTransactionTemplateViewModel>> GetActiveTemplatesAsync(Guid userId)
    {
        var query = _unitOfWork.Repository<RecurringTransactionTemplate, Guid>()
            .GetNoTrackingEntities()
            .Where(t => t.UserId == userId && t.IsActive);

        return await Mapper.ProjectTo<RecurringTransactionTemplateViewModel>(query).ToListAsync();
    }

    /// <summary>
    ///     (EN) Gets a list of recurring transaction templates associated with a specific account.<br />
    ///     (VI) Lấy danh sách các mẫu giao dịch định kỳ liên quan đến một tài khoản cụ thể.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>A list of recurring transaction templates by account.</returns>
    public async Task<IEnumerable<RecurringTransactionTemplateViewModel>> GetTemplatesByAccountAsync(Guid accountId)
    {
        var query = _unitOfWork.Repository<RecurringTransactionTemplate, Guid>()
            .GetNoTrackingEntities()
            .Where(t => t.AccountId == accountId);

        return await Mapper.ProjectTo<RecurringTransactionTemplateViewModel>(query).ToListAsync();
    }

    /// <summary>
    ///     (EN) Toggles the active status of a recurring transaction template.<br />
    ///     (VI) Chuyển đổi trạng thái hoạt động của một mẫu giao dịch định kỳ.
    /// </summary>
    /// <param name="templateId">The ID of the template to toggle.</param>
    /// <param name="isActive">The new active status.</param>
    /// <returns>True if the status was successfully toggled, false otherwise.</returns>
    public async Task<bool> ToggleActiveStatusAsync(Guid templateId, bool isActive)
    {
        await using var trans = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var template = await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>()
                .GetByIdAsync(templateId);

            if (template == null)
                return false;

            template.IsActive = isActive;
            template.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>().UpdateAsync(template);
            await _unitOfWork.SaveChangesAsync();

            await trans.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await trans.RollbackAsync();
            logger.LogError(ex, "Error toggling active status for template {TemplateId}", templateId);
            return false;
        }
    }

    /// <summary>
    ///     (EN) Calculates the next execution date for a recurring transaction template.<br />
    ///     (VI) Tính toán ngày thực hiện tiếp theo cho một mẫu giao dịch định kỳ.
    /// </summary>
    /// <param name="templateId">The ID of the template.</param>
    /// <returns>The next execution date.</returns>
    public async Task<DateTime> CalculateNextExecutionDateAsync(Guid templateId)
    {
        var template = await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>()
            .GetByIdAsync(templateId);

        if (template == null)
            throw new ArgumentException("Template not found", nameof(templateId));

        return CalculateNextExecutionDate(template.NextExecutionDate, template.Frequency, template.CustomIntervalDays);
    }

    /// <summary>
    ///     (EN) Generates expected transactions for a specific recurring transaction template within a number of days in
    ///     advance.<br />
    ///     (VI) Tạo các giao dịch dự kiến cho một mẫu giao dịch định kỳ cụ thể trong một số ngày tới.
    /// </summary>
    /// <param name="templateId">The ID of the template.</param>
    /// <param name="daysInAdvance">The number of days in advance to generate transactions for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GenerateExpectedTransactionsAsync(Guid templateId, int daysInAdvance)
    {
        await using var trans = await _unitOfWork.BeginTransactionAsync();
        try
        {
            await GenerateExpectedTransactionsInternalAsync(templateId, daysInAdvance);
            await trans.CommitAsync();
        }
        catch (Exception ex)
        {
            await trans.RollbackAsync();
            logger.LogError(ex, "Error generating expected transactions for template {TemplateId}", templateId);
            throw;
        }
    }

    /// <summary>
    ///     (EN) Generates expected transactions for all active recurring transaction templates.<br />
    ///     (VI) Tạo các giao dịch dự kiến cho tất cả các mẫu giao dịch định kỳ đang hoạt động.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GenerateExpectedTransactionsForAllActiveTemplatesAsync()
    {
        await using var trans = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var activeTemplates = await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>()
                .GetNoTrackingEntities()
                .Where(t => t.IsActive && t.AutoGenerate)
                .ToListAsync();

            foreach (var template in activeTemplates)
                await GenerateExpectedTransactionsInternalAsync(template.Id, template.DaysInAdvance);

            await trans.CommitAsync();
        }
        catch (Exception ex)
        {
            await trans.RollbackAsync();
            logger.LogError(ex, "Error generating expected transactions for all active templates");
            throw;
        }
    }

    /// <summary>
    ///     (EN) Creates a new recurring transaction template.<br />
    ///     (VI) Tạo một mẫu giao dịch định kỳ mới.
    /// </summary>
    /// <param name="request">The create request.</param>
    /// <returns>The created recurring transaction template view model.</returns>
    public override async Task<RecurringTransactionTemplateViewModel?> CreateAsync(
        RecurringTransactionTemplateCreateRequest request)
    {
        // Set default values
        if (request.NextExecutionDate == default) request.NextExecutionDate = request.StartDate;

        var result = await base.CreateAsync(request);

        // Generate expected transactions if auto-generate is enabled
        if (result != null && request.AutoGenerate)
            await GenerateExpectedTransactionsAsync(result.Id, request.DaysInAdvance);

        return result;
    }

    /// <summary>
    ///     (EN) Updates an existing recurring transaction template.<br />
    ///     (VI) Cập nhật một mẫu giao dịch định kỳ hiện có.
    /// </summary>
    /// <param name="id">The ID of the template to update.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The updated recurring transaction template view model.</returns>
    public override async Task<RecurringTransactionTemplateViewModel?> UpdateAsync(Guid id,
        RecurringTransactionTemplateUpdateRequest request)
    {
        var result = await base.UpdateAsync(id, request);

        // Regenerate expected transactions if auto-generate is enabled
        if (result is { AutoGenerate: true }) await GenerateExpectedTransactionsAsync(result.Id, result.DaysInAdvance);

        return result;
    }

    /// <summary>
    ///     (EN) Generates expected transactions internally for a specific recurring transaction template within a number of
    ///     days in advance.<br />
    ///     (VI) Tạo các giao dịch dự kiến nội bộ cho một mẫu giao dịch định kỳ cụ thể trong một số ngày tới.
    /// </summary>
    /// <param name="templateId">The ID of the template.</param>
    /// <param name="daysInAdvance">The number of days in advance to generate transactions for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task GenerateExpectedTransactionsInternalAsync(Guid templateId, int daysInAdvance)
    {
        var template = await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>()
            .GetByIdAsync(templateId);

        if (template == null || !template.IsActive || !template.AutoGenerate)
            return;

        var endDate = DateTime.UtcNow.AddDays(daysInAdvance);
        var currentDate = template.NextExecutionDate;

        // Check if end date is set and we've passed it
        if (template.EndDate.HasValue && currentDate > template.EndDate.Value)
            return;

        var expectedTransactionRepo = _unitOfWork.Repository<ExpectedTransaction, Guid>();

        while (currentDate <= endDate)
        {
            // Check if end date is set and we've passed it
            if (template.EndDate.HasValue && currentDate > template.EndDate.Value)
                break;

            // Check if expected transaction already exists for this date
            var existingTransaction = await expectedTransactionRepo
                .GetNoTrackingEntities()
                .FirstOrDefaultAsync(et => et.RecurringTransactionTemplateId == templateId &&
                                           et.ExpectedDate.Date == currentDate.Date);

            if (existingTransaction == null)
            {
                var expectedTransaction = new ExpectedTransaction
                {
                    Id = Guid.CreateVersion7(),
                    RecurringTransactionTemplateId = templateId,
                    UserId = template.UserId,
                    AccountId = template.AccountId,
                    ExpectedDate = currentDate,
                    ExpectedAmount = template.Amount,
                    Description = template.Description,
                    TransactionType = template.TransactionType,
                    Category = template.Category,
                    Status = ExpectedTransactionStatus.Pending,
                    GeneratedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await expectedTransactionRepo.CreateAsync(expectedTransaction);
            }

            // Calculate next execution date
            currentDate = CalculateNextExecutionDate(currentDate, template.Frequency, template.CustomIntervalDays);
        }

        // Update template's next execution date
        template.NextExecutionDate = currentDate;
        template.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<RecurringTransactionTemplate, Guid>().UpdateAsync(template);

        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    ///     (EN) Calculates the next execution date based on the current date, frequency, and custom interval.<br />
    ///     (VI) Tính toán ngày thực hiện tiếp theo dựa trên ngày hiện tại, tần suất và khoảng thời gian tùy chỉnh.
    /// </summary>
    /// <param name="currentDate">The current date.</param>
    /// <param name="frequency">The recurrence frequency.</param>
    /// <param name="customIntervalDays">The custom interval in days (if frequency is Custom).</param>
    /// <returns>The next execution date.</returns>
    private static DateTime CalculateNextExecutionDate(DateTime currentDate, RecurrenceFrequency frequency,
        int? customIntervalDays)
    {
        return frequency switch
        {
            RecurrenceFrequency.Daily => currentDate.AddDays(1),
            RecurrenceFrequency.Weekly => currentDate.AddDays(7),
            RecurrenceFrequency.Biweekly => currentDate.AddDays(14),
            RecurrenceFrequency.Monthly => currentDate.AddMonths(1),
            RecurrenceFrequency.Quarterly => currentDate.AddMonths(3),
            RecurrenceFrequency.SemiAnnually => currentDate.AddMonths(6),
            RecurrenceFrequency.Annually => currentDate.AddYears(1),
            RecurrenceFrequency.Custom => currentDate.AddDays(customIntervalDays ?? 1),
            _ => currentDate.AddDays(1)
        };
    }
}