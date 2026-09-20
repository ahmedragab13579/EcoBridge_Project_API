using EcoBridge.Application.Features.Missions.Queries;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Missions.Validation;

public class GetMyMissionsQueryValidator : AbstractValidator<GetMyMissionsQuery>
{
    public GetMyMissionsQueryValidator()
    {
        RuleFor(x => x.VolunteerAccountId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);
    }
}