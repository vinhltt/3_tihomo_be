using FluentValidation;
using MoneyManagement.Application.DTOs.SharedExpenseParticipant;

namespace MoneyManagement.Application.Validators;

/// <summary>
///     Validator for CreateSharedExpenseParticipantRequestDto (EN)<br />
///     Validator cho CreateSharedExpenseParticipantRequestDto (VI)
/// </summary>
public class
    CreateSharedExpenseParticipantRequestValidator : AbstractValidator<CreateSharedExpenseParticipantRequestDto>
{
    public CreateSharedExpenseParticipantRequestValidator()
    {
        RuleFor(x => x.ShareAmount)
            .GreaterThan(0)
            .WithMessage("Share amount must be greater than 0")
            .LessThanOrEqualTo(1000000000)
            .WithMessage("Share amount cannot exceed 1,000,000,000");

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(200)
            .WithMessage("Email cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.ParticipantName)
            .MaximumLength(200)
            .WithMessage("Participant name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.ParticipantName));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Invalid phone number format (use international format)")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x)
            .Must(x => x.UserId.HasValue || !string.IsNullOrWhiteSpace(x.ParticipantName))
            .WithMessage("Either UserId or ParticipantName must be provided");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}