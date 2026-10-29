using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using MediatR;

namespace EcoBridge.Application.Features.Missions.Queries;

public record GetMyMissionsQuery(
    Guid VolunteerAccountId,
    int page,
    int pageSize
) : IRequest<ApiResult<PagedResult<MissionDTO>>>;
