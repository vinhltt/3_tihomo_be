using System.ComponentModel.DataAnnotations;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.DTOs.Jar;

/// <summary>
///     Request model for distributing income across jars (EN)<br />
///     Model yêu cầu để phân phối thu nhập giữa các lọ (VI)
/// </summary>
public class DistributeIncomeRequestDto
{
    /// <summary>
    ///     Total income amount to distribute (EN)<br />
    ///     Tổng số tiền thu nhập cần phân phối (VI)
    /// </summary>
    [Required(ErrorMessage = "Income amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Income amount must be greater than 0")]
    public decimal IncomeAmount { get; set; }

    /// <summary>
    ///     Custom allocation ratios for specific jars (EN)<br />
    ///     Tỷ lệ phân bổ tùy chỉnh cho các lọ cụ thể (VI)
    /// </summary>
    public Dictionary<JarType, decimal>? CustomRatios { get; set; }

    /// <summary>
    ///     Selected jar IDs to include in distribution (EN)<br />
    ///     ID các lọ được chọn để bao gồm trong phân phối (VI)
    /// </summary>
    public List<Guid>? SelectedJarIds { get; set; }

    /// <summary>
    ///     Description for this income distribution (EN)<br />
    ///     Mô tả cho việc phân phối thu nhập này (VI)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    ///     Source of the income (EN)<br />
    ///     Nguồn thu nhập (VI)
    /// </summary>
    [MaxLength(200, ErrorMessage = "Source cannot exceed 200 characters")]
    public string? Source { get; set; }

    /// <summary>
    ///     Income date (EN)<br />
    ///     Ngày thu nhập (VI)
    /// </summary>
    public DateTime? IncomeDate { get; set; }

    /// <summary>
    ///     Whether to use default Six Jars allocation (EN)<br />
    ///     Có sử dụng phân bổ Six Jars mặc định không (VI)
    /// </summary>
    public bool UseDefaultAllocation { get; set; } = true;
}