using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Missions.Commands;

public record ConfirmPickupCommand(
    Guid MissionId,
    Guid DonorAccountId
) : IRequest<Result<bool>>;
