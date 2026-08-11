using FluentValidation;
using MoneyManagement.Application.DTOs.Jar;

namespace MoneyManagement.Application.Validators;

/// <summary>
///     Validator for UpdateJarRequest (EN)<br />
///     Validator cho UpdateJarRequest (VI)
/// </summary>
public class UpdateJarRequestValidator : AbstractValidator<UpdateJarRequest>
{
    public UpdateJarRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Jar name is required")
            .MaximumLength(200)
            .WithMessage("Jar name cannot exceed 200 characters");

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0)
            .WithMessage("Target amount must be greater than 0")
            .LessThanOrEqualTo(1000000000)
            .WithMessage("Target amount cannot exceed 1,000,000,000")
            .When(x => x.TargetAmount.HasValue);
        RuleFor(x => x.AllocationPercentage)
            .GreaterThan(0)
            .WithMessage("Allocation percentage must be greater than 0")
            .LessThanOrEqualTo(1)
            .WithMessage("Allocation percentage must be between 0 and 1 (100%)")
            .When(x => x.AllocationPercentage.HasValue);
    }
}