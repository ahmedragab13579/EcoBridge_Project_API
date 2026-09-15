using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.Features.Donation.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoBridge.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DonationRequestsController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpPost("{donationId:guid}/request")]
    [Authorize(Roles = "Charity")]
    public async Task<IActionResult> RequestDonation(Guid donationId, CancellationToken ct)
    {
        var charityId = CurrentUserId;
        if (charityId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new RequestDonationCommand(donationId, charityId), ct);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> ApproveDonationRequest(Guid id, CancellationToken ct)
    {
        var donorId = CurrentUserId;
        if (donorId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new ApproveDonationRequestCommand(id, donorId), ct);
        return HandleResult(result);
    }
}
