using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Missions.Queries;

public record GetAvailableMissionsQuery(
    Guid VolunteerAccountId,
    int? VolunteerCityId = null
) : IRequest<ApiResult<List<MissionDTO>>>;
