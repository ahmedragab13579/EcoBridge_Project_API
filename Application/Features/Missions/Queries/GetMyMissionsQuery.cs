using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Missions.Queries;

public record GetMyMissionsQuery(
    Guid VolunteerAccountId
) : IRequest<ApiResult<List<MissionDTO>>>;
