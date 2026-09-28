using EcoBridge.Application.Features.Donation.Queries;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Donation.Validation;

public class GetMyAcceptedDonationsQueryValidator : AbstractValidator<GetMyDonationsByStatusQuery>
{
    public GetMyAcceptedDonationsQueryValidator()
    {
        RuleFor(x => x.CharityId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);
    }
}