using EcoBridge.Domain.Enums;
using EcoBridge.Application.Features.Admin.Commands;
using EcoBridge.Application.Features.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoBridge.API.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAdminStatsQuery(), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers([FromQuery] UserRole? role, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAdminUsersQuery(role), cancellationToken);
        return HandleResult(result);
    }

    [HttpPatch("users/{id:guid}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleUserStatus(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ToggleUserStatusCommand(id), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("donations")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllDonations(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAdminDonationsQuery(), cancellationToken);
        return HandleResult(result);
    }
}
