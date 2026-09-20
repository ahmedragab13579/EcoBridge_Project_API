using EcoBridge.Application.Features.Admin.Queries;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Admin.Validations;

public class GetAdminDonationsQueryValidator : AbstractValidator<GetAdminDonationsQuery>
{
    public GetAdminDonationsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode(AppMessageCodes.PAGE_INVALID);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode(AppMessageCodes.PAGE_SIZE_INVALID);
    }
}