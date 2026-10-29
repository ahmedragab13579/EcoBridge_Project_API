using EcoBridge.API.Filters;
using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Features.Reviews.Commands;
using EcoBridge.Application.Features.Reviews.Queries;
using EcoBridge.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/review")]
[ApiController]
[Authorize]
[EnableRateLimiting("GeneralTokenBucket")]
public class ReviewsController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetReviewById(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetReviewByIdQuery(id), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("{id:int}/reasons")]
    public async Task<IActionResult> GetReviewReasons(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetReviewReasonsByReviewIdQuery(id), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("mission/{missionId:guid}")]
    public async Task<IActionResult> GetReviewsByMissionId(Guid missionId, [FromQuery] int page, [FromQuery] int pagesize, CancellationToken ct)
    {
        var result = await _sender.Send(new GetReviewsByMissionIdQuery(missionId,page,pagesize), ct);
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

    [HttpPost]
    [ValidateCsrfToken]
    public async Task<IActionResult> CreateReview(
        [FromBody] CreateReviewDTO dto,
        CancellationToken ct)
    {
        var result = await _sender.Send(new CreateReviewCommand(CurrentUserId, dto), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPatch("{id:int}")]
    [ValidateCsrfToken]
    public async Task<IActionResult> UpdateReview(
        int id,
        [FromBody] UpdateReviewDTO dto,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new UpdateReviewCommand(id, CurrentUserId, dto), ct);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpDelete("{id:int}")]
    [ValidateCsrfToken]
    public async Task<IActionResult> DeleteReview(int id, CancellationToken ct)
    {
        var result = await _sender.Send(
            new DeleteReviewCommand(id, CurrentUserId), ct);

        return ApiOk(result.Data!, result.Code);
    }
}