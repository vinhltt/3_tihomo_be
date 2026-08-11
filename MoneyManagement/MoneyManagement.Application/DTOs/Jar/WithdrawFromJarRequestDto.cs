using System.ComponentModel.DataAnnotations;

namespace MoneyManagement.Application.DTOs.Jar;

/// <summary>
///     Request model for withdrawing money from a jar (EN)<br />
///     Model yêu cầu để rút tiền từ lọ (VI)
/// </summary>
public class WithdrawFromJarRequestDto
{
    /// <summary>
    ///     Amount to withdraw from the jar (EN)<br />
    ///     Số tiền cần rút từ lọ (VI)
    /// </summary>
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    /// <summary>
    ///     Description for this withdrawal (EN)<br />
    ///     Mô tả cho việc rút tiền này (VI)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    ///     Purpose of the withdrawal (EN)<br />
    ///     Mục đích rút tiền (VI)
    /// </summary>
    [MaxLength(200, ErrorMessage = "Purpose cannot exceed 200 characters")]
    public string? Purpose { get; set; }

    /// <summary>
    ///     Transaction date (EN)<br />
    ///     Ngày giao dịch (VI)
    /// </summary>
    public DateTime? TransactionDate { get; set; }
}