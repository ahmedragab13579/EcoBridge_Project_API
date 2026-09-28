using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Donation.Commands;

public record UnAssignVolunteerCommand(
    Guid DonationId,
    Guid VolunteerId
) : IRequest<ApiResult<bool>>;