using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Application.Services.Base;
using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Entities;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Application.Interfaces;

/// <summary>
///     (EN) Represents the service interface for managing recurring transaction templates.<br />
///     (VI) Đại diện cho interface dịch vụ quản lý các mẫu giao dịch định kỳ.
/// </summary>
public interface IRecurringTransactionTemplateService : IBaseService<RecurringTransactionTemplate,
    RecurringTransactionTemplateCreateRequest, RecurringTransactionTemplateUpdateRequest,
    RecurringTransactionTemplateViewModel, Guid>
{
    /// <summary>
    ///     (EN) Gets a paginated list of recurring transaction templates.<br />
    ///     (VI) Lấy danh sách mẫu giao dịch định kỳ có phân trang.
    /// </summary>
    /// <param name="request">The filter request body.</param>
    /// <returns>A paginated list of recurring transaction templates.</returns>
    Task<IBasePaging<RecurringTransactionTemplateViewModel>?> GetPagingAsync(IFilterBodyRequest request);

    /// <summary>
    ///     (EN) Gets a list of active recurring transaction templates for a user.<br />
    ///     (VI) Lấy danh sách các mẫu giao dịch định kỳ đang hoạt động cho người dùng.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A list of active recurring transaction templates.</returns>
    Task<IEnumerable<RecurringTransactionTemplateViewModel>> GetActiveTemplatesAsync(Guid userId);

    /// <summary>
    ///     (EN) Gets a list of recurring transaction templates associated with a specific account.<br />
    ///     (VI) Lấy danh sách các mẫu giao dịch định kỳ liên quan đến một tài khoản cụ thể.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>A list of recurring transaction templates by account.</returns>
    Task<IEnumerable<RecurringTransactionTemplateViewModel>> GetTemplatesByAccountAsync(Guid accountId);

    /// <summary>
    ///     (EN) Toggles the active status of a recurring transaction template.<br />
    ///     (VI) Chuyển đổi trạng thái hoạt động của một mẫu giao dịch định kỳ.
    /// </summary>
    /// <param name="templateId">The ID of the template to toggle.</param>
    /// <param name="isActive">The new active status.</param>
    /// <returns>True if the status was successfully toggled, false otherwise.</returns>
    Task<bool> ToggleActiveStatusAsync(Guid templateId, bool isActive);

    /// <summary>
    ///     (EN) Calculates the next execution date for a recurring transaction template.<br />
    ///     (VI) Tính toán ngày thực hiện tiếp theo cho một mẫu giao dịch định kỳ.
    /// </summary>
    /// <param name="templateId">The ID of the template.</param>
    /// <returns>The next execution date.</returns>
    Task<DateTime> CalculateNextExecutionDateAsync(Guid templateId);

    /// <summary>
    ///     (EN) Generates expected transactions for a specific recurring transaction template within a number of days in
    ///     advance.<br />
    ///     (VI) Tạo các giao dịch dự kiến cho một mẫu giao dịch định kỳ cụ thể trong một số ngày tới.
    /// </summary>
    /// <param name="templateId">The ID of the template.</param>
    /// <param name="daysInAdvance">The number of days in advance to generate transactions for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task GenerateExpectedTransactionsAsync(Guid templateId, int daysInAdvance);

    /// <summary>
    ///     (EN) Generates expected transactions for all active recurring transaction templates.<br />
    ///     (VI) Tạo các giao dịch dự kiến cho tất cả các mẫu giao dịch định kỳ đang hoạt động.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task GenerateExpectedTransactionsForAllActiveTemplatesAsync();
}