using EcoBridge.Application.Features.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/dashboard")]
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
        var result = await _sender.Send(new GetDonorDashboardQuery(CurrentUserId), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("volunteer")]
    [Authorize(Roles = "Volunteer,Admin")]
    public async Task<IActionResult> GetVolunteerDashboard(CancellationToken ct)
    {
        var result = await _sender.Send(new GetVolunteerDashboardQuery(CurrentUserId), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("charity")]
    [Authorize(Roles = "Charity,Admin")]
    public async Task<IActionResult> GetCharityDashboard(CancellationToken ct)
    {
        var result = await _sender.Send(new GetCharityDashboardQuery(CurrentUserId), ct);
        return ApiOk(result.Data!, result.Code);
    }
}