using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Donation.Validation;

public class UpdateDonationCommandValidator : AbstractValidator<UpdateDonationCommand>
{
    public UpdateDonationCommandValidator()
    {
        RuleFor(x => x.DonationId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.Dto)
            .NotNull()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        When(x => x.Dto != null, () =>
        {
            RuleFor(x => x.Dto.FoodType)
                .MaximumLength(100)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .When(x => !string.IsNullOrWhiteSpace(x.Dto.FoodType));

            RuleFor(x => x.Dto.Quantity)
                .MaximumLength(100)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .When(x => !string.IsNullOrWhiteSpace(x.Dto.Quantity));

            RuleFor(x => x.Dto.PickupLocation)
                .MaximumLength(500)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .When(x => !string.IsNullOrWhiteSpace(x.Dto.PickupLocation));

            RuleFor(x => x.Dto.ExpiryDate)
                .GreaterThan(DateTime.UtcNow)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .When(x => string.IsNullOrEmpty( x.Dto.ExpiryDate.ToString()));

            RuleFor(x => x.Dto.CityId)
                .GreaterThan(0)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .When(x => x.Dto.CityId!=0);

            RuleFor(x => x.Dto.Image)
                .Must(f => f == null || f.Length <= 5 * 1024 * 1024)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .When(x => x.Dto.Image != null);
        });
    }
}