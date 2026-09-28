using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Missions.Commands;
using EcoBridge.Application.Features.Missions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoBridge.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MissionsController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;


    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Volunteer")]
    public async Task<IActionResult> CompleteMission(
    Guid id,
    [FromBody] CompleteMissionRequestDTO dto,
    CancellationToken ct)
    {
        var result = await _sender.Send(
            new CompleteMissionCommand(id, CurrentUserId, dto.OtpCode), ct);

        return ApiOk(result.Data!, result.Code);
    }



    [HttpGet("my")]
    [Authorize(Roles = "Volunteer")]
    public async Task<IActionResult> GetMyMissions(CancellationToken ct)
    {
        var result = await _sender.Send(new GetMyMissionsQuery(CurrentUserId), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMissionById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetMissionByIdQuery(id), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("{id:guid}/otp")]
    [Authorize(Roles = "Charity")]
    public async Task<IActionResult> GetMissionOtp(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(
            new GetMissionOtpQuery(id, CurrentUserId), ct);

        return ApiOk(result.Data!, result.Code);
    }
}