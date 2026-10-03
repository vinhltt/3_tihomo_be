using AutoMapper;
using CoreFinance.Application.DTOs.Transaction;
using CoreFinance.Application.Interfaces;
using CoreFinance.Application.Services.Base;
using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.EntityFrameworkUtilities;

namespace CoreFinance.Application.Services;

/// <summary>
///     (EN) Service for managing transactions.<br />
///     (VI) Dịch vụ quản lý giao dịch.
/// </summary>
public class TransactionService(
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<TransactionService> logger)
    : BaseService<Transaction, TransactionCreateRequest, TransactionUpdateRequest, TransactionViewModel, Guid>(mapper,
            unitOfWork, logger),
        ITransactionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    ///     (EN) Gets a paginated list of transactions based on a filter request.<br />
    ///     (VI) Lấy danh sách giao dịch có phân trang dựa trên yêu cầu lọc.
    /// </summary>
    /// <param name="request">The filter request body.</param>
    /// <returns>A paginated list of transaction view models.</returns>
    public async Task<IBasePaging<TransactionViewModel>?> GetPagingAsync(IFilterBodyRequest request)
    {
        var query =
            Mapper.ProjectTo<TransactionViewModel>(_unitOfWork.Repository<Transaction, Guid>()
                .GetNoTrackingEntities());

        // Example: filter by Description or other fields if needed
        if (!string.IsNullOrEmpty(request.SearchValue))
            query = query.Where(e => e.Description != null
                                     && (e.Description.ToLower().Contains(request.SearchValue.ToLower())
                                         || e.CategorySummary!.ToLower().Contains(request.SearchValue.ToLower())));

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
    ///     (EN) Creates a new transaction with automatic balance calculation.<br />
    ///     (VI) Tạo giao dịch mới với tính toán số dư tự động.
    /// </summary>
    /// <param name="request">The transaction creation request.</param>
    /// <returns>The created transaction view model.</returns>
    public override async Task<TransactionViewModel?> CreateAsync(TransactionCreateRequest request)
    {
        //var calculatedBalance = await CalculateBalanceForTransactionAsync(
        //    request.AccountId,
        //    request.TransactionDate,
        //    request.RevenueAmount,
        //    request.SpentAmount);
        //request.Balance = calculatedBalance;

        var entity = Mapper.Map<Transaction>(request);

        await _unitOfWork.Repository<Transaction, Guid>().CreateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        // Recalculate subsequent transactions if balance was provided
        //await RecalculateSubsequentBalancesAsync(request.AccountId, request.TransactionDate);

        return Mapper.Map<TransactionViewModel>(entity);
    }

    /// <summary>
    ///     (EN) Calculates balance for a transaction based on previous transactions.<br />
    ///     (VI) Tính toán số dư cho giao dịch dựa trên các giao dịch trước đó.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="transactionDate">The transaction date.</param>
    /// <param name="revenueAmount">The revenue amount.</param>
    /// <param name="spentAmount">The spent amount.</param>
    /// <returns>The calculated balance or null if cannot be calculated.</returns>
    private async Task<decimal> CalculateBalanceForTransactionAsync(
        Guid accountId,
        DateTime transactionDate,
        decimal revenueAmount,
        decimal spentAmount)
    {
        try
        {
            // Find the most recent transaction before the current transaction date for the same account
            var previousTransaction = await _unitOfWork.Repository<Transaction, Guid>()
                .GetNoTrackingEntities()
                .Where(t => t.AccountId == accountId && t.TransactionDate < transactionDate)
                .OrderByDescending(t => t.TransactionDate)
                .FirstOrDefaultAsync();

            if (previousTransaction == null)
            {
                logger.LogWarning("No previous transaction with balance found for account {AccountId}", accountId);
                return 0;
            }

            // Calculate new balance: previous balance + revenue - spent
            var calculatedBalance = previousTransaction.Balance + revenueAmount - spentAmount;

            logger.LogInformation("Balance calculated for account {AccountId}: {Balance}", accountId,
                calculatedBalance);
            return calculatedBalance;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating balance for account {AccountId}", accountId);
            return 0;
        }
    }

    /// <summary>
    ///     (EN) Recalculates balances for all transactions after a given date for an account.<br />
    ///     (VI) Tính lại số dư cho tất cả giao dịch sau một ngày nhất định của tài khoản.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="fromDate">The date from which to recalculate.</param>
    private async Task RecalculateSubsequentBalancesAsync(Guid accountId, DateTime fromDate)
    {
        var subsequentTransactions = await _unitOfWork.Repository<Transaction, Guid>()
            .GetNoTrackingEntities()
            .Where(t => t.AccountId == accountId && t.TransactionDate > fromDate)
            .OrderBy(t => t.TransactionDate)
            .ToListAsync();

        foreach (var transaction in subsequentTransactions)
        {
            var calculatedBalance = await CalculateBalanceForTransactionAsync(
                accountId,
                transaction.TransactionDate,
                transaction.RevenueAmount,
                transaction.SpentAmount);
            // Update the transaction entity directly
            var entityToUpdate = await _unitOfWork.Repository<Transaction, Guid>().GetByIdAsync(transaction.Id);
            if (entityToUpdate == null)
                continue;
            entityToUpdate.Balance = calculatedBalance;
            await _unitOfWork.Repository<Transaction, Guid>().UpdateAsync(entityToUpdate);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}