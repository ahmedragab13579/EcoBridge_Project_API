using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Application.Features.Donation.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoBridge.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DonationController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpPost]
    [Authorize(Roles = "Donor")]
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
            new GetDonationsQuery(cityId, status, categoryId, targetPage, pageSize),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("my-donations")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> GetMyDonations()
    {
        var result = await _sender.Send(
            new GetMyDonationsQuery(CurrentUserId),
            HttpContext.RequestAborted);

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

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> UpdateDonation(Guid id, [FromForm] UpdateDonationDTO dto)
    {
        var result = await _sender.Send(
            new UpdateDonationCommand(id, CurrentUserId, dto),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> DeleteDonation(Guid id)
    {
        var result = await _sender.Send(
            new DeleteDonationCommand(id, CurrentUserId),
            HttpContext.RequestAborted);

        return ApiOkWithCode(result.Code);
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "Volunteer,Admin")]
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

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Volunteer")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateStatusDTO dto)
    {
        var result = await _sender.Send(
            new UpdateDonationStatusCommand(id, CurrentUserId, dto),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("open")]
    [Authorize(Roles = "Charity,Admin")]
    public async Task<IActionResult> GetPendingDonations()
    {
        var result = await _sender.Send(
            new GetPendingDonationsQuery(),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Charity")]
    public async Task<IActionResult> AcceptDonation(Guid id)
    {
        var result = await _sender.Send(
            new ClaimDonationCommand(id, CurrentUserId),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Roles = "Donor,Charity,Admin")]
    public async Task<IActionResult> CancelDonation(Guid id, [FromBody] CancelDonationDTO dto)
    {
        var result = await _sender.Send(
            new CancelDonationCommand(id, CurrentUserId, dto.Reason),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }
}