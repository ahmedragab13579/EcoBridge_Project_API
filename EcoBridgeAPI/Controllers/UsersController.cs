using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("GeneralTokenBucket")]
public class UsersController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserProfile(CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new GetUserProfileQuery(userId), ct);
        return HandleResult(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUserProfile([FromForm] UpdateProfileDTO request, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new UpdateUserProfileCommand(userId, request), ct);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}/public-profile")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicProfile(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new EcoBridge.Application.Features.Users.Queries.GetPublicProfileQuery(id), ct);
        return HandleResult(result);
    }
}
