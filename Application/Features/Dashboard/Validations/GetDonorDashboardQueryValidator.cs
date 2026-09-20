using EcoBridge.Application.Features.Dashboard.Queries;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Dashboard.Validations;

public class GetDonorDashboardQueryValidator : AbstractValidator<GetDonorDashboardQuery>
{
    public GetDonorDashboardQueryValidator()
    {
        RuleFor(x => x.DonorId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.DONOR_ID_REQUIRED);
    }
}