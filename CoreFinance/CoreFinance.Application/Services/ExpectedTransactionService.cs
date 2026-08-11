using AutoMapper;
using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Interfaces;
using CoreFinance.Application.Services.Base;
using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.EntityFrameworkUtilities;

namespace CoreFinance.Application.Services;

/// <summary>
///     (EN) Service for managing expected transactions.<br />
///     (VI) Dịch vụ quản lý các giao dịch dự kiến.
/// </summary>
public class ExpectedTransactionService(
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<ExpectedTransactionService> logger)
    : BaseService<ExpectedTransaction, ExpectedTransactionCreateRequest,
            ExpectedTransactionUpdateRequest, ExpectedTransactionViewModel, Guid>(mapper, unitOfWork, logger),
        IExpectedTransactionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    ///     (EN) Gets a paginated list of expected transactions based on a filter request.<br />
    ///     (VI) Lấy danh sách giao dịch dự kiến có phân trang dựa trên yêu cầu lọc.
    /// </summary>
    /// <param name="request">The filter request body.</param>
    /// <returns>A paginated list of expected transaction view models.</returns>
    public async Task<IBasePaging<ExpectedTransactionViewModel>?> GetPagingAsync(IFilterBodyRequest request)
    {
        var query = Mapper.ProjectTo<ExpectedTransactionViewModel>(
            _unitOfWork.Repository<ExpectedTransaction, Guid>()
                .GetNoTrackingEntities());

        if (!string.IsNullOrEmpty(request.SearchValue))
            query = query.Where(e =>
                (e.Description != null && e.Description.ToLower().Contains(request.SearchValue.ToLower())) ||
                (e.Category != null && e.Category.ToLower().Contains(request.SearchValue.ToLower())) ||
                (e.TemplateName != null && e.TemplateName.ToLower().Contains(request.SearchValue.ToLower())));

        return await query.ToPagingAsync(request);
    }

    /// <summary>
    ///     (EN) Gets a list of pending expected transactions for a user.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến đang chờ xử lý cho người dùng.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A list of pending expected transactions.</returns>
    public async Task<IEnumerable<ExpectedTransactionViewModel>> GetPendingTransactionsAsync(Guid userId)
    {
        var query = _unitOfWork.Repository<ExpectedTransaction, Guid>()
            .GetNoTrackingEntities()
            .Where(t => t.UserId == userId && t.Status == ExpectedTransactionStatus.Pending);

        return await Mapper.ProjectTo<ExpectedTransactionViewModel>(query).ToListAsync();
    }

    /// <summary>
    ///     (EN) Gets a list of upcoming expected transactions for a user within a specified number of days.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến sắp tới cho người dùng trong một số ngày cụ thể.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="days">The number of upcoming days to consider (default is 30).</param>
    /// <returns>A list of upcoming expected transactions.</returns>
    public async Task<IEnumerable<ExpectedTransactionViewModel>> GetUpcomingTransactionsAsync(Guid userId,
        int days = 30)
    {
        var endDate = DateTime.UtcNow.AddDays(days);
        var query = _unitOfWork.Repository<ExpectedTransaction, Guid>()
            .GetNoTrackingEntities()
            .Where(t => t.UserId == userId &&
                        t.Status == ExpectedTransactionStatus.Pending &&
                        t.ExpectedDate >= DateTime.UtcNow.Date &&
                        t.ExpectedDate <= endDate)
            .OrderBy(t => t.ExpectedDate);

        return await Mapper.ProjectTo<ExpectedTransactionViewModel>(query).ToListAsync();
    }

    /// <summary>
    ///     (EN) Gets a list of expected transactions associated with a specific recurring transaction template.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến liên quan đến một mẫu giao dịch định kỳ cụ thể.
    /// </summary>
    /// <param name="templateId">The recurring transaction template ID.</param>
    /// <returns>A list of expected transactions by template.</returns>
    public async Task<IEnumerable<ExpectedTransactionViewModel>> GetTransactionsByTemplateAsync(Guid templateId)
    {
        var query = _unitOfWork.Repository<ExpectedTransaction, Guid>()
            .GetNoTrackingEntities()
            .Where(t => t.RecurringTransactionTemplateId == templateId)
            .OrderBy(t => t.ExpectedDate);

        return await Mapper.ProjectTo<ExpectedTransactionViewModel>(query).ToListAsync();
    }

    /// <summary>
    ///     (EN) Gets a list of expected transactions associated with a specific account.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến liên quan đến một tài khoản cụ thể.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>A list of expected transactions by account.</returns>
    public async Task<IEnumerable<ExpectedTransactionViewModel>> GetTransactionsByAccountAsync(Guid accountId)
    {
        var query = _unitOfWork.Repository<ExpectedTransaction, Guid>()
            .GetNoTrackingEntities()
            .Where(t => t.AccountId == accountId)
            .OrderBy(t => t.ExpectedDate);

        return await Mapper.ProjectTo<ExpectedTransactionViewModel>(query).ToListAsync();
    }

    /// <summary>
    ///     (EN) Gets a list of expected transactions for a user within a specified date range.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến cho người dùng trong một khoảng ngày cụ thể.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>A list of expected transactions within the date range.</returns>
    public async Task<IEnumerable<ExpectedTransactionViewModel>> GetTransactionsByDateRangeAsync(Guid userId,
        DateTime startDate, DateTime endDate)
    {
        var query = _unitOfWork.Repository<ExpectedTransaction, Guid>()
            .GetNoTrackingEntities()
            .Where(t => t.UserId == userId &&
                        t.ExpectedDate >= startDate.Date &&
                        t.ExpectedDate <= endDate.Date)
            .OrderBy(t => t.ExpectedDate);

        return await Mapper.ProjectTo<ExpectedTransactionViewModel>(query).ToListAsync();
    }

    /// <summary>
    ///     (EN) Confirms an expected transaction by linking it to an actual transaction.<br />
    ///     (VI) Xác nhận một giao dịch dự kiến bằng cách liên kết nó với một giao dịch thực tế.
    /// </summary>
    /// <param name="expectedTransactionId">The ID of the expected transaction to confirm.</param>
    /// <param name="actualTransactionId">The ID of the actual transaction.</param>
    /// <returns>True if the confirmation was successful, false otherwise.</returns>
    public async Task<bool> ConfirmExpectedTransactionAsync(Guid expectedTransactionId, Guid actualTransactionId)
    {
        await using var trans = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var expectedTransaction = await _unitOfWork.Repository<ExpectedTransaction, Guid>()
                .GetByIdAsync(expectedTransactionId);

            if (expectedTransaction == null || expectedTransaction.Status != ExpectedTransactionStatus.Pending)
                return false;

            expectedTransaction.Status = ExpectedTransactionStatus.Confirmed;
            expectedTransaction.ActualTransactionId = actualTransactionId;
            expectedTransaction.ProcessedAt = DateTime.UtcNow;
            expectedTransaction.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<ExpectedTransaction, Guid>().UpdateAsync(expectedTransaction);
            await _unitOfWork.SaveChangesAsync();

            await trans.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await trans.RollbackAsync();
            logger.LogError(ex, "Error confirming expected transaction {ExpectedTransactionId}", expectedTransactionId);
            return false;
        }
    }

    /// <summary>
    ///     (EN) Cancels an expected transaction with a specified reason.<br />
    ///     (VI) Hủy một giao dịch dự kiến với lý do cụ thể.
    /// </summary>
    /// <param name="expectedTransactionId">The ID of the expected transaction to cancel.</param>
    /// <param name="reason">The reason for canceling the transaction.</param>
    /// <returns>True if the cancellation was successful, false otherwise.</returns>
    public async Task<bool> CancelExpectedTransactionAsync(Guid expectedTransactionId, string reason)
    {
        await using var trans = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var expectedTransaction = await _unitOfWork.Repository<ExpectedTransaction, Guid>()
                .GetByIdAsync(expectedTransactionId);

            if (expectedTransaction == null || expectedTransaction.Status != ExpectedTransactionStatus.Pending)
                return false;

            expectedTransaction.Status = ExpectedTransactionStatus.Cancelled;
            expectedTransaction.AdjustmentReason = reason;
            expectedTransaction.ProcessedAt = DateTime.UtcNow;
            expectedTransaction.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<ExpectedTransaction, Guid>().UpdateAsync(expectedTransaction);
            await _unitOfWork.SaveChangesAsync();

            await trans.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await trans.RollbackAsync();
            logger.LogError(ex, "Error cancelling expected transaction {ExpectedTransactionId}", expectedTransactionId);
            return false;
        }
    }

    /// <summary>
    ///     (EN) Adjusts the amount of an expected transaction with a specified reason.<br />
    ///     (VI) Điều chỉnh số tiền của một giao dịch dự kiến với lý do cụ thể.
    /// </summary>
    /// <param name="expectedTransactionId">The ID of the expected transaction to adjust.</param>
    /// <param name="newAmount">The new amount for the transaction.</param>
    /// <param name="reason">The reason for adjusting the transaction.</param>
    /// <returns>True if the adjustment was successful, false otherwise.</returns>
    public async Task<bool> AdjustExpectedTransactionAsync(Guid expectedTransactionId, decimal newAmount, string reason)
    {
        await using var trans = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var expectedTransaction = await _unitOfWork.Repository<ExpectedTransaction, Guid>()
                .GetByIdAsync(expectedTransactionId);

            if (expectedTransaction == null || expectedTransaction.Status != ExpectedTransactionStatus.Pending)
                return false;

            if (!expectedTransaction.IsAdjusted)
                expectedTransaction.OriginalAmount = expectedTransaction.ExpectedAmount;

            expectedTransaction.ExpectedAmount = newAmount;
            expectedTransaction.IsAdjusted = true;
            expectedTransaction.AdjustmentReason = reason;
            expectedTransaction.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<ExpectedTransaction, Guid>().UpdateAsync(expectedTransaction);
            await _unitOfWork.SaveChangesAsync();

            await trans.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await trans.RollbackAsync();
            logger.LogError(ex, "Error adjusting expected transaction {ExpectedTransactionId}", expectedTransactionId);
            return false;
        }
    }

    /// <summary>
    ///     (EN) Gets the cash flow forecast for a user within a specified date range.<br />
    ///     (VI) Lấy dự báo dòng tiền cho người dùng trong một khoảng ngày cụ thể.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="startDate">The start date of the forecast.</param>
    /// <param name="endDate">The end date of the forecast.</param>
    /// <returns>The cash flow forecast amount.</returns>
    public async Task<decimal> GetCashFlowForecastAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        var expectedTransactions = (await GetTransactionsByDateRangeAsync(userId, startDate, endDate)).ToList();

        var totalIncome = expectedTransactions
            .Where(t => t is
                { TransactionType: RecurringTransactionType.Income, Status: ExpectedTransactionStatus.Pending })
            .Sum(t => t.ExpectedAmount);

        var totalExpenses = expectedTransactions
            .Where(t => t is
                { TransactionType: RecurringTransactionType.Expense, Status: ExpectedTransactionStatus.Pending })
            .Sum(t => t.ExpectedAmount);

        return totalIncome - totalExpenses;
    }

    /// <summary>
    ///     (EN) Gets the category-based cash flow forecast for a user within a specified date range.<br />
    ///     (VI) Lấy dự báo dòng tiền theo danh mục cho người dùng trong một khoảng ngày cụ thể.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="startDate">The start date of the forecast.</param>
    /// <param name="endDate">The end date of the forecast.</param>
    /// <returns>A dictionary where keys are categories and values are the forecast amounts.</returns>
    public async Task<Dictionary<string, decimal>> GetCategoryForecastAsync(Guid userId, DateTime startDate,
        DateTime endDate)
    {
        var expectedTransactions = await GetTransactionsByDateRangeAsync(userId, startDate, endDate);

        return expectedTransactions
            .Where(t => t.Status == ExpectedTransactionStatus.Pending && !string.IsNullOrEmpty(t.Category))
            .GroupBy(t => t.Category!)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(t =>
                    t.TransactionType == RecurringTransactionType.Income ? t.ExpectedAmount : -t.ExpectedAmount)
            );
    }

    /// <summary>
    ///     (EN) Creates a new expected transaction.<br />
    ///     (VI) Tạo một giao dịch dự kiến mới.
    /// </summary>
    /// <param name="request">The create request.</param>
    /// <returns>The created expected transaction view model.</returns>
    public override async Task<ExpectedTransactionViewModel?> CreateAsync(ExpectedTransactionCreateRequest request)
    {
        // Set default values
        if (request.Status == default) request.Status = ExpectedTransactionStatus.Pending;

        return await base.CreateAsync(request);
    }
}