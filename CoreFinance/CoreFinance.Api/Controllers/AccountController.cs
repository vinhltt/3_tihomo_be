using CoreFinance.Api.Controllers.Base;
using CoreFinance.Application.DTOs.Account;
using CoreFinance.Application.Interfaces;
using CoreFinance.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.DTOs;

namespace CoreFinance.Api.Controllers;

/// <summary>
///     Controller for managing accounts. (EN)<br />
///     Controller để quản lý các tài khoản. (VI)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccountController(
    ILogger<AccountController> logger,
    IAccountService accountService)
    : CrudController<Account, AccountCreateRequest,
        AccountUpdateRequest, AccountViewModel, Guid>(logger,
        accountService)
{
    /// <summary>
    ///     Gets a paginated list of accounts based on a filter request. (EN)<br />
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public override async Task<ActionResult<IBasePaging<AccountViewModel>>> GetPagingAsync(FilterBodyRequest request)
    {
        var result = await accountService.GetPagingAsync(request);
        return Ok(result);
    }

    /// <summary>
    ///     (EN) Gets an account by its unique code identifier.<br />
    ///     (VI) Lấy tài khoản theo mã định danh duy nhất.
    /// </summary>
    /// <param name="code">Account code (snake_case format)</param>
    /// <returns>Account details or 404 if not found</returns>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(AccountViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountViewModel>> GetByCodeAsync(string code)
    {
        // Validate code format
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("Code cannot be empty");
        }

        // Call service
        var result = await accountService.GetByCodeAsync(code);

        if (result == null)
        {
            return NotFound($"Account with code '{code}' not found");
        }

        return Ok(result);
    }

    /// <summary>
    ///     (EN) Gets a list of account selections for UI components.<br />
    ///     (VI) Lấy danh sách lựa chọn tài khoản cho các thành phần giao diện người dùng.
    /// </summary>
    /// <returns></returns>
    [HttpGet("selections")]
    public async Task<ActionResult<IBasePaging<AccountViewModel>>> GetAccountSelectionAsync()
    {
        var result = await accountService.GetAccountSelectionAsync();
        return Ok(result);
    }
}