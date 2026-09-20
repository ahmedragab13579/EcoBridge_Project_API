using EcoBridge.Application.Features.Donation.Queries;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Donation.Validation;

public class GetDonationByIdQueryValidator : AbstractValidator<GetDonationByIdQuery>
{
    public GetDonationByIdQueryValidator()
    {
        RuleFor(x => x.DonationId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);
    }
}