using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;

namespace EcoBridge.Application.Features.Missions.Queries;

public record GetMyMissionsQuery(
    Guid VolunteerAccountId
) : IRequest<Result<List<MissionDTO>>>;
