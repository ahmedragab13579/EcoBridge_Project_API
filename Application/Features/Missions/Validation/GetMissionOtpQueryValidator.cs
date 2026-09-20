using EcoBridge.Application.Features.Missions.Queries;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Missions.Validation;

public class GetMissionOtpQueryValidator : AbstractValidator<GetMissionOtpQuery>
{
    public GetMissionOtpQueryValidator()
    {
        RuleFor(x => x.MissionId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.CharityAccountId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);
    }
}