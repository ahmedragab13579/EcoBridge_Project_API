using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Donation.Validation;

public class CancelDonationCommandValidator : AbstractValidator<CancelDonationCommand>
{
    public CancelDonationCommandValidator()
    {
        RuleFor(x => x.DonationId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT)
            .MaximumLength(500)
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);
    }
}