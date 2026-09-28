using EcoBridge.Application.Result;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace EcoBridge.Application.Features.Missions.Commands;

public record CompleteMissionCommand(
     Guid MissionId,
    Guid VolunteerId,
    string Otp
) : IRequest<ApiResult<CompleteMissionResponseDTO>>;

public class CompleteMissionResponseDTO
{
    public Guid MissionId { get; set; }
    public int PointsAwarded { get; set; }
}