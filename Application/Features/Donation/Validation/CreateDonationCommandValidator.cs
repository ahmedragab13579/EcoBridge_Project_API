using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Donation.Validation;
public class CreateDonationCommandValidator : AbstractValidator<CreateDonationCommand>
{
    public CreateDonationCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.Dto)
            .NotNull()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        When(x => x.Dto != null, () =>
        {
            RuleFor(x => x.Dto.Quantity)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .MaximumLength(100)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT);

            RuleFor(x => x.Dto.PickupLocation)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .MaximumLength(500)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT);

            RuleFor(x => x.Dto.CategoryId)
                .GreaterThan(0)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT);

            RuleFor(x => x.Dto.ExpiryDate)
                .GreaterThan(DateTime.UtcNow)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .When(x => String.IsNullOrEmpty( x.Dto.ExpiryDate.ToString()??""));

            RuleFor(x => x.Dto.Weight)
                .GreaterThan(0)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .When(x => x.Dto.Weight.HasValue);

            RuleFor(x => x.Dto.Title)
                .MaximumLength(200)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .When(x => !string.IsNullOrWhiteSpace(x.Dto.Title));

            RuleFor(x => x.Dto.Description)
                .MaximumLength(2000)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .When(x => !string.IsNullOrWhiteSpace(x.Dto.Description));

            RuleFor(x => x.Dto.CityId)
                .GreaterThan(0)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .When(x => x.Dto.CityId !=0);

            // Only validate the image if photo service will actually use it.
            RuleFor(x => x.Dto.Image)
                .Must(f => f == null || f.Length <= 5 * 1024 * 1024)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .When(x => x.Dto.Image != null);
        });
    }
}