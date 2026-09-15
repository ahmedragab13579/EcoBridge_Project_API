using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.DTO;
using MediatR;

namespace EcoBridge.Application.Features.Missions.Commands;

public record AcceptMissionCommand(
    Guid MissionId,
    Guid VolunteerAccountId
) : IRequest<Result<AcceptMissionResponseDTO>>;
