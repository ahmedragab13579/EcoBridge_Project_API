using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Extensions;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Gamification.Queries;

public record GetLeaderboardQuery(int page, int pageSize,string? Role = "Volunteer")
    : IRequest<ApiResult<PagedResult<LeaderboardUserDTO>>>;

public class GetLeaderboardQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetLeaderboardQuery, ApiResult<PagedResult<LeaderboardUserDTO>>>
{
    public async Task<ApiResult<PagedResult<LeaderboardUserDTO>>> Handle(
        GetLeaderboardQuery request,
        CancellationToken ct)
    {
        // 1. Resolve target role (default to Volunteer)
        var targetRole = UserRole.Volunteer;

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var parsedRole)
                || !Enum.IsDefined(parsedRole))
            {
                throw new DomainException.ValidationException(AppMessageCodes.INVALID_ROLE);
            }

            targetRole = parsedRole;
        }


        // 3. Query top users
        var users = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.Role == targetRole && !a.IsDeleted && a.IsActive)
            .OrderByDescending(a => a.TotalWalletPoints)
            .ThenByDescending(a => a.TrustScore).Skip((request.page-1) * request.pageSize)
            .Take(request.pageSize)
            .Select(a => new
            {
                a.Id,
                a.FullName,
                a.ImagePath,
                RoleId = a.Role.ToString(),
                TotalPoints = a.TotalWalletPoints,
                a.TrustScore,
                CityName = a.City != null ? a.City.NameEn : null
            })
            .ToListAsync(ct);

        var leaderboard = users.Select((user, index) => new LeaderboardUserDTO
        {
            Rank = index + 1,
            UserId = user.Id,
            FullName = user.FullName,
            AvatarUrl = user.ImagePath.ToCloudinaryUrl(),
            Role = user.RoleId,
            TotalPoints = user.TotalPoints,
            TrustScore = user.TrustScore,
            City = user.CityName
        }).ToList();

        return ApiResult<PagedResult<LeaderboardUserDTO>>.Ok(
            new PagedResult<LeaderboardUserDTO>
            {
              Items = leaderboard,
              Page = 1,
              PageNumber = 1,
              PageSize = 1,

            },
            AppMessageCodes.LEADERBOARD_RETRIEVED);
    }
}