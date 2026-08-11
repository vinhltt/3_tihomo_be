using FluentValidation;
using MoneyManagement.Application.DTOs.Jar;

namespace MoneyManagement.Application.Validators;

/// <summary>
///     Validator for AllocateIncomeRequest (EN)<br />
///     Validator cho AllocateIncomeRequest (VI)
/// </summary>
public class AllocateIncomeRequestValidator : AbstractValidator<AllocateIncomeRequest>
{
    public AllocateIncomeRequestValidator()
    {
        RuleFor(x => x.IncomeAmount)
            .GreaterThan(0)
            .WithMessage("Income amount must be greater than 0")
            .LessThanOrEqualTo(1000000000)
            .WithMessage("Income amount cannot exceed 1,000,000,000");

        RuleFor(x => x.CustomRatios)
            .Must(ratios => ratios == null || ratios.Values.All(v => v >= 0 && v <= 1))
            .WithMessage("All custom ratios must be between 0 and 1")
            .Must(ratios => ratios == null || ratios.Values.Sum() <= 1)
            .WithMessage("Sum of custom ratios cannot exceed 1");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.SelectedJarIds)
            .Must(jarIds => jarIds == null || jarIds.All(id => id != Guid.Empty))
            .WithMessage("All selected jar IDs must be valid");
    }
}