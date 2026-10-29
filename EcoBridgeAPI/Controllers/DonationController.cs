using EcoBridge.API.Filters;
using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Application.Features.Donation.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/donation")]
[ApiController]
[EnableRateLimiting("GeneralTokenBucket")]
public class DonationController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpPost("{id:guid}/unassign")]
    [Authorize(Roles = "Volunteer")]
    [ValidateCsrfToken]
    public async Task<IActionResult> UnassignVolunteer(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(
            new UnAssignVolunteerCommand(id, CurrentUserId), ct);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost("{id:guid}/unclaim")]
    [Authorize(Roles = "Charity")]
    [ValidateCsrfToken]
    public async Task<IActionResult> UnclaimDonation(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(
            new UnClaimCommand(id, CurrentUserId), ct);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost]
    [Authorize(Roles = "Donor")]
    [ValidateCsrfToken]
    public async Task<IActionResult> CreateDonation([FromForm] CreateDonationDTO dto)
    {
        var result = await _sender.Send(
            new CreateDonationCommand(CurrentUserId, dto),
            HttpContext.RequestAborted);

        return ApiCreated(
            result.Data!,
            actionName: nameof(GetDonationById),
            routeValues: new { id = result.Data!.Id },
            code: result.Code);
    }

    [HttpGet]
    [Authorize(Roles = "Charity,Admin,Donor,Volunteer")]
    public async Task<IActionResult> GetDonations(
        [FromQuery] int? cityId,
        [FromQuery] int? categoryId,
        [FromQuery] string? status,
        [FromQuery] int? pageNumber,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var targetPage = pageNumber ?? page;

        var result = await _sender.Send(
            new GetDonationsByStatusQuery(cityId, status, categoryId, targetPage, pageSize),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("available")]
    [Authorize(Roles = "Charity,Volunteer,Admin")]
    public async Task<IActionResult> GetAvailableDonations([FromQuery] int page, [FromQuery] int pagesize, CancellationToken ct)
    {
        var result = await _sender.Send(
            new GetAvailableDonationsQuery(page,pagesize), ct);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetDonationById(Guid id)
    {
        var result = await _sender.Send(
            new GetDonationByIdQuery(id),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("{id:guid}/finished-mission")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> GetFinishedMission(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(
            new GetFinishedMissionByDonationIdQuery(id, CurrentUserId), ct);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Donor")]
    [ValidateCsrfToken]
    public async Task<IActionResult> UpdateDonation(Guid id, [FromForm] UpdateDonationDTO dto)
    {
        var result = await _sender.Send(
            new UpdateDonationCommand(id, CurrentUserId, dto),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Donor")]
    [ValidateCsrfToken]
    public async Task<IActionResult> DeleteDonation(Guid id)
    {
        var result = await _sender.Send(
            new DeleteDonationCommand(id, CurrentUserId),
            HttpContext.RequestAborted);

        return ApiOkWithCode(result.Code);
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "Volunteer,Admin")]
    [ValidateCsrfToken]
    public async Task<IActionResult> AssignSelfOrTarget(
        Guid id,
        [FromBody] AssignVolunteerDTO? dto = null)
    {
        bool isAdmin = User.IsInRole("Admin");

        Guid? targetVolunteerId = dto?.VolunteerId != Guid.Empty
            ? dto?.VolunteerId
            : null;

        var result = await _sender.Send(
            new AssignVolunteerCommand(id, CurrentUserId, targetVolunteerId, isAdmin),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Charity")]
    [ValidateCsrfToken]
    public async Task<IActionResult> AcceptDonation(Guid id)
    {
        var result = await _sender.Send(
            new ClaimDonationCommand(id, CurrentUserId),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Roles = "Donor")]
    [ValidateCsrfToken]
    public async Task<IActionResult> CancelDonation(Guid id, [FromBody] CancelDonationDTO dto)
    {
        var result = await _sender.Send(
            new CancelDonationCommand(id, CurrentUserId, dto.Reason),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }
}