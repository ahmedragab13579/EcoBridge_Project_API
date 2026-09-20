using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Donation.Validation;

public class DeleteDonationCommandValidator : AbstractValidator<DeleteDonationCommand>
{
    public DeleteDonationCommandValidator()
    {
        RuleFor(x => x.DonationId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);
    }
}