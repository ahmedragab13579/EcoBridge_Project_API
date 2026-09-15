using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Models;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.Features.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Reviews.Commands;

public class CreateReviewCommandHandler(
    IEcoBridgeDbContext dbContext,
    IPublisher publisher
) : IRequestHandler<CreateReviewCommand, Result<ReviewResponseDTO>>
{
    public async Task<Result<ReviewResponseDTO>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // 1. Fetch mission with participants
        var mission = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Include(m => m.DonationRequest)
                .ThenInclude(r => r.Donation)
            .FirstOrDefaultAsync(m => m.Id == dto.MissionId, cancellationToken);

        if (mission == null)
            return Result<ReviewResponseDTO>.FailResult(new ReviewResponseDTO(), AppMessageCodes.MISSION_NOT_FOUND, ErrorType.NotFound);

        var donorId = mission.DonationRequest?.Donation?.DonorId ?? Guid.Empty;
        var charityId = mission.DonationRequest?.CharityId ?? Guid.Empty;
        var volunteerId = mission.VolunteerId ?? Guid.Empty;

        var participantIds = new HashSet<Guid> { donorId, charityId, volunteerId };
        participantIds.Remove(Guid.Empty); // remove unassigned

        // 2. Validate reviewer is a participant
        if (!participantIds.Contains(request.ReviewerId))
        {
            return Result<ReviewResponseDTO>.FailResult(new ReviewResponseDTO(), AppMessageCodes.FORBIDDEN, ErrorType.Forbidden);
        }

        // 3. Validate reviewee is a participant and not the reviewer
        if (dto.RevieweeId == request.ReviewerId || !participantIds.Contains(dto.RevieweeId))
        {
            return Result<ReviewResponseDTO>.FailResult(new ReviewResponseDTO(), AppMessageCodes.REVIEW_INVALID_MISSION, ErrorType.Validation);
        }

        // 4. Prevent duplicate reviews (same reviewer + reviewee + mission)
        var alreadyReviewed = await dbContext.Reviews.AnyAsync(r =>
            r.MissionId == dto.MissionId &&
            r.ReviewerId == request.ReviewerId &&
            r.RevieweeId == dto.RevieweeId,
            cancellationToken);

        if (alreadyReviewed)
        {
            return Result<ReviewResponseDTO>.FailResult(new ReviewResponseDTO(), AppMessageCodes.REVIEW_ALREADY_EXISTS, ErrorType.Conflict);
        }

        // 5. Fetch review reason
        var reason = await dbContext.ReviewReasons.FirstOrDefaultAsync(r => r.Id == dto.ReviewReasonId, cancellationToken);
        if (reason == null)
        {
            return Result<ReviewResponseDTO>.FailResult(new ReviewResponseDTO(), AppMessageCodes.VALIDATION_ERROR, ErrorType.Validation);
        }

        // 6. Create Review entity using static factory
        var review = Review.Create(
            request.ReviewerId,
            dto.RevieweeId,
            dto.ReviewReasonId,
            dto.Comment?.Trim(),
            dto.MissionId,
            null
        );

        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync(cancellationToken);

        // 7. Deduct TrustScore if negative review reason
        if (!reason.IsPositive)
        {
            await publisher.Publish(new TrustScoreDeductedEvent(
                dto.RevieweeId,
                10,
                reason.Code,
                dto.MissionId
            ), cancellationToken);
        }

        var response = new ReviewResponseDTO
        {
            Id = review.Id,
            ReviewerId = review.ReviewerId,
            RevieweeId = review.RevieweeId,
            MissionId = review.MissionId ?? dto.MissionId,
            ReviewReasonId = review.ReviewReasonId,
            ReviewReasonCode = reason.Code,
            IsPositive = reason.IsPositive,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        return Result<ReviewResponseDTO>.SuccessResult(response, AppMessageCodes.REVIEW_CREATED);
    }
}
