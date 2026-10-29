using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Missions.Queries;

public record GetMissionByIdQuery(
    Guid MissionId
) : IRequest<ApiResult<MissionDTO>>;
