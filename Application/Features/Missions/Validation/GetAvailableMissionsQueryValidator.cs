using EcoBridge.Application.Features.Missions.Queries;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Missions.Validation;

public class GetAvailableMissionsQueryValidator : AbstractValidator<GetAvailableMissionsQuery>
{
    public GetAvailableMissionsQueryValidator()
    {
        RuleFor(x => x.VolunteerCityId)
            .GreaterThan(0)
            .WithErrorCode(AppMessageCodes.INVALID_INPUT)
            .When(x => x.VolunteerCityId.HasValue);

        RuleFor(x => x.VolunteerAccountId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT)
            .When(x => x.VolunteerAccountId != Guid.Empty);
    }
}