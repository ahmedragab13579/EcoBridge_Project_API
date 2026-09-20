using EcoBridge.Application.Features.Dashboard.Queries;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Dashboard.Validations;

public class GetCharityDashboardQueryValidator : AbstractValidator<GetCharityDashboardQuery>
{
    public GetCharityDashboardQueryValidator()
    {
        RuleFor(x => x.CharityId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.CHARITY_ID_REQUIRED);
    }
}