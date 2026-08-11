using FluentValidation;
using MoneyManagement.Application.DTOs.Budget;

namespace MoneyManagement.Application.Validators;

/// <summary>
///     Validator for UpdateBudgetRequest (EN)<br />
///     Validator cho UpdateBudgetRequest (VI)
/// </summary>
public class UpdateBudgetRequestValidator : AbstractValidator<UpdateBudgetRequest>
{
    public UpdateBudgetRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Budget ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Budget name is required")
            .MaximumLength(200)
            .WithMessage("Budget name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Category)
            .MaximumLength(100)
            .WithMessage("Category cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.BudgetAmount)
            .GreaterThan(0)
            .WithMessage("Budget amount must be greater than 0");

        RuleFor(x => x.Period)
            .IsInEnum()
            .WithMessage("Invalid budget period");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid budget status");

        RuleFor(x => x.AlertThreshold)
            .InclusiveBetween(0, 100)
            .WithMessage("Alert threshold must be between 0 and 100")
            .When(x => x.AlertThreshold.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}