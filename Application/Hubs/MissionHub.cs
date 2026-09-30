using System.Security.Claims;
using EcoBridge.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Hubs;

[Authorize]
public class MissionHub(IEcoBridgeDbContext dbContext) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdStr, out var userId) && userId != Guid.Empty)
        {
            // 1. Add to personal user group for direct notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

            // 2. Auto-join city, governorate, and country groups if volunteer has a registered city
            var account = await dbContext.Accounts
                .Include(a => a.City)
                    .ThenInclude(c => c!.Governorate)
                        .ThenInclude(g => g.Country)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == userId);

            if (account?.Role == EcoBridge.Domain.Enums.UserRole.Volunteer && account.City != null)
            {
                if (!string.IsNullOrWhiteSpace(account.City.NameEn))
                {
                    var normalizedCity = account.City.NameEn.Trim().ToLowerInvariant();
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_City_{normalizedCity}");
                }

                if (account.City.Governorate != null && !string.IsNullOrWhiteSpace(account.City.Governorate.NameEn))
                {
                    var normalizedGov = account.City.Governorate.NameEn.Trim().ToLowerInvariant();
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_Governorate_{normalizedGov}");
                }

                if (account.City.Governorate?.Country != null && !string.IsNullOrWhiteSpace(account.City.Governorate.Country.NameEn))
                {
                    var normalizedCountry = account.City.Governorate.Country.NameEn.Trim().ToLowerInvariant();
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_Country_{normalizedCountry}");
                }
            }
        }

        await base.OnConnectedAsync();
    }

    public async Task JoinCityGroup(string cityName)
    {
        if (!string.IsNullOrWhiteSpace(cityName))
        {
            var normalizedCity = cityName.Trim().ToLowerInvariant();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_City_{normalizedCity}");
        }
    }

    public async Task LeaveCityGroup(string cityName)
    {
        if (!string.IsNullOrWhiteSpace(cityName))
        {
            var normalizedCity = cityName.Trim().ToLowerInvariant();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Group_City_{normalizedCity}");
        }
    }

    public async Task JoinGovernorateGroup(string governorateName)
    {
        if (!string.IsNullOrWhiteSpace(governorateName))
        {
            var normalizedGov = governorateName.Trim().ToLowerInvariant();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_Governorate_{normalizedGov}");
        }
    }

    public async Task LeaveGovernorateGroup(string governorateName)
    {
        if (!string.IsNullOrWhiteSpace(governorateName))
        {
            var normalizedGov = governorateName.Trim().ToLowerInvariant();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Group_Governorate_{normalizedGov}");
        }
    }

    public async Task JoinCountryGroup(string countryName)
    {
        if (!string.IsNullOrWhiteSpace(countryName))
        {
            var normalizedCountry = countryName.Trim().ToLowerInvariant();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_Country_{normalizedCountry}");
        }
    }

    public async Task LeaveCountryGroup(string countryName)
    {
        if (!string.IsNullOrWhiteSpace(countryName))
        {
            var normalizedCountry = countryName.Trim().ToLowerInvariant();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Group_Country_{normalizedCountry}");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
