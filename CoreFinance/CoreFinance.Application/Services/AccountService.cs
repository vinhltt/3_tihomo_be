using AutoMapper;
using CoreFinance.Application.DTOs.Account;
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
///     (EN) Service for managing accounts.<br />
///     (VI) Dịch vụ quản lý tài khoản.
/// </summary>
public class AccountService(
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<AccountService> logger)
    : BaseService<Account, AccountCreateRequest, AccountUpdateRequest, AccountViewModel, Guid>(mapper, unitOfWork,
            logger),
        IAccountService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    ///     (EN) Gets a paginated list of accounts based on a filter request.<br />
    ///     (VI) Lấy danh sách tài khoản có phân trang dựa trên yêu cầu lọc.
    /// </summary>
    /// <param name="request">The filter request body.</param>
    /// <returns>A paginated list of account view models.</returns>
    public async Task<IBasePaging<AccountViewModel>?> GetPagingAsync(IFilterBodyRequest request)
    {
        var query =
            Mapper.ProjectTo<AccountViewModel>(_unitOfWork.Repository<Account, Guid>()
                .GetNoTrackingEntities());

        if (!string.IsNullOrEmpty(request.SearchValue))
            query = query.Where(e => e.Name!.Contains(request.SearchValue, StringComparison.CurrentCultureIgnoreCase));

        return await query.ToPagingAsync(request);
    }

    public async Task<List<AccountSelectionViewModel>?> GetAccountSelectionAsync()
    {
        var query =
            Mapper.ProjectTo<AccountSelectionViewModel>(_unitOfWork.Repository<Account, Guid>()
                .GetNoTrackingEntities());

        return await query.ToListAsync();
    }

    /// <summary>
    ///     (EN) Gets an account by its unique code identifier.<br />
    ///     (VI) Lấy tài khoản theo mã định danh duy nhất.
    /// </summary>
    /// <param name="code">The unique code of the account.</param>
    /// <returns>The account view model if found, otherwise null.</returns>
    public async Task<AccountViewModel?> GetByCodeAsync(string code)
    {
        // Return null if code is null or whitespace
        if (string.IsNullOrWhiteSpace(code))
            return null;

        // Query accounts with matching code (case-sensitive) and active status
        var query = Mapper.ProjectTo<AccountViewModel>(
            _unitOfWork.Repository<Account, Guid>()
                .GetNoTrackingEntities()
                .Where(a => a.Code == code && a.IsActive)
        );

        // Return first matching account or null
        return await query.FirstOrDefaultAsync();
    }
}