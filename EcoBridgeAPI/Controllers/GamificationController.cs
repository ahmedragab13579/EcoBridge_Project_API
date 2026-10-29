using EcoBridge.Application.Features.Gamification.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/gamification")]
[ApiController]
[EnableRateLimiting("GeneralTokenBucket")]
public class GamificationController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpGet("my-points")]
    [Authorize]
    public async Task<IActionResult> GetMyPoints(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(
            new GetMyPointsQuery(CurrentUserId, page, pageSize), ct);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("leaderboard")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLeaderboard(
        [FromQuery] string? role,
        [FromQuery] int page = 20,
        [FromQuery] int pagesize = 20,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetLeaderboardQuery(page,pagesize,role), ct);
        return ApiOk(result.Data!, result.Code);
    }
}