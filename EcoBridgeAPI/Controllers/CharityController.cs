using EcoBridge.Application.Features.Donation.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoBridge.API.Controllers;

[Route("api/charity")]
[ApiController]
[Authorize(Roles = "Charity")]
public class CharityController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpGet("my-accepted-donations")]
    public async Task<IActionResult> GetMyAcceptedDonations([FromQuery] int page, [FromQuery] int pagesize )
    {
        var result = await _sender.Send(
            new GetMyDonationsByStatusQuery(CurrentUserId,page,pagesize),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }
}