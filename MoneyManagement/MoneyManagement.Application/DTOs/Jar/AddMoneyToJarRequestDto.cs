using System.ComponentModel.DataAnnotations;

namespace MoneyManagement.Application.DTOs.Jar;

/// <summary>
///     Request model for adding money to a jar (EN)<br />
///     Model yêu cầu để thêm tiền vào lọ (VI)
/// </summary>
public class AddMoneyToJarRequestDto
{
    /// <summary>
    ///     Amount to add to the jar (EN)<br />
    ///     Số tiền cần thêm vào lọ (VI)
    /// </summary>
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    /// <summary>
    ///     Description for this addition (EN)<br />
    ///     Mô tả cho việc thêm tiền này (VI)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    ///     Source of the money (optional) (EN)<br />
    ///     Nguồn tiền (tùy chọn) (VI)
    /// </summary>
    [MaxLength(200, ErrorMessage = "Source cannot exceed 200 characters")]
    public string? Source { get; set; }

    /// <summary>
    ///     Transaction date (EN)<br />
    ///     Ngày giao dịch (VI)
    /// </summary>
    public DateTime? TransactionDate { get; set; }
}