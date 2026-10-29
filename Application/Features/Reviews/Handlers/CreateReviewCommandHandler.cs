using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Features.Events;
using EcoBridge.Application.Features.Reviews.Commands;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Reviews.Handlers;

public class CreateReviewCommandHandler(
    IEcoBridgeDbContext dbContext,
    IOutboxService outboxService
) : IRequestHandler<CreateReviewCommand, ApiResult<ReviewResponseDTO>>
{
    public async Task<ApiResult<ReviewResponseDTO>> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // 1. Fetch mission with participants and check existence
        var mission = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Include(m => m.Donation)
            .FirstOrDefaultAsync(m => m.Id == dto.MissionId, cancellationToken);

        if (mission == null)
            throw new DomainException.NotFoundException(AppMessageCodes.MISSION_NOT_FOUND);

        // 2. Ensure mission and donation are completed
        if (mission.Status != MissionStatus.Delivered || mission.Donation == null || mission.Donation.Status != DonationStatus.Completed)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_NOT_COMPLETED);

        // 3. Extract and validate participants
        var donorId = mission.Donation.DonorId;
        var charityId = mission.Donation.CharityId ?? Guid.Empty;
        var volunteerId = mission.VolunteerId;

        var participantIds = new HashSet<Guid> { donorId, charityId, volunteerId };
        participantIds.Remove(Guid.Empty);

        if (!participantIds.Contains(request.ReviewerId))
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

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
            dto.Rating,
            dto.ReviewReasonId,
            dto.MissionId,
            dto.Comment?.Trim()
        );

        dbContext.Reviews.Add(review);

        // 7. Enqueue TrustScore penalty event to durable outbox if negative review
        if (!reason.IsPositive)
        {
            outboxService.Enqueue(new TrustScoreDeductedEvent(
                dto.RevieweeId,
                10,
                reason.Code,
                dto.MissionId
            ));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

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