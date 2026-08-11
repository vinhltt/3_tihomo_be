using CoreFinance.Domain.Enums;
using Shared.EntityFramework.DTOs;

namespace CoreFinance.Application.DTOs.Transaction;

/// <summary>
///     Represents a request to update an existing transaction. (EN)<br />
///     Đại diện cho request cập nhật giao dịch hiện có. (VI)
/// </summary>
public class TransactionUpdateRequest : BaseUpdateRequest<Guid>
{
    /// <summary>
    ///     The ID of the account associated with the transaction. (EN)<br />
    ///     ID của tài khoản liên quan đến giao dịch. (VI)
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    ///     The ID of the user who performed the transaction (optional). (EN)<br />
    ///     ID của người dùng thực hiện giao dịch (tùy chọn). (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     The date and time of the transaction. (EN)<br />
    ///     Ngày và giờ thực hiện giao dịch. (VI)
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    ///     The revenue amount of the transaction. (EN)<br />
    ///     Số tiền thu vào của giao dịch. (VI)
    /// </summary>
    public decimal RevenueAmount { get; set; }

    /// <summary>
    ///     The spent amount of the transaction. (EN)<br />
    ///     Số tiền chi ra của giao dịch. (VI)
    /// </summary>
    public decimal SpentAmount { get; set; }

    /// <summary>
    ///     A description of the transaction (optional). (EN)<br />
    ///     Mô tả về giao dịch (tùy chọn). (VI)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     The balance of the account after the transaction (optional, can be calculated automatically). (EN)<br />
    ///     Số dư tài khoản sau giao dịch (tùy chọn, có thể được tính tự động). (VI)
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    ///     The balance of the account before the transaction for comparison (optional). (EN)<br />
    ///     Số dư tài khoản trước giao dịch để so sánh (tùy chọn). (VI)
    /// </summary>
    public decimal? BalanceCompare { get; set; }

    /// <summary>
    ///     The available credit limit after the transaction for credit card accounts (optional). (EN)<br />
    ///     Hạn mức tín dụng khả dụng sau giao dịch cho tài khoản thẻ tín dụng (tùy chọn). (VI)
    /// </summary>
    public decimal? AvailableLimit { get; set; }

    /// <summary>
    ///     The available credit limit before the transaction for comparison (optional). (EN)<br />
    ///     Hạn mức tín dụng khả dụng trước giao dịch để so sánh (tùy chọn). (VI)
    /// </summary>
    public decimal? AvailableLimitCompare { get; set; }

    /// <summary>
    ///     The transaction code (optional). (EN)<br />
    ///     Mã giao dịch (tùy chọn). (VI)
    /// </summary>
    public string? TransactionCode { get; set; }

    /// <summary>
    ///     Indicates if the transaction was synced from Misa (optional). (EN)<br />
    ///     Cho biết giao dịch có được đồng bộ từ Misa hay không (tùy chọn). (VI)
    /// </summary>
    public bool SyncMisa { get; set; }

    /// <summary>
    ///     Indicates if the transaction was synced from SMS (optional). (EN)<br />
    ///     Cho biết giao dịch có được đồng bộ từ SMS hay không (tùy chọn). (VI)
    /// </summary>
    public bool SyncSms { get; set; }

    /// <summary>
    ///     Indicates if the transaction is related to Vietnam Dong currency (optional). (EN)<br />
    ///     Cho biết giao dịch có liên quan đến tiền tệ Việt Nam Đồng hay không (tùy chọn). (VI)
    /// </summary>
    public bool Vn { get; set; }

    /// <summary>
    ///     A summary of the transaction category (optional). (EN)<br />
    ///     Tóm tắt danh mục giao dịch (tùy chọn). (VI)
    /// </summary>
    public string? CategorySummary { get; set; }

    /// <summary>
    ///     Additional notes about the transaction (optional). (EN)<br />
    ///     Ghi chú bổ sung về giao dịch (tùy chọn). (VI)
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    ///     The source from which the transaction was imported (optional). (EN)<br />
    ///     Nguồn mà giao dịch được import từ đó (tùy chọn). (VI)
    /// </summary>
    public string? ImportFrom { get; set; }

    /// <summary>
    ///     The amount by which the credit limit was increased (optional). (EN)<br />
    ///     Số tiền hạn mức tín dụng được tăng thêm (tùy chọn). (VI)
    /// </summary>
    public decimal? IncreaseCreditLimit { get; set; }

    /// <summary>
    ///     The percentage of the credit limit used (optional). (EN)<br />
    ///     Tỷ lệ phần trăm hạn mức tín dụng đã sử dụng (tùy chọn). (VI)
    /// </summary>
    public decimal? UsedPercent { get; set; }

    /// <summary>
    ///     The type of transaction category. (EN)<br />
    ///     Loại danh mục giao dịch. (VI)
    /// </summary>
    public CategoryType CategoryType { get; set; }

    /// <summary>
    ///     The transaction group (optional). (EN)<br />
    ///     Nhóm giao dịch (tùy chọn). (VI)
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    ///     Sequence number for ordering transactions, supports fractional values for inserting between existing records (EN)<br />
    ///     Số thứ tự để sắp xếp giao dịch, hỗ trợ giá trị phân số để chèn giữa các bản ghi hiện có (VI)
    /// </summary>
    public double? No { get; set; }
}