using EcoBridge.Application.Features.Gamification.Queries;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("GeneralTokenBucket")]
public class GamificationController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpGet("my-points")]
    [Authorize]
    public async Task<IActionResult> GetMyPoints([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        if (userId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new GetMyPointsQuery(userId, page, pageSize), ct);
        return HandleResult(result);
    }

    [HttpGet("leaderboard")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLeaderboard([FromQuery] string? role, [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(role))
            return BadRequest(new { code = "INVALID_ROLE", message = "Query parameter 'role' is required and must be a valid role (e.g., Volunteer)." });

        var result = await _sender.Send(new GetLeaderboardQuery(role, limit), ct);
        return HandleResult(result);
    }
}
