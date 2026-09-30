using EcoBridge.Application.Contracts;
using EcoBridge.Application.Features.Events;
using EcoBridge.Application.Features.Reviews.Commands;
using EcoBridge.Application.Hubs;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Reviews.Handlers;

public class DeleteReviewCommandHandler(
    IEcoBridgeDbContext dbContext,
    IOutboxService outboxService,
    IHubContext<MissionHub> hubContext
) : IRequestHandler<DeleteReviewCommand, ApiResult<bool>>
{
    private const int TrustCreditForDeletedNegativeReview = 10;
    private const string ReasonCodeReviewDeleted = "REVIEW_DELETED";

    public async Task<ApiResult<bool>> Handle(
        DeleteReviewCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load review with its reason
        var review = await dbContext.Reviews
            .Include(r => r.ReviewReason)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
            throw new DomainException.NotFoundException(AppMessageCodes.REVIEW_NOT_FOUND);

        // 2. Only the reviewer may delete
        if (review.ReviewerId != request.ReviewerId)
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        // 3. Capture context before soft-delete
        var revieweeId = review.RevieweeId;
        var missionId = review.MissionId;
        var wasNegative = review.ReviewReason != null && !review.ReviewReason.IsPositive;

        // 4. Soft delete
        review.SoftDelete();

        // 5. If the review was negative, enqueue durable trust credit restoration event
        if (wasNegative)
        {
            outboxService.Enqueue(new TrustScoreIncreasedEvent(
                revieweeId,
                TrustCreditForDeletedNegativeReview,
                ReasonCodeReviewDeleted,
                missionId
            ));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // 6. SignalR — notify the reviewee so their UI refreshes
        await hubContext.Clients.Group($"User_{revieweeId}")
            .SendAsync("ReviewDeleted", new
            {
                reviewId = review.Id,
                missionId = review.MissionId
            }, cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.REVIEW_DELETED);
    }
}