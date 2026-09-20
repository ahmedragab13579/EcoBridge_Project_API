using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Donation.Validation;

public class AssignVolunteerCommandValidator : AbstractValidator<AssignVolunteerCommand>
{
    public AssignVolunteerCommandValidator()
    {
        RuleFor(x => x.DonationId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.CallerId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);
        RuleFor(x => x.TargetVolunteerId)
            .NotNull()
            .When(x => x.IsAdmin)
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);
    }
}