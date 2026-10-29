using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Features.Reviews.Commands;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Reviews.Handlers;

public class UpdateReviewCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<UpdateReviewCommand, ApiResult<ReviewResponseDTO>>
{
    // Polarity bands
    private const int PositiveMinRating = 3;  
    private const int NegativeMaxRating = 2;  

    public async Task<ApiResult<ReviewResponseDTO>> Handle(
        UpdateReviewCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load review with reason
        var review = await dbContext.Reviews
            .Include(r => r.ReviewReason)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
            throw new DomainException.NotFoundException(AppMessageCodes.REVIEW_NOT_FOUND);

        // 2. Only the reviewer may update
        if (review.ReviewerId != request.ReviewerId)
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        // 3. Rating polarity guard (if rating provided)
        if (request.Dto.Rating.HasValue)
        {
            var isPositiveReason = review.ReviewReason?.IsPositive ?? false;

            var ratingInBand = isPositiveReason
                ? request.Dto.Rating.Value >= PositiveMinRating
                : request.Dto.Rating.Value <= NegativeMaxRating;

            if (!ratingInBand)
                throw new DomainException.ValidationException(
                    AppMessageCodes.REVIEW_RATING_POLARITY_MISMATCH);
        }

        // 4. Apply updates via domain methods
        if (request.Dto.Comment != null)
            review.UpdateComment(request.Dto.Comment);

        if (request.Dto.Rating.HasValue)
            review.UpdateRating(request.Dto.Rating.Value);

        await dbContext.SaveChangesAsync(cancellationToken);

        // 5. Build response
        var response = new ReviewResponseDTO
        {
            Id = review.Id,
            ReviewerId = review.ReviewerId,
            RevieweeId = review.RevieweeId,
            MissionId = review.MissionId,
            ReviewReasonId = review.ReviewReasonId,
            ReviewReasonCode = review.ReviewReason?.Code ?? string.Empty,
            IsPositive = review.ReviewReason?.IsPositive ?? false,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        return ApiResult<ReviewResponseDTO>.Ok(response, AppMessageCodes.REVIEW_UPDATED);
    }
}