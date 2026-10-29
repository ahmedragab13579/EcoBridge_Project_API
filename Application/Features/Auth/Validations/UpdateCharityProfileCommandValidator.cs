using EcoBridge.Application.Features.Auth.Commands.Profiles;
using EcoBridge.Domain.Errors;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoBridge.Application.Features.Auth.Validations
{
    public class UpdateCharityProfileCommandValidator : AbstractValidator<UpdateCharityProfileCommand>
    {
        public UpdateCharityProfileCommandValidator()
        {
            RuleFor(x => x.UserId)
           .NotEmpty()
           .WithErrorCode(AppMessageCodes.USER_ID_REQUIRED);

            RuleFor(x => x.updateCharityProfileDTO)
                .NotNull()
                .WithErrorCode(AppMessageCodes.REQUEST_BODY_REQUIERD);

            When(x => x.updateCharityProfileDTO != null, () =>
            {
                RuleFor(x => x.updateCharityProfileDTO.FullName)
                    .NotEmpty()
                    .WithErrorCode(AppMessageCodes.FULL_NAME_REQUIERD)
                    .MaximumLength(200)
                    .WithErrorCode(AppMessageCodes.FULL_NAME_TOO_LONG);

                RuleFor(x => x.updateCharityProfileDTO.Phone)
                    .NotEmpty()
                    .WithErrorCode(AppMessageCodes.PHONE_REQUIERD)
                    .MaximumLength(20)
                    .WithErrorCode(AppMessageCodes.PHONE_TOO_LONG);

                RuleFor(x => x.updateCharityProfileDTO.AddressDetails)
                    .MaximumLength(500)
                    .WithErrorCode(AppMessageCodes.ADDRESS_DETAILS_TOO_LONG)
                    .When(x => !string.IsNullOrWhiteSpace(x.updateCharityProfileDTO.AddressDetails));

                RuleFor(x => x.updateCharityProfileDTO.Address)
                    .MaximumLength(500)
                    .WithErrorCode(AppMessageCodes.ADDRESS_TOO_LONG)
                    .When(x => !string.IsNullOrWhiteSpace(x.updateCharityProfileDTO.Address));

                RuleFor(x => x.updateCharityProfileDTO.OrganizationName)
                    .MaximumLength(100)
                    .WithErrorCode(AppMessageCodes.CHARITY_NAME_TOO_LONG)
                    .When(x => !string.IsNullOrWhiteSpace(x.updateCharityProfileDTO.OrganizationName));

                RuleFor(x => x.updateCharityProfileDTO.RegistrationNumber)
                    .MaximumLength(500)
                    .WithErrorCode(AppMessageCodes.REGSTRATION_NUMBER_TOO_LONG)
                    .When(x => !string.IsNullOrWhiteSpace(x.updateCharityProfileDTO.RegistrationNumber));

                RuleFor(x => x.updateCharityProfileDTO.ProfileImage)
                    .Must(f => f == null || f.Length <= 5 * 1024 * 1024)
                    .WithErrorCode(AppMessageCodes.PROFILE_IMAGE_TOO_LARGE)
                    .When(x => x.updateCharityProfileDTO.ProfileImage != null);
            });
        }
    }
}
