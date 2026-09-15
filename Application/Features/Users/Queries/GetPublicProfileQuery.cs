using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Extensions;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Users.Queries;

public record GetPublicProfileQuery(Guid UserId) : IRequest<Result<PublicProfileDTO>>;

public class GetPublicProfileQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetPublicProfileQuery, Result<PublicProfileDTO>>
{
    public async Task<Result<PublicProfileDTO>> Handle(GetPublicProfileQuery request, CancellationToken ct)
    {
        var account = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.Id == request.UserId && !a.IsDeleted)
            .Select(a => new
            {
                a.Id,
                a.FullName,
                a.ImagePath,
                a.RoleId,
                a.TrustScore,
                a.CreatedAt,
                CityName = a.City != null ? a.City.NameEn : null,
                DonorOrgName = a.Donor != null ? a.Donor.OrganizationName : null,
                CharityOrgName = a.Charity != null ? a.Charity.OrganizationName : null
            })
            .FirstOrDefaultAsync(ct);

        if (account == null)
        {
            return Result<PublicProfileDTO>.FailResult(null!, AppMessageCodes.USER_NOT_FOUND, ErrorType.NotFound);
        }

        // Reviews metrics
        var reviews = await dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.RevieweeId == request.UserId && !r.IsDeleted)
            .Select(r => new
            {
                IsPositive = r.ReviewReason != null && r.ReviewReason.IsPositive
            })
            .ToListAsync(ct);

        var totalReviews = reviews.Count;
        float averageRating = 5.0f;
        if (totalReviews > 0)
        {
            var positiveCount = reviews.Count(r => r.IsPositive);
            averageRating = (float)Math.Round((double)positiveCount / totalReviews * 5.0, 1);
        }

        // Total Activity metrics based on role
        int totalActivities = 0;
        string? orgName = account.RoleId == UserRole.Donor ? account.DonorOrgName :
                          account.RoleId == UserRole.Charity ? account.CharityOrgName : null;

        switch (account.RoleId)
        {
            case UserRole.Donor:
                totalActivities = await dbContext.Donations
                    .AsNoTracking()
                    .CountAsync(d => d.DonorId == account.Id && !d.IsDeleted, ct);
                break;

            case UserRole.Volunteer:
                totalActivities = await dbContext.DeliveryMissions
                    .AsNoTracking()
                    .CountAsync(m => m.VolunteerId == account.Id && m.MissionStatus == MissionStatus.Delivered, ct);
                break;

            case UserRole.Charity:
                totalActivities = await dbContext.DonationRequests
                    .AsNoTracking()
                    .CountAsync(r => r.CharityId == account.Id && r.RequestStatus == RequestStatus.ApprovedByDonor, ct);
                break;

            default:
                totalActivities = 0;
                break;
        }

        var profile = new PublicProfileDTO
        {
            Id = account.Id,
            DisplayName = !string.IsNullOrWhiteSpace(orgName) ? orgName : account.FullName,
            AvatarUrl = account.ImagePath.ToCloudinaryUrl(),
            AverageRating = averageRating,
            TotalReviews = totalReviews,
            TotalActivities = totalActivities,
            JoinDate = account.CreatedAt,
            Role = account.RoleId.ToString(),
            TrustScore = account.TrustScore,
            City = account.CityName,
            OrganizationName = orgName
        };

        return Result<PublicProfileDTO>.SuccessResult(profile, AppMessageCodes.PUBLIC_PROFILE_RETRIEVED);
    }
}
