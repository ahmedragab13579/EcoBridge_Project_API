using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Auth.Validations;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithErrorCode(AppMessageCodes.REQUEST_BODY_REQUIERD);

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.FullName)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.FULL_NAME_REQUIERD)
                .MaximumLength(200)
                .WithErrorCode(AppMessageCodes.FULL_NAME_TOO_LONG);

            RuleFor(x => x.Request.Email)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.EMAIL_REQUIERD)
                .EmailAddress()
                .WithErrorCode(AppMessageCodes.EMAIL_INVALID_FORMAT)
                .MaximumLength(256)
                .WithErrorCode(AppMessageCodes.EMAIL_TOO_LONG);

            RuleFor(x => x.Request.Phone)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.PHONE_REQUIERD)
                .MaximumLength(20)
                .WithErrorCode(AppMessageCodes.PHONE_TOO_LONG);

            RuleFor(x => x.Request.Password)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.PASSWORD_REQUIERD)
                .MinimumLength(8)
                .WithErrorCode(AppMessageCodes.PASSWORD_TOO_SHORT)
                .MaximumLength(128)
                .WithErrorCode(AppMessageCodes.PASSWORD_TOO_LONG);

            RuleFor(x => x.Request.Role)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.ROLE_REQUIERD)
                .Must(BeAValidRole)
                .WithErrorCode(AppMessageCodes.ROLE_INVALID);

            RuleFor(x => x.Request.CityId)
                .GreaterThan(0)
                .WithErrorCode(AppMessageCodes.CITY_ID_INVALID)
                .When(x => x.Request.CityId.HasValue);

            RuleFor(x => x.Request.AddressDetails)
                .MaximumLength(500)
                .WithErrorCode(AppMessageCodes.ADDRESS_DETAILS_TOO_LONG)
                .When(x => !string.IsNullOrWhiteSpace(x.Request.AddressDetails));
        });
    }

    private static bool BeAValidRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return false;

        // Admin registration is not allowed
        if (string.Equals(role, nameof(UserRole.Admin), StringComparison.OrdinalIgnoreCase))
            return false;

        return Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsed)
               && Enum.IsDefined(parsed);
    }
}