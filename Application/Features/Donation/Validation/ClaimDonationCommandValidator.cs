using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Donation.Validation;

public class ClaimDonationCommandValidator : AbstractValidator<ClaimDonationCommand>
{
    public ClaimDonationCommandValidator()
    {
        RuleFor(x => x.DonationId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.CharityId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);
    }
}