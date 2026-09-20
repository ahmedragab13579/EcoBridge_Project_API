using EcoBridge.Application.Features.Donation.Queries;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Donation.Validation;

public class GetMyDonationsQueryValidator : AbstractValidator<GetMyDonationsQuery>
{
    public GetMyDonationsQueryValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);
    }
}