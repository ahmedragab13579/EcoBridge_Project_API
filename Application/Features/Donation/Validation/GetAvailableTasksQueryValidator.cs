using EcoBridge.Application.Features.Donation.Queries;
using FluentValidation;

namespace EcoBridge.Application.Features.Donation.Validation;

public class GetAvailableTasksQueryValidator : AbstractValidator<GetAvailableTasksQuery>
{
    public GetAvailableTasksQueryValidator()
    {
      
        // Validator exists for consistency and to allow future filters
    }
}