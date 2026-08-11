using System.ComponentModel.DataAnnotations;

namespace MoneyManagement.Application.DTOs.Jar;

public class JarWithdrawRequest
{
    [Required] public Guid JarId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "Reference cannot exceed 100 characters")]
    public string? Reference { get; set; }
}

public class JarWithdrawResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal PreviousBalance { get; set; }
    public decimal WithdrawnAmount { get; set; }
    public decimal NewBalance { get; set; }
    public Guid TransactionId { get; set; }
    public DateTime WithdrawDate { get; set; }
}