using CoreFinance.Application.DTOs.Transaction;
using CoreFinance.Application.Services.Base;
using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Entities;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Application.Interfaces;

/// <summary>
///     (EN) Represents the service interface for managing transactions.<br />
///     (VI) Đại diện cho interface dịch vụ quản lý giao dịch.
/// </summary>
public interface ITransactionService :
    IBaseService<Transaction, TransactionCreateRequest, TransactionUpdateRequest, TransactionViewModel, Guid>
{
    /// <summary>
    ///     (EN) Gets a paginated list of transactions.<br />
    ///     (VI) Lấy danh sách giao dịch có phân trang.
    /// </summary>
    /// <param name="request">The filter request body.</param>
    /// <returns>A paginated list of transactions.</returns>
    Task<IBasePaging<TransactionViewModel>?> GetPagingAsync(IFilterBodyRequest request);
}