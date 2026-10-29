using EcoBridge.Application.Features.Auth.Commands.Profiles;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Auth.Validations;

public class UpdateVolunteerProfileCommandValidator : AbstractValidator<UpdateVolunteerProfileCommand>
{
    public UpdateVolunteerProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.USER_ID_REQUIRED);

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

            RuleFor(x => x.Request.Phone)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.PHONE_REQUIERD)
                .MaximumLength(20)
                .WithErrorCode(AppMessageCodes.PHONE_TOO_LONG);

            RuleFor(x => x.Request.AddressDetails)
                .MaximumLength(500)
                .WithErrorCode(AppMessageCodes.ADDRESS_DETAILS_TOO_LONG)
                .When(x => !string.IsNullOrWhiteSpace(x.Request.AddressDetails));

            RuleFor(x => x.Request.Address)
                .MaximumLength(500)
                .WithErrorCode(AppMessageCodes.ADDRESS_TOO_LONG)
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Address));

            RuleFor(x => x.Request.VehicleType)
                .MaximumLength(100)
                .WithErrorCode(AppMessageCodes.VEHICLE_TYPE_TOO_LONG)
                .When(x => !string.IsNullOrWhiteSpace(x.Request.VehicleType));

            RuleFor(x => x.Request.VehicleDetails)
                .MaximumLength(500)
                .WithErrorCode(AppMessageCodes.VEHICLE_DETAILS_TOO_LONG)
                .When(x => !string.IsNullOrWhiteSpace(x.Request.VehicleDetails));
            RuleFor(x => x.Request.ProfileImage)
                .Must(f => f == null || f.Length <= 5 * 1024 * 1024)
                .WithErrorCode(AppMessageCodes.PROFILE_IMAGE_TOO_LARGE)
                .When(x => x.Request.ProfileImage != null);
        });
    }
}