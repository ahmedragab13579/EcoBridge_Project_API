using EcoBridge.API.Filters;
using EcoBridge.Application.Features.Admin.Commands;
using EcoBridge.Application.Features.Admin.Queries;
using EcoBridge.Domain.Enums;
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
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] UserRole? role,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAdminUsersQuery(role), cancellationToken);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPatch("users/{id:guid}/toggle-status")]
    [Authorize(Roles = "Admin")]
    [ValidateCsrfToken]
    public async Task<IActionResult> ToggleUserStatus(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ToggleUserStatusCommand(id), cancellationToken);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("donations")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllDonations([FromQuery]int page,[FromQuery] int pagesize, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAdminDonationsQuery(page,pagesize), cancellationToken);
        return ApiOk(result.Data!, result.Code);
    }
}