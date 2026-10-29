using EcoBridge.Application.Features.Auth.Commands.Profiles;
using EcoBridge.Domain.Errors;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoBridge.Application.Features.Auth.Validations
{
    public class UpdateDonorAdminProfileCommandValidator : AbstractValidator<UpdateDonorAdminProfileCommand>
    {
        public UpdateDonorAdminProfileCommandValidator()
        {
            RuleFor(x => x.id)
           .NotEmpty()
           .WithErrorCode(AppMessageCodes.USER_ID_REQUIRED);

            RuleFor(x => x.request)
                .NotNull()
                .WithErrorCode(AppMessageCodes.REQUEST_BODY_REQUIERD);

            When(x => x.request != null, () =>
            {
                RuleFor(x => x.request.FullName)
                    .NotEmpty()
                    .WithErrorCode(AppMessageCodes.FULL_NAME_REQUIERD)
                    .MaximumLength(200)
                    .WithErrorCode(AppMessageCodes.FULL_NAME_TOO_LONG);

                RuleFor(x => x.request.Phone)
                    .NotEmpty()
                    .WithErrorCode(AppMessageCodes.PHONE_REQUIERD)
                    .MaximumLength(20)
                    .WithErrorCode(AppMessageCodes.PHONE_TOO_LONG);

                RuleFor(x => x.request.AddressDetails)
                    .MaximumLength(500)
                    .WithErrorCode(AppMessageCodes.ADDRESS_DETAILS_TOO_LONG)
                    .When(x => !string.IsNullOrWhiteSpace(x.request.AddressDetails));

                RuleFor(x => x.request.Address)
                    .MaximumLength(500)
                    .WithErrorCode(AppMessageCodes.ADDRESS_TOO_LONG)
                    .When(x => !string.IsNullOrWhiteSpace(x.request.Address));

    
                RuleFor(x => x.request.ProfileImage)
                    .Must(f => f == null || f.Length <= 5 * 1024 * 1024)
                    .WithErrorCode(AppMessageCodes.PROFILE_IMAGE_TOO_LARGE)
                    .When(x => x.request.ProfileImage != null);
            });
        }
    }
}
