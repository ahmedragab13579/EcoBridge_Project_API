using EcoBridge.Domain.Errors;
using FluentValidation;
using global::EcoBridge.Application.Features.Donation.Commands;

namespace EcoBridge.Application.Features.Donation.Validation;

public class RequestDonationCommandValidator : AbstractValidator<RequestDonationCommand>
{
    public RequestDonationCommandValidator()
    {
        RuleFor(x => x.DonationId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.CharityAccountId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);
    }
}