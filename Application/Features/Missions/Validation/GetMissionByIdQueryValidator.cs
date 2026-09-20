using EcoBridge.Application.Features.Missions.Queries;
using EcoBridge.Domain.Errors;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoBridge.Application.Features.Missions.Validation
{
    public class GetMissionByIdQueryValidator : AbstractValidator<GetMissionByIdQuery>
    {
        public GetMissionByIdQueryValidator()
        {
            RuleFor(x => x.MissionId)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.INVALID_INPUT);
        }
    }
}
