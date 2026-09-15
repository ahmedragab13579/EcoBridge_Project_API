using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Extensions;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Gamification.Queries;

public record GetLeaderboardQuery(string? Role = "Volunteer", int Limit = 20) : IRequest<Result<List<LeaderboardUserDTO>>>;

public class GetLeaderboardQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetLeaderboardQuery, Result<List<LeaderboardUserDTO>>>
{
    public async Task<Result<List<LeaderboardUserDTO>>> Handle(GetLeaderboardQuery request, CancellationToken ct)
    {
        var targetRole = UserRole.Volunteer;
        if (!string.IsNullOrWhiteSpace(request.Role) && Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
        {
            targetRole = parsedRole;
        }

        var limit = request.Limit is < 1 or > 100 ? 20 : request.Limit;

        var users = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.RoleId == targetRole && !a.IsDeleted && a.IsActive)
            .OrderByDescending(a => a.TotalWalletPoints)
            .ThenByDescending(a => a.TrustScore)
            .Take(limit)
            .Select(a => new
            {
                a.Id,
                a.FullName,
                a.ImagePath,
                RoleId = a.RoleId.ToString(),
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

        return Result<List<LeaderboardUserDTO>>.SuccessResult(leaderboard, AppMessageCodes.LEADERBOARD_RETRIEVED);
    }
}
