using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Enums;
using CoreFinance.Application.Interfaces;

namespace CoreFinance.Application.DTOs.Account;

/// <summary>
///     Represents a request to create a new account. (EN)<br />
///     Đại diện cho request tạo tài khoản mới. (VI)
/// </summary>
public class AccountCreateRequest : BaseCreateRequest, IUserRequest
{
    /// <summary>
    ///     The ID of the user who owns the account (optional). (EN)<br />
    ///     ID của người dùng sở hữu tài khoản (tùy chọn). (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     The name of the account. (EN)<br />
    ///     Tên tài khoản. (VI)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     The type of the account (e.g., checking, savings, credit card). (EN)<br />
    ///     Loại tài khoản (ví dụ: tài khoản thanh toán, tiết kiệm, thẻ tín dụng). (VI)
    /// </summary>
    public AccountType Type { get; set; }

    /// <summary>
    ///     The card number associated with the account (optional). (EN)<br />
    ///     Số thẻ liên kết với tài khoản (tùy chọn). (VI)
    /// </summary>
    public string? CardNumber { get; set; }

    /// <summary>
    ///     The currency of the account (e.g., "VND", "USD"). (EN)<br />
    ///     Tiền tệ của tài khoản (ví dụ: "VND", "USD"). (VI)
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    ///     The initial balance of the account. (EN)<br />
    ///     Số dư ban đầu của tài khoản. (VI)
    /// </summary>
    public decimal InitialBalance { get; set; }

    /// <summary>
    ///     The available credit limit for credit card accounts (optional). (EN)<br />
    ///     Hạn mức tín dụng khả dụng cho tài khoản thẻ tín dụng (tùy chọn). (VI)
    /// </summary>
    public decimal? AvailableLimit { get; set; }

    /// <summary>
    ///     Unique code identifier for account mapping (EN)<br/>
    ///     Mã định danh duy nhất cho ánh xạ tài khoản (VI)
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    ///     Sequence number for ordering accounts, supports fractional values (EN)<br/>
    ///     Số thứ tự để sắp xếp tài khoản, hỗ trợ giá trị phân số (VI)
    /// </summary>
    public double? No { get; set; }
}