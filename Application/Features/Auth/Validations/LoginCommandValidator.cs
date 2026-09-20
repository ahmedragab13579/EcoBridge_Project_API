using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Auth.Validations;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithErrorCode(AppMessageCodes.REQUEST_BODY_REQUIERD);

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.Email)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.EMAIL_REQUIERD)
                .EmailAddress()
                .WithErrorCode(AppMessageCodes.EMAIL_INVALID_FORMAT);

            RuleFor(x => x.Request.Password)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.PASSWORD_REQUIERD);
        });
    }
}