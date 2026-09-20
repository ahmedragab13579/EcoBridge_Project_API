using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Donation.Validation;

public class UpdateDonationStatusCommandValidator : AbstractValidator<UpdateDonationStatusCommand>
{
    public UpdateDonationStatusCommandValidator()
    {
        RuleFor(x => x.DonationId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.VolunteerId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.Dto)
            .NotNull()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        When(x => x.Dto != null, () =>
        {
            RuleFor(x => x.Dto.Status)
                .IsInEnum()
                .WithErrorCode(AppMessageCodes.INVALID_INPUT);

            // Only transitions the handler supports are allowed at the input level too.
            RuleFor(x => x.Dto.Status)
                .Must(s => s == DonationStatus.InTransit
                        || s == DonationStatus.Delivered
                        || s == DonationStatus.Cancelled)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT);
        });
    }
}