using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Missions.Queries;

public record GetMissionOtpQuery(
    Guid MissionId,
    Guid CharityAccountId
) : IRequest<Result<string>>;
