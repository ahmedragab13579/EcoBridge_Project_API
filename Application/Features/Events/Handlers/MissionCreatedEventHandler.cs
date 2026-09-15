using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Application.Hubs;
using EcoBridge.Application.Features.Notifications;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Events.Handlers;

public class MissionCreatedEventHandler(
    IHubContext<MissionHub> hubContext,
    INotificationService notificationService,
    IEcoBridgeDbContext dbContext
) : INotificationHandler<MissionCreatedEvent>
{
    public async Task Handle(MissionCreatedEvent notification, CancellationToken cancellationToken)
    {
        var missionData = new
        {
            missionId = notification.MissionId,
            donationId = notification.DonationId,
            pickupLocation = notification.PickupLocation,
            dropOffLocation = notification.DropOffLocation,
            cityName = notification.CityName,
            governorateName = notification.GovernorateName,
            countryName = notification.CountryName,
            createdAt = DateTime.UtcNow
        };

        // 1. Broadcast specifically to the targeted City, Governorate, and Country Groups in SignalR
        var targetCity = !string.IsNullOrWhiteSpace(notification.CityName) 
            ? notification.CityName.Trim().ToLowerInvariant() 
            : notification.PickupLocation.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(targetCity))
        {
            await hubContext.Clients.Group($"Group_City_{targetCity}")
                .SendAsync("MissionAvailable", missionData, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(notification.GovernorateName))
        {
            var targetGov = notification.GovernorateName.Trim().ToLowerInvariant();
            await hubContext.Clients.Group($"Group_Governorate_{targetGov}")
                .SendAsync("MissionAvailable", missionData, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(notification.CountryName))
        {
            var targetCountry = notification.CountryName.Trim().ToLowerInvariant();
            await hubContext.Clients.Group($"Group_Country_{targetCountry}")
                .SendAsync("MissionAvailable", missionData, cancellationToken);
        }

        // Also broadcast to all as general channel
        await hubContext.Clients.All
            .SendAsync("MissionAvailable", missionData, cancellationToken);

        // 2. Query volunteers belonging to the target city / governorate (active and available)
        var volunteerQuery = dbContext.Volunteers
            .Include(v => v.Account)
                .ThenInclude(a => a.City)
                    .ThenInclude(c => c!.Governorate)
            .AsNoTracking()
            .Where(v => v.Account.IsActive && v.IsAvailable);

        List<Guid> volunteerIds = new();

        if (notification.CityId.HasValue && notification.CityId.Value > 0)
        {
            volunteerIds = await volunteerQuery
                .Where(v => v.Account.CityId == notification.CityId.Value)
                .Select(v => v.AccountId)
                .Take(50)
                .ToListAsync(cancellationToken);
        }

        if (volunteerIds.Count == 0 && !string.IsNullOrWhiteSpace(notification.GovernorateName))
        {
            var targetGov = notification.GovernorateName.Trim().ToLowerInvariant();
            volunteerIds = await volunteerQuery
                .Where(v => v.Account.City != null && v.Account.City.Governorate != null &&
                            (v.Account.City.Governorate.NameEn.ToLower() == targetGov || v.Account.City.Governorate.NameAr.ToLower() == targetGov))
                .Select(v => v.AccountId)
                .Take(50)
                .ToListAsync(cancellationToken);
        }

        // If no city/governorate-specific volunteers found, fall back to active available volunteers
        if (volunteerIds.Count == 0)
        {
            volunteerIds = await dbContext.Volunteers
                .Where(v => v.Account.IsActive && v.IsAvailable)
                .Select(v => v.AccountId)
                .Take(20)
                .ToListAsync(cancellationToken);
        }

        // 3. Dispatch in-app notifications to filtered volunteers
        foreach (var volunteerId in volunteerIds)
        {
            await notificationService.CreateNotificationAsync(
                volunteerId,
                "New Delivery Mission in Your Area!",
                $"A new delivery mission #{notification.MissionId} is available in {notification.PickupLocation}. Open your app to accept.",
                NotificationType.MissionAvailable,
                notification.MissionId,
                cancellationToken);
        }
    }
}
