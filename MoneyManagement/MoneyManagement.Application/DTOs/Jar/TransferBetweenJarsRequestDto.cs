using System.ComponentModel.DataAnnotations;

namespace MoneyManagement.Application.DTOs.Jar;

/// <summary>
///     Request model for transferring money between jars (EN)<br />
///     Model yêu cầu để chuyển tiền giữa các lọ (VI)
/// </summary>
public class TransferBetweenJarsRequestDto
{
    /// <summary>
    ///     Source jar identifier (EN)<br />
    ///     Định danh lọ nguồn (VI)
    /// </summary>
    [Required(ErrorMessage = "Source jar ID is required")]
    public Guid FromJarId { get; set; }

    /// <summary>
    ///     Destination jar identifier (EN)<br />
    ///     Định danh lọ đích (VI)
    /// </summary>
    [Required(ErrorMessage = "Destination jar ID is required")]
    public Guid ToJarId { get; set; }

    /// <summary>
    ///     Transfer amount (EN)<br />
    ///     Số tiền chuyển (VI)
    /// </summary>
    [Required(ErrorMessage = "Transfer amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Transfer amount must be greater than 0")]
    public decimal Amount { get; set; }

    /// <summary>
    ///     Description for this transfer (EN)<br />
    ///     Mô tả cho việc chuyển tiền này (VI)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    ///     Transfer date (EN)<br />
    ///     Ngày chuyển tiền (VI)
    /// </summary>
    public DateTime? TransferDate { get; set; }
}