using FluentValidation;
using MoneyManagement.Application.DTOs.SharedExpense;
using MoneyManagement.Application.DTOs.SharedExpenseParticipant;

namespace MoneyManagement.Application.Validators;

/// <summary>
///     Validator for CreateSharedExpenseRequestDto (EN)<br />
///     Validator cho CreateSharedExpenseRequestDto (VI)
/// </summary>
public class CreateSharedExpenseRequestValidator : AbstractValidator<CreateSharedExpenseRequestDto>
{
    public CreateSharedExpenseRequestValidator()
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

        RuleFor(x => x.Participants)
            .NotEmpty()
            .WithMessage("At least one participant is required")
            .Must(HaveUniqueParticipants)
            .WithMessage("Participants must be unique");
        RuleForEach(x => x.Participants)
            .NotNull()
            .WithMessage("Participant cannot be null")
            .ChildRules(participant =>
            {
                participant.RuleFor(p => p.ShareAmount)
                    .GreaterThan(0)
                    .WithMessage("Share amount must be greater than 0");

                participant.RuleFor(p => p.Email)
                    .EmailAddress()
                    .WithMessage("Invalid email format")
                    .When(p => !string.IsNullOrEmpty(p.Email));

                participant.RuleFor(p => p.ParticipantName)
                    .MaximumLength(200)
                    .WithMessage("Participant name cannot exceed 200 characters")
                    .When(p => !string.IsNullOrEmpty(p.ParticipantName));

                participant.RuleFor(p => p.PhoneNumber)
                    .Matches(@"^\+?[1-9]\d{1,14}$")
                    .WithMessage("Invalid phone number format")
                    .When(p => !string.IsNullOrEmpty(p.PhoneNumber));

                participant.RuleFor(p => p)
                    .Must(p => p.UserId.HasValue || !string.IsNullOrWhiteSpace(p.ParticipantName))
                    .WithMessage("Either UserId or ParticipantName must be provided");
            });

        RuleFor(x => x)
            .Must(HaveMatchingTotalAmount)
            .WithMessage("Sum of participant shares must equal total amount");
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    private static bool HaveUniqueParticipants(ICollection<CreateSharedExpenseParticipantRequestDto> participants)
    {
        // Check for duplicate user IDs among registered users
        var userIds = participants.Where(p => p.UserId.HasValue).Select(p => p.UserId!.Value);
        return userIds.Distinct().Count() == userIds.Count();
    }

    private static bool HaveMatchingTotalAmount(CreateSharedExpenseRequestDto request)
    {
        var totalShares = request.Participants?.Sum(p => p.ShareAmount) ?? 0;
        return Math.Abs(totalShares - request.TotalAmount) < 0.01m; // Allow for minor rounding differences
    }
}