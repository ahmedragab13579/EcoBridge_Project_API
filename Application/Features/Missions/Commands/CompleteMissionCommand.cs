using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Missions.Commands;

public record CompleteMissionCommand(
    Guid MissionId,
    Guid VolunteerAccountId,
    string OtpCode
) : IRequest<Result<bool>>;
