using MoneyManagement.Application.DTOs.Budget;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Interfaces;

/// <summary>
///     Interface for budget management service (EN)<br />
///     Giao diện cho dịch vụ quản lý ngân sách (VI)
/// </summary>
public interface IBudgetService
{
    /// <summary>
    ///     Creates a new budget asynchronously (EN)<br />
    ///     Tạo ngân sách mới một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="request">Create budget request</param>
    /// <param name="userId">User identifier</param>
    /// <returns>Created budget view model</returns>
    Task<BudgetViewModel> CreateBudgetAsync(CreateBudgetRequest request, Guid userId);

    /// <summary>
    ///     Updates an existing budget asynchronously (EN)<br />
    ///     Cập nhật ngân sách hiện có một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="request">Update budget request</param>
    /// <param name="userId">User identifier</param>
    /// <returns>Updated budget view model</returns>
    Task<BudgetViewModel> UpdateBudgetAsync(UpdateBudgetRequest request, Guid userId);

    /// <summary>
    ///     Deletes a budget asynchronously (EN)<br />
    ///     Xóa ngân sách một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="budgetId">Budget identifier</param>
    /// <param name="userId">User identifier</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteBudgetAsync(Guid budgetId, Guid userId);

    /// <summary>
    ///     Gets a budget by identifier asynchronously (EN)<br />
    ///     Lấy ngân sách theo định danh một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="budgetId">Budget identifier</param>
    /// <param name="userId">User identifier</param>
    /// <returns>Budget view model or null if not found</returns>
    Task<BudgetViewModel?> GetBudgetByIdAsync(Guid budgetId, Guid userId);

    /// <summary>
    ///     Gets all budgets for a user asynchronously (EN)<br />
    ///     Lấy tất cả ngân sách của người dùng một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>List of budget view models</returns>
    Task<List<BudgetViewModel>> GetBudgetsByUserIdAsync(Guid userId);

    /// <summary>
    ///     Gets active budgets for a user asynchronously (EN)<br />
    ///     Lấy các ngân sách đang hoạt động của người dùng một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>List of active budget view models</returns>
    Task<List<BudgetViewModel>> GetActiveBudgetsByUserIdAsync(Guid userId);

    /// <summary>
    ///     Gets budgets by category asynchronously (EN)<br />
    ///     Lấy ngân sách theo danh mục một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="category">Budget category</param>
    /// <returns>List of budget view models</returns>
    Task<List<BudgetViewModel>> GetBudgetsByCategoryAsync(Guid userId, string category);

    /// <summary>
    ///     Gets budgets by period asynchronously (EN)<br />
    ///     Lấy ngân sách theo chu kỳ một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="period">Budget period</param>
    /// <returns>List of budget view models</returns>
    Task<List<BudgetViewModel>> GetBudgetsByPeriodAsync(Guid userId, BudgetPeriod period);

    /// <summary>
    ///     Updates budget spent amount asynchronously (EN)<br />
    ///     Cập nhật số tiền đã chi tiêu của ngân sách một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="budgetId">Budget identifier</param>
    /// <param name="amount">Amount to add to spent amount</param>
    /// <param name="userId">User identifier</param>
    /// <returns>Updated budget view model</returns>
    Task<BudgetViewModel> UpdateBudgetSpentAmountAsync(Guid budgetId, decimal amount, Guid userId);

    /// <summary>
    ///     Gets budgets that have reached alert threshold asynchronously (EN)<br />
    ///     Lấy các ngân sách đã đạt ngưỡng cảnh báo một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>List of budget view models that reached alert threshold</returns>
    Task<List<BudgetViewModel>> GetBudgetsReachedAlertThresholdAsync(Guid userId);

    /// <summary>
    ///     Gets over-budget budgets asynchronously (EN)<br />
    ///     Lấy các ngân sách vượt giới hạn một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>List of over-budget view models</returns>
    Task<List<BudgetViewModel>> GetOverBudgetBudgetsAsync(Guid userId);

    /// <summary>
    ///     Changes budget status asynchronously (EN)<br />
    ///     Thay đổi trạng thái ngân sách một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="budgetId">Budget identifier</param>
    /// <param name="status">New budget status</param>
    /// <param name="userId">User identifier</param>
    /// <returns>Updated budget view model</returns>
    Task<BudgetViewModel> ChangeBudgetStatusAsync(Guid budgetId, BudgetStatus status, Guid userId);
}