using FluentValidation;
using MoneyManagement.Application.DTOs.SharedExpense;

namespace MoneyManagement.Application.Validators;

/// <summary>
///     Validator for UpdateSharedExpenseRequestDto (EN)<br />
///     Validator cho UpdateSharedExpenseRequestDto (VI)
/// </summary>
public class UpdateSharedExpenseRequestValidator : AbstractValidator<UpdateSharedExpenseRequestDto>
{
    public UpdateSharedExpenseRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Expense title is required")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1,000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .WithMessage("Total amount must be greater than 0")
            .LessThanOrEqualTo(1000000000)
            .WithMessage("Total amount cannot exceed 1,000,000,000");
        RuleFor(x => x.Category)
            .MaximumLength(100)
            .WithMessage("Category cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.GroupName)
            .MaximumLength(200)
            .WithMessage("Group name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.GroupName));

        RuleFor(x => x.ReceiptImageUrl)
            .MaximumLength(500)
            .WithMessage("Receipt image URL cannot exceed 500 characters")
            .Must(BeValidUrl)
            .WithMessage("Receipt image URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.ReceiptImageUrl));

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}