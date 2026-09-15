using EcoBridge.Domain.Errors;
using EcoBridge.Application.Features.Donation.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoBridge.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Charity")]
public class CharityController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpGet("my-accepted-donations")]
    public async Task<IActionResult> GetMyAcceptedDonations()
    {
        var charityId = CurrentUserId;
        if (charityId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new GetMyAcceptedDonationsQuery(charityId), HttpContext.RequestAborted);
        return HandleResult(result);
    }
}