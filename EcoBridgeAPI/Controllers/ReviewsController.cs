using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Reviews.Commands;
using EcoBridge.Application.Features.Reviews.Queriess;
using EcoBridge.Domain.Enums;
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
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReview(int id, CancellationToken ct)
    {
        var result = await _sender.Send(
            new DeleteReviewCommand(id, CurrentUserId), ct);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateReview(
        int id,
        [FromBody] UpdateReviewDTO dto,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new UpdateReviewCommand(id, CurrentUserId, dto), ct);

        return ApiOk(result.Data!, result.Code);
    }
    [HttpPost]
    public async Task<IActionResult> CreateReview(
        [FromBody] CreateReviewDTO dto,
        CancellationToken ct)
    {
        var result = await _sender.Send(new CreateReviewCommand(CurrentUserId, dto), ct);
        return ApiOk(result.Data!, result.Code);
    }


    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserReviews(
        Guid userId,
        [FromQuery] ReviewRole role = ReviewRole.Reviewee,   
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(
            new GetReviewsByUserIdQuery(userId, role, page, pageSize), ct);
        return ApiOk(result.Data!, result.Code);
    }
}