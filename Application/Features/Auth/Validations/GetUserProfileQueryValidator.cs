using EcoBridge.Application.Features.Auth.Queries;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Auth.Validations;

public class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
{
    public GetUserProfileQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.USER_ID_REQUIRED);
    }
}