using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
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
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewDTO dto, CancellationToken ct)
    {
        var reviewerId = CurrentUserId;
        if (reviewerId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new CreateReviewCommand(reviewerId, dto), ct);
        return HandleResult(result);
    }
}
