using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Auth.Validations;

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        // Either RevokeAll must be true (with a valid UserId), or a RefreshToken must be provided.
        RuleFor(x => x)
            .Must(HaveSomethingToRevoke)
            .WithErrorCode(AppMessageCodes.REVOKE_TARGET_REQUIERD);

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.USER_ID_REQUIRED)
            .When(x => x.RevokeAll);
    }

    private static bool HaveSomethingToRevoke(RevokeTokenCommand command)
    {
        if (command.RevokeAll)
            return command.UserId.HasValue && command.UserId.Value != Guid.Empty;

        return !string.IsNullOrWhiteSpace(command.RefreshToken);
    }
}