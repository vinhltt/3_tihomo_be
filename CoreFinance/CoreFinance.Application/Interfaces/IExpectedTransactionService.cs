using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Services.Base;
using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Entities;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Application.Interfaces;

/// <summary>
///     (EN) Represents the service interface for managing expected transactions.<br />
///     (VI) Đại diện cho interface dịch vụ quản lý các giao dịch dự kiến.
/// </summary>
public interface IExpectedTransactionService : IBaseService<ExpectedTransaction,
    ExpectedTransactionCreateRequest, ExpectedTransactionUpdateRequest,
    ExpectedTransactionViewModel, Guid>
{
    /// <summary>
    ///     (EN) Gets a paginated list of expected transactions.<br />
    ///     (VI) Lấy danh sách giao dịch dự kiến có phân trang.
    /// </summary>
    /// <param name="request">The filter request body.</param>
    /// <returns>A paginated list of expected transactions.</returns>
    Task<IBasePaging<ExpectedTransactionViewModel>?> GetPagingAsync(IFilterBodyRequest request);

    /// <summary>
    ///     (EN) Gets a list of pending expected transactions for a user.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến đang chờ xử lý cho người dùng.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A list of pending expected transactions.</returns>
    Task<IEnumerable<ExpectedTransactionViewModel>> GetPendingTransactionsAsync(Guid userId);

    /// <summary>
    ///     (EN) Gets a list of upcoming expected transactions for a user within a specified number of days.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến sắp tới cho người dùng trong một số ngày cụ thể.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="days">The number of upcoming days to consider (default is 30).</param>
    /// <returns>A list of upcoming expected transactions.</returns>
    Task<IEnumerable<ExpectedTransactionViewModel>> GetUpcomingTransactionsAsync(Guid userId, int days = 30);

    /// <summary>
    ///     (EN) Gets a list of expected transactions associated with a specific recurring transaction template.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến liên quan đến một mẫu giao dịch định kỳ cụ thể.
    /// </summary>
    /// <param name="templateId">The recurring transaction template ID.</param>
    /// <returns>A list of expected transactions by template.</returns>
    Task<IEnumerable<ExpectedTransactionViewModel>> GetTransactionsByTemplateAsync(Guid templateId);

    /// <summary>
    ///     (EN) Gets a list of expected transactions associated with a specific account.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến liên quan đến một tài khoản cụ thể.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>A list of expected transactions by account.</returns>
    Task<IEnumerable<ExpectedTransactionViewModel>> GetTransactionsByAccountAsync(Guid accountId);

    /// <summary>
    ///     (EN) Gets a list of expected transactions for a user within a specified date range.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến cho người dùng trong một khoảng ngày cụ thể.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>A list of expected transactions within the date range.</returns>
    Task<IEnumerable<ExpectedTransactionViewModel>> GetTransactionsByDateRangeAsync(Guid userId, DateTime startDate,
        DateTime endDate);

    /// <summary>
    ///     (EN) Confirms an expected transaction by linking it to an actual transaction.<br />
    ///     (VI) Xác nhận một giao dịch dự kiến bằng cách liên kết nó với một giao dịch thực tế.
    /// </summary>
    /// <param name="expectedTransactionId">The ID of the expected transaction to confirm.</param>
    /// <param name="actualTransactionId">The ID of the actual transaction.</param>
    /// <returns>True if the confirmation was successful, false otherwise.</returns>
    Task<bool> ConfirmExpectedTransactionAsync(Guid expectedTransactionId, Guid actualTransactionId);

    /// <summary>
    ///     (EN) Cancels an expected transaction with a specified reason.<br />
    ///     (VI) Hủy một giao dịch dự kiến với lý do cụ thể.
    /// </summary>
    /// <param name="expectedTransactionId">The ID of the expected transaction to cancel.</param>
    /// <param name="reason">The reason for canceling the transaction.</param>
    /// <returns>True if the cancellation was successful, false otherwise.</returns>
    Task<bool> CancelExpectedTransactionAsync(Guid expectedTransactionId, string reason);

    /// <summary>
    ///     (EN) Adjusts the amount of an expected transaction with a specified reason.<br />
    ///     (VI) Điều chỉnh số tiền của một giao dịch dự kiến với lý do cụ thể.
    /// </summary>
    /// <param name="expectedTransactionId">The ID of the expected transaction to adjust.</param>
    /// <param name="newAmount">The new amount for the transaction.</param>
    /// <param name="reason">The reason for adjusting the transaction.</param>
    /// <returns>True if the adjustment was successful, false otherwise.</returns>
    Task<bool> AdjustExpectedTransactionAsync(Guid expectedTransactionId, decimal newAmount, string reason);

    /// <summary>
    ///     (EN) Gets the cash flow forecast for a user within a specified date range.<br />
    ///     (VI) Lấy dự báo dòng tiền cho người dùng trong một khoảng ngày cụ thể.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="startDate">The start date of the forecast.</param>
    /// <param name="endDate">The end date of the forecast.</param>
    /// <returns>The cash flow forecast amount.</returns>
    Task<decimal> GetCashFlowForecastAsync(Guid userId, DateTime startDate, DateTime endDate);

    /// <summary>
    ///     (EN) Gets the category-based cash flow forecast for a user within a specified date range.<br />
    ///     (VI) Lấy dự báo dòng tiền theo danh mục cho người dùng trong một khoảng ngày cụ thể.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="startDate">The start date of the forecast.</param>
    /// <param name="endDate">The end date of the forecast.</param>
    /// <returns>A dictionary where keys are categories and values are the forecast amounts.</returns>
    Task<Dictionary<string, decimal>> GetCategoryForecastAsync(Guid userId, DateTime startDate, DateTime endDate);
}