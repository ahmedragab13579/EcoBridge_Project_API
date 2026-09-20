using EcoBridge.Application.Features.Donation.Queries;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Donation.Validation;

public class GetVolunteerTasksQueryValidator : AbstractValidator<GetVolunteerTasksQuery>
{
    public GetVolunteerTasksQueryValidator()
    {
        RuleFor(x => x.VolunteerId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);
    }
}