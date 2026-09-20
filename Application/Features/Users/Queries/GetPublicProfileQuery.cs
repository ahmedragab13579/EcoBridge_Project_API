using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Extensions;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Users.Queries;

public record GetPublicProfileQuery(Guid UserId) : IRequest<ApiResult<PublicProfileDTO>>;

public class GetPublicProfileQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetPublicProfileQuery, ApiResult<PublicProfileDTO>>
{
    public async Task<ApiResult<PublicProfileDTO>> Handle(GetPublicProfileQuery request, CancellationToken ct)
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
                OrgName = a.OrganizationName
            })
            .FirstOrDefaultAsync(ct);

        if (account == null)
        {
            return ApiResult<PublicProfileDTO>.Fail(AppMessageCodes.USER_NOT_FOUND);
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
        string? orgName = account.OrgName;

       
                totalActivities = await dbContext.Donations
                    .AsNoTracking()
                    .CountAsync(d => d.DonorId == account.Id && !d.IsDeleted, ct);
           

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

        return ApiResult<PublicProfileDTO>.Ok(profile, AppMessageCodes.PUBLIC_PROFILE_RETRIEVED);
    }
}
