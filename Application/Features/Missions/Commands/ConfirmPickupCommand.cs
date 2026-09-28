using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Missions.Commands;

public record ConfirmPickupCommand(
    Guid MissionId,
    Guid VolunteerId
) : IRequest<ApiResult<bool>>;