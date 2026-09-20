using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Events;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Reviews.Commands;

public class CreateReviewCommandHandler(
    IEcoBridgeDbContext dbContext,
    IPublisher publisher
) : IRequestHandler<CreateReviewCommand, ApiResult<ReviewResponseDTO>>
{
    public async Task<ApiResult<ReviewResponseDTO>> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // 1. Fetch mission with participants
        var mission = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Include(m => m.Donation)
            .FirstOrDefaultAsync(m => m.Id == dto.MissionId, cancellationToken);

        if (mission == null)
            throw new DomainException.NotFoundException(AppMessageCodes.MISSION_NOT_FOUND);

        var donorId = mission.Donation?.DonorId ?? Guid.Empty;
        var charityId = mission.Donation?.CharityId ?? Guid.Empty;
        var volunteerId = mission?.VolunteerId ?? Guid.Empty;

        var participantIds = new HashSet<Guid> { donorId, charityId, volunteerId };
        participantIds.Remove(Guid.Empty);

        // 2. Validate reviewer is a participant
        if (!participantIds.Contains(request.ReviewerId))
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        // 3. Validate reviewee is a participant and not the reviewer
        if (dto.RevieweeId == request.ReviewerId || !participantIds.Contains(dto.RevieweeId))
            throw new DomainException.ValidationException(AppMessageCodes.REVIEW_INVALID_MISSION);

        // 4. Prevent duplicate reviews
        var alreadyReviewed = await dbContext.Reviews.AnyAsync(r =>
            r.MissionId == dto.MissionId &&
            r.ReviewerId == request.ReviewerId &&
            r.RevieweeId == dto.RevieweeId,
            cancellationToken);

        if (alreadyReviewed)
            throw new DomainException.ConflictException(AppMessageCodes.REVIEW_ALREADY_EXISTS);

        // 5. Fetch review reason
        var reason = await dbContext.ReviewReasons
            .FirstOrDefaultAsync(r => r.Id == dto.ReviewReasonId, cancellationToken);

        if (reason == null)
            throw new DomainException.ValidationException(AppMessageCodes.VALIDATION_ERROR);

        // 6. Create Review entity using static factory
        var review = Review.Create(
            request.ReviewerId,
            dto.RevieweeId,
            dto.ReviewReasonId,
            dto.MissionId,
            dto.Comment?.Trim()
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
            MissionId = review.MissionId,
            ReviewReasonId = review.ReviewReasonId,
            ReviewReasonCode = reason.Code,
            IsPositive = reason.IsPositive,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        return ApiResult<ReviewResponseDTO>.Ok(response, AppMessageCodes.REVIEW_CREATED);
    }
}