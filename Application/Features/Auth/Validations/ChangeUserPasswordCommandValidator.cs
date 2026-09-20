using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Auth.Validations;

public class ChangeUserPasswordCommandValidator : AbstractValidator<ChangeUserPasswordCommand>
{
    public ChangeUserPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.USER_ID_REQUIRED);

        RuleFor(x => x.Request)
            .NotNull()
            .WithErrorCode(AppMessageCodes.REQUEST_BODY_REQUIERD);

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.CurrentPassword)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.CURRENT_PASSWORD_REQUIERD);

            RuleFor(x => x.Request.NewPassword)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.NEW_PASSWORD_REQUIERD)
                .MinimumLength(8)
                .WithErrorCode(AppMessageCodes.NEW_PASSWORD_TOO_SHORT)
                .MaximumLength(128)
                .WithErrorCode(AppMessageCodes.NEW_PASSWORD_TOO_LONG)
                .NotEqual(x => x.Request.CurrentPassword)
                .WithErrorCode(AppMessageCodes.NEW_PASSWORD_SAME_AS_CURRENT);
        });
    }
}