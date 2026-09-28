using EcoBridge.Application.Features.Gamification.Queries;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Gamification.Validations;

public class GetMyPointsQueryValidator : AbstractValidator<GetMyPointsQuery>
{
    public GetMyPointsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);
    }
}