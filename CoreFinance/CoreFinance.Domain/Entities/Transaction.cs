using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CoreFinance.Domain.Enums;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Domain.Entities;

public class Transaction : UserOwnedEntity<Guid>
{
    /// <summary>
    ///     Foreign key linking to account (EN)
    ///     Khóa ngoại liên kết với tài khoản (VI)
    /// </summary>
    public Guid? AccountId { get; set; }

    /// <summary>
    ///     Transaction date and time (EN)
    ///     Ngày và giờ giao dịch (VI)
    /// </summary>
    [Required]
    public DateTime TransactionDate { get; set; }

    /// <summary>
    ///     Revenue amount (EN)
    ///     Số tiền thu vào (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Revenue amount must be non-negative")]
    public decimal RevenueAmount { get; set; }

    /// <summary>
    ///     Spent amount (EN)
    ///     Số tiền chi ra (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Spent amount must be non-negative")]
    public decimal SpentAmount { get; set; }

    /// <summary>
    ///     Transaction description (EN)
    ///     Mô tả giao dịch (VI)
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    ///     Balance after transaction (EN)
    ///     Số dư sau giao dịch (VI)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }

    /// <summary>
    ///     Balance compared to app notification (EN)<br />
    ///     Số dư so với thông báo giao dịch trên app (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? BalanceCompare { get; set; }

    /// <summary>
    ///     Available limit (credit card only) (EN)<br />
    ///     Số tiền khả dụng (chỉ áp dụng cho thẻ tín dụng) (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Available limit must be non-negative")]
    public decimal? AvailableLimit { get; set; }

    /// <summary>
    ///     Available limit compared to app notification (credit card only) (EN)<br />
    ///     Số tiền khả dụng so với thông báo giao dịch trên app (chỉ áp dụng cho thẻ tín dụng) (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? AvailableLimitCompare { get; set; }

    /// <summary>
    ///     Transaction code (EN)<br />
    ///     Mã giao dịch (VI)
    /// </summary>
    [MaxLength(50)]
    public string? TransactionCode { get; set; }

    /// <summary>
    ///     Synced with MISA (EN)<br />
    ///     Đánh dấu đã đồng bộ với MISA (VI)
    /// </summary>
    public bool SyncMisa { get; set; }

    /// <summary>
    ///     Synced with SMS (credit card only) (EN)<br />
    ///     Đánh dấu đã đồng bộ với SMS (chỉ áp dụng cho thẻ tín dụng) (VI)
    /// </summary>
    public bool SyncSms { get; set; }

    /// <summary>
    ///     Marked for Vietnam market (EN)<br />
    ///     Đánh dấu giao dịch dành cho thị trường Việt Nam (VI)
    /// </summary>
    public bool Vn { get; set; }

    /// <summary>
    ///     Transaction category summary (EN)<br />
    ///     Tóm tắt danh mục giao dịch (VI)
    /// </summary>
    [MaxLength(200)]
    public string? CategorySummary { get; set; }

    /// <summary>
    ///     Additional note for transaction (EN)<br />
    ///     Ghi chú bổ sung cho giao dịch (VI)
    /// </summary>
    [MaxLength(1000)]
    public string? Note { get; set; }

    /// <summary>
    ///     Import source (e.g. file, app, etc.) (EN)<br />
    ///     Nguồn nhập giao dịch (ví dụ: import từ file, app, v.v.) (VI)
    /// </summary>
    [MaxLength(100)]
    public string? ImportFrom { get; set; }

    /// <summary>
    ///     Increased credit limit (credit card only) (EN)<br />
    ///     Hạn mức tín dụng tăng thêm (chỉ áp dụng cho thẻ tín dụng) (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Increased credit limit must be non-negative")]
    public decimal? IncreaseCreditLimit { get; set; }

    /// <summary>
    ///     Used percent of credit limit (credit card only) (EN)<br />
    ///     Phần trăm hạn mức đã sử dụng (chỉ áp dụng cho thẻ tín dụng) (VI)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    [Range(0, 100, ErrorMessage = "Used percent must be between 0 and 100")]
    public decimal? UsedPercent { get; set; }

    /// <summary>
    ///     Transaction category type (EN)<br />
    ///     Loại giao dịch (thu, chi, chuyển khoản, phí, khác) (VI)
    /// </summary>
    [Required]
    public CategoryType CategoryType { get; set; }

    /// <summary>
    ///     Group of transactions (statement period) (EN)<br />
    ///     Nhóm giao dịch thuộc kỳ sao kê nào (VI)
    /// </summary>
    [MaxLength(100)]
    public string? Group { get; set; }

    /// <summary>
    ///     Sequence number for ordering transactions, supports fractional values for inserting between existing records (EN)<br />
    ///     Số thứ tự để sắp xếp giao dịch, hỗ trợ giá trị phân số để chèn giữa các bản ghi hiện có (VI)
    /// </summary>
    public double? No { get; set; }

    /// <summary>
    ///     Navigation property: link to account (EN)<br />
    ///     Thuộc tính điều hướng: liên kết với tài khoản (VI)
    /// </summary>
    public Account? Account { get; set; }
}