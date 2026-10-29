using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.Users.Auth;
using EcoBridge.Application.DTO.Users.Auth.Users.Common;
using EcoBridge.Application.Extensions;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Users.Queries;

public record GetPublicProfileQuery(Guid UserId) : IRequest<ApiResult<PublicProfileDTO>>;

public class GetPublicProfileQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetPublicProfileQuery, ApiResult<PublicProfileDTO>>
{
    public async Task<ApiResult<PublicProfileDTO>> Handle(
        GetPublicProfileQuery request,
        CancellationToken ct)
    {
        if (request.UserId == Guid.Empty)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_INPUT);

        var account = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.Id == request.UserId && !a.IsDeleted)
            .Select(a => new
            {
                a.Id,
                a.FullName,
                a.ImagePath,
                a.Role,
                a.TrustScore,
                a.CreatedAt,
                CityName = a.City != null ? a.City.NameEn : null,
                a.OrganizationName
            })
            .FirstOrDefaultAsync(ct);

        if (account == null)
            throw new DomainException.NotFoundException(AppMessageCodes.USER_NOT_FOUND);

        // ── Reviews metrics ────────────────────────────────────
        // Pull only what we need: positivity (for impression) and rating (for average)
        var reviews = await dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.RevieweeId == request.UserId && !r.IsDeleted)
            .Select(r => new
            {
                IsPositive = r.ReviewReason != null && r.ReviewReason.IsPositive,
                Rating = r.Rating
            })
            .ToListAsync(ct);

        var totalReviews = reviews.Count;

        // Impression: fraction of positive reasons, mapped to 0–5.
        // Defaults to 0 for users with no reviews (not a perfect score).
        float averageImpression = 0f;
        if (totalReviews > 0)
        {
            var positiveCount = reviews.Count(r => r.IsPositive);
            averageImpression = (float)Math.Round((double)positiveCount / totalReviews * 5.0, 1);
        }

        // Rating: direct average of rated reviews (already on a 1–5 scale).
        // Null-rated legacy reviews are excluded so they don't drag the average down.
        var ratedReviews = reviews.ToList();
        float averageRating = 0f;
        if (ratedReviews.Count > 0)
        {
            var sum = ratedReviews.Sum(r => r.Rating);
            averageRating = (float)Math.Round((double)sum / ratedReviews.Count, 1);
        }

        // ── Activity metrics based on role ─────────────────────
        int totalActivities = account.Role switch
        {
            UserRole.Donor => await dbContext.Donations
                .AsNoTracking()
                .CountAsync(d => d.DonorId == account.Id && !d.IsDeleted, ct),

            UserRole.Volunteer => await dbContext.DeliveryMissions
                .AsNoTracking()
                .CountAsync(d => d.VolunteerId == account.Id
                              && d.Status == MissionStatus.Delivered
                              && !d.IsDeleted, ct),

            UserRole.Charity => await dbContext.DeliveryMissions
                .AsNoTracking()
                .CountAsync(d => d.Donation.CharityId == account.Id
                              && d.Status == MissionStatus.Delivered
                              && !d.IsDeleted, ct),

            _ => 0
        };

        var orgName = account.OrganizationName;

        var profile = new PublicProfileDTO
        {
            Id = account.Id,
            DisplayName = !string.IsNullOrWhiteSpace(orgName) ? orgName : account.FullName,
            AvatarUrl = account.ImagePath.ToCloudinaryUrl(),
            AverageRating = (int)averageRating,
            AverageImpression =(int) averageImpression,
            TotalReviews = totalReviews,
            TotalActivities = totalActivities,
            JoinDate = account.CreatedAt,
            Role = account.Role.ToString(),
            TrustScore = account.TrustScore,
            City = account.CityName,
            OrganizationName = orgName
        };

        return ApiResult<PublicProfileDTO>.Ok(profile, AppMessageCodes.PUBLIC_PROFILE_RETRIEVED);
    }
}