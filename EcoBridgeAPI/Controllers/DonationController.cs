using EcoBridge.Domain.Errors;
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
        var accountId = CurrentUserId;
        if (accountId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new CreateDonationCommand(accountId, dto), HttpContext.RequestAborted);
        return HandleResult(result);
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
        var result = await _sender.Send(new GetDonationsQuery(cityId, status, categoryId, targetPage, pageSize), HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpGet("my-donations")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> GetMyDonations()
    {
        var accountId = CurrentUserId;
        if (accountId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new GetMyDonationsQuery(accountId), HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetDonationById(Guid id)
    {
        var result = await _sender.Send(new GetDonationByIdQuery(id), HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> UpdateDonation(Guid id, [FromForm] UpdateDonationDTO dto)
    {
        var accountId = CurrentUserId;
        if (accountId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new UpdateDonationCommand(id, accountId, dto), HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> DeleteDonation(Guid id)
    {
        var accountId = CurrentUserId;
        if (accountId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new DeleteDonationCommand(id, accountId), HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "Volunteer,Admin")]
    public async Task<IActionResult> AssignSelfOrTarget(Guid id, [FromBody] AssignVolunteerDTO? dto = null)
    {
        var callerId = CurrentUserId;
        if (callerId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        bool isAdmin = User.IsInRole("Admin");
        Guid? targetVolunteerId = dto?.VolunteerId != Guid.Empty ? dto?.VolunteerId : null;

        var result = await _sender.Send(new AssignVolunteerCommand(id, callerId, targetVolunteerId, isAdmin), HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Volunteer")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateStatusDTO dto)
    {
        var volunteerId = CurrentUserId;
        if (volunteerId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new UpdateDonationStatusCommand(id, volunteerId, dto), HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpGet("open")]
    [Authorize(Roles = "Charity,Admin")]
    public async Task<IActionResult> GetPendingDonations()
    {
        var result = await _sender.Send(new GetPendingDonationsQuery(), HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Charity")]
    public async Task<IActionResult> AcceptDonation(Guid id)
    {
        var charityId = CurrentUserId;
        if (charityId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new AcceptDonationCommand(id, charityId), HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Roles = "Donor,Charity,Admin")]
    public async Task<IActionResult> CancelDonation(Guid id, [FromBody] CancelDonationDTO dto)
    {
        var accountId = CurrentUserId;
        if (accountId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new CancelDonationCommand(id, accountId, dto.Reason), HttpContext.RequestAborted);
        return HandleResult(result);
    }
}
