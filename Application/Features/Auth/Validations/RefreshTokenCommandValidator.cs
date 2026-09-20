using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Auth.Validations;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithErrorCode(AppMessageCodes.REQUEST_BODY_REQUIERD);

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.RefreshToken)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.REFRESH_TOKEN_REQUIERD);

            RuleFor(x => x.Request.AccessToken)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.ACCESS_TOKEN_REQUIERD);
        });
    }
}