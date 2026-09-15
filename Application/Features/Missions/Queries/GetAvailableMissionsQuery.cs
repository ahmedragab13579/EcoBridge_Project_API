using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;

namespace EcoBridge.Application.Features.Missions.Queries;

public record GetAvailableMissionsQuery(
    Guid VolunteerAccountId,
    int? VolunteerCityId = null
) : IRequest<Result<List<MissionDTO>>>;
