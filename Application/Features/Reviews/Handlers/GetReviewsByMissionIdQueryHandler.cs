using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Reviews.Queries;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Reviews.Handlers;

public class GetReviewsByMissionIdQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetReviewsByMissionIdQuery, ApiResult<PagedResult<ReviewResponseDTO>>>
{
    public async Task<ApiResult<PagedResult<ReviewResponseDTO>>> Handle(
        GetReviewsByMissionIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.MissionId == Guid.Empty)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_MISSION_ID);

        // 1. Verify mission exists (and is not soft-deleted)
        var missionExists = await dbContext.DeliveryMissions
            .AsNoTracking()
            .AnyAsync(m => m.Id == request.MissionId && !m.IsDeleted, cancellationToken);

        if (!missionExists)
            throw new DomainException.NotFoundException(AppMessageCodes.MISSION_NOT_FOUND);

        // 2. Fetch non-deleted reviews for the mission
        var reviews = await dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.MissionId == request.MissionId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt).Skip((request.page - 1) * request.pageSize).Take(request.pageSize)

            .Select(r => new ReviewResponseDTO
            {
                Id = r.Id,
                ReviewerId = r.ReviewerId,
                RevieweeId = r.RevieweeId,
                MissionId = r.MissionId,
                ReviewReasonId = r.ReviewReasonId,
                ReviewReasonCode = r.ReviewReason != null ? r.ReviewReason.Code : string.Empty,
                IsPositive = r.ReviewReason != null && r.ReviewReason.IsPositive,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ApiResult<PagedResult<ReviewResponseDTO>>.Ok(
            new PagedResult<ReviewResponseDTO>
            {
                Items = reviews,
                Page = request.page,
                PageSize = request.pageSize,
                PageNumber = request.page,
                TotalCount = reviews.Count,
            },
            AppMessageCodes.REVIEWS_RETRIEVED);
    }
}