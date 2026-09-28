using EcoBridge.Application.Features.Gamification.Queries;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Gamification.Validations;

public class GetLeaderboardQueryValidator : AbstractValidator<GetLeaderboardQuery>
{
    public GetLeaderboardQueryValidator()
    {
        RuleFor(x => x.Role)
            .Must(BeAValidRole)
            .WithErrorCode(AppMessageCodes.INVALID_ROLE)
            .When(x => !string.IsNullOrWhiteSpace(x.Role));

        RuleFor(x => x.pageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode(AppMessageCodes.PAGE_SIZE_INVALID);
        RuleFor(x => x.page)
            .InclusiveBetween(1, 100)
            .WithErrorCode(AppMessageCodes.PAGE_INVALID);
    }

    private static bool BeAValidRole(string? role)
        => Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsed)
           && Enum.IsDefined(parsed);
}