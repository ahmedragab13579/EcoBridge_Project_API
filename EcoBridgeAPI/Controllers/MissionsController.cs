using EcoBridge.Domain.Errors;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Missions.Commands;
using EcoBridge.Application.Features.Missions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MissionsController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpGet("available")]
    [Authorize(Roles = "Volunteer,Admin")]
    public async Task<IActionResult> GetAvailableMissions(CancellationToken ct)
    {
        var volunteerId = CurrentUserId;
        int? volunteerCityId = null;

        var cityClaim = User.FindFirst("cityId")?.Value;
        if (int.TryParse(cityClaim, out var cId))
        {
            volunteerCityId = cId;
        }

        var result = await _sender.Send(new GetAvailableMissionsQuery(volunteerId, volunteerCityId), ct);
        return HandleResult(result);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Volunteer")]
    public async Task<IActionResult> GetMyMissions(CancellationToken ct)
    {
        var volunteerId = CurrentUserId;
        if (volunteerId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new GetMyMissionsQuery(volunteerId), ct);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMissionById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetMissionByIdQuery(id), ct);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Volunteer")]
    public async Task<IActionResult> AcceptMission(Guid id, CancellationToken ct)
    {
        var volunteerId = CurrentUserId;
        if (volunteerId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new AcceptMissionCommand(id, volunteerId), ct);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/confirm-pickup")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> ConfirmPickup(Guid id, CancellationToken ct)
    {
        var donorId = CurrentUserId;
        if (donorId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new ConfirmPickupCommand(id, donorId), ct);
        return HandleResult(result, () => ApiOk(new { isSuccess = true, status = "InTransit", message = result.Message }));
    }

    [HttpPost("{id:guid}/deliver")]
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Volunteer")]
    [EnableRateLimiting("SecurityFixedWindow")]
    public async Task<IActionResult> DeliverMission(Guid id, [FromBody] CompleteMissionRequestDTO request, CancellationToken ct)
    {
        var volunteerId = CurrentUserId;
        if (volunteerId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new CompleteMissionCommand(id, volunteerId, request.OtpCode), ct);
        return HandleResult(result, () => ApiOk(new { isSuccess = true, pointsEarned = 50, status = "Delivered", message = result.Message }));
    }

    [HttpGet("{id:guid}/otp")]
    [Authorize(Roles = "Charity")]
    public async Task<IActionResult> GetMissionOtp(Guid id, CancellationToken ct)
    {
        var charityId = CurrentUserId;
        if (charityId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new GetMissionOtpQuery(id, charityId), ct);
        return HandleResult(result, () => ApiOk(new { otpCode = result.Value }));
    }
}
