using EcoBridge.Application.Features.Donation.Queries;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Donation.Validation;

public class GetDonationsQueryValidator : AbstractValidator<GetDonationsByStatusQuery>
{
    public GetDonationsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.CityId)
            .GreaterThan(0)
            .WithErrorCode(AppMessageCodes.INVALID_INPUT)
            .When(x => x.CityId.HasValue);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithErrorCode(AppMessageCodes.INVALID_INPUT)
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.Status)
            .Must(BeAValidStatus)
            .WithErrorCode(AppMessageCodes.INVALID_INPUT)
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
    }

    private static bool BeAValidStatus(string? status)
        => Enum.TryParse<DonationStatus>(status, ignoreCase: true, out _);
}