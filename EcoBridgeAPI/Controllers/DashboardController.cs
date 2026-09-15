using EcoBridge.Application.Features.Dashboard.Queries;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("GeneralTokenBucket")]
public class DashboardController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpGet("donor")]
    [Authorize(Roles = "Donor,Admin")]
    public async Task<IActionResult> GetDonorDashboard(CancellationToken ct)
    {
        var donorId = CurrentUserId;
        if (donorId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new GetDonorDashboardQuery(donorId), ct);
        return HandleResult(result);
    }

    [HttpGet("volunteer")]
    [Authorize(Roles = "Volunteer,Admin")]
    public async Task<IActionResult> GetVolunteerDashboard(CancellationToken ct)
    {
        var volunteerId = CurrentUserId;
        if (volunteerId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new GetVolunteerDashboardQuery(volunteerId), ct);
        return HandleResult(result);
    }

    [HttpGet("charity")]
    [Authorize(Roles = "Charity,Admin")]
    public async Task<IActionResult> GetCharityDashboard(CancellationToken ct)
    {
        var charityId = CurrentUserId;
        if (charityId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new GetCharityDashboardQuery(charityId), ct);
        return HandleResult(result);
    }
}
