using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Reviews.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("GeneralTokenBucket")]
public class ReviewsController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpPost]
    public async Task<IActionResult> CreateReview(
        [FromBody] CreateReviewDTO dto,
        CancellationToken ct)
    {
        var result = await _sender.Send(new CreateReviewCommand(CurrentUserId, dto), ct);
        return ApiOk(result.Data!, result.Code);
    }
}