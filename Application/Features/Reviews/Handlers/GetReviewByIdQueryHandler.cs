using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Reviews.Queries;

public class GetReviewByIdQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetReviewByIdQuery, ApiResult<ReviewResponseDTO>>
{
    public async Task<ApiResult<ReviewResponseDTO>> Handle(
        GetReviewByIdQuery request,
        CancellationToken cancellationToken)
    {
        var review = await dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.Id == request.ReviewId && !r.IsDeleted)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (review == null)
            throw new DomainException.NotFoundException(AppMessageCodes.REVIEW_NOT_FOUND);

        return ApiResult<ReviewResponseDTO>.Ok(review, AppMessageCodes.REVIEW_RETRIEVED);
    }
}