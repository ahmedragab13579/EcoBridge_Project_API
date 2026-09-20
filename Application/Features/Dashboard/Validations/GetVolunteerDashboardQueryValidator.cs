using EcoBridge.Application.Features.Dashboard.Queries;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Dashboard.Validations;

public class GetVolunteerDashboardQueryValidator : AbstractValidator<GetVolunteerDashboardQuery>
{
    public GetVolunteerDashboardQueryValidator()
    {
        RuleFor(x => x.VolunteerId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.VOLUNTEER_ID_REQUIRED);
    }
}