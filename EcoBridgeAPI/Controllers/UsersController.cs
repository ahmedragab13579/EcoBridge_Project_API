using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.Features.Auth.Queries;
using EcoBridge.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/user")]
[ApiController]
[Authorize]
[EnableRateLimiting("GeneralTokenBucket")]
public class UsersController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;
    [HttpGet("{id:guid}/public-profile")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicProfile(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetPublicProfileQuery(id), ct);
        return ApiOk(result.Data!, result.Code);
    }
}