using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CoreFinance.Domain.Enums;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Domain.Entities;

public class Account : UserOwnedEntity<Guid>
{
    /// <summary>
    ///     Account name for easy identification (EN)
    ///     Tên tài khoản để dễ nhận biết (VI)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Type of financial account (EN)
    ///     Loại tài khoản tài chính (VI)
    /// </summary>
    [Required]
    public AccountType Type { get; set; }

    /// <summary>
    ///     Card number (optional, for card accounts) (EN)
    ///     Số thẻ (tùy chọn, cho tài khoản thẻ) (VI)
    /// </summary>
    [MaxLength(32)]
    public string? CardNumber { get; set; }

    /// <summary>
    ///     Currency of the account (EN)
    ///     Đơn vị tiền tệ của tài khoản (VI)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Initial balance when the account was created (EN)
    ///     Số dư ban đầu khi tài khoản được tạo (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal InitialBalance { get; set; }

    /// <summary>
    ///     Current balance of the account (EN)
    ///     Số dư hiện tại của tài khoản (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentBalance { get; set; }

    /// <summary>
    ///     Available credit limit (credit card only) (EN)
    ///     Hạn mức tín dụng khả dụng (chỉ áp dụng cho thẻ tín dụng) (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? AvailableLimit { get; set; }

    /// <summary>
    ///     Whether the account is active (EN)
    ///     Tài khoản có đang hoạt động hay không (VI)
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>
    ///     Unique code identifier for account, used for mapping from external sources (EN)<br/>
    ///     Mã định danh duy nhất cho tài khoản, dùng để ánh xạ từ nguồn bên ngoài (VI)
    /// </summary>
    [MaxLength(50)]
    [RegularExpression(@"^[a-z0-9_]+$", ErrorMessage = "Code must be snake_case (lowercase letters, numbers, underscores only)")]
    public string? Code { get; set; }

    /// <summary>
    ///     Sequence number for ordering accounts, supports fractional values for inserting between existing records (EN)<br/>
    ///     Số thứ tự để sắp xếp tài khoản, hỗ trợ giá trị phân số để chèn giữa các bản ghi hiện có (VI)
    /// </summary>
    public double? No { get; set; }

    /// <summary>
    ///     ID of the last synced transaction from external source (e.g., Google Sheets), used for incremental sync (EN)<br/>
    ///     ID của giao dịch đã đồng bộ cuối cùng từ nguồn bên ngoài (ví dụ: Google Sheets), dùng cho đồng bộ tăng dần (VI)
    /// </summary>
    public Guid? LastSyncTransactionId { get; set; }

    // Navigation property
    [InverseProperty("Account")] public ICollection<Transaction>? Transactions { get; set; }
}