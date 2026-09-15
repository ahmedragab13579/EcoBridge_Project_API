using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Application.Features.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.BackgroundJobs;

public class CharityApprovalTimeoutJob(
    IEcoBridgeDbContext dbContext,
    INotificationService notificationService,
    ILogger<CharityApprovalTimeoutJob> logger
)
{
    public async Task ExecuteAsync(Guid requestId)
    {
        logger.LogInformation("Evaluating Charity Approval Timeout SLA (12h) for Request #{RequestId}", requestId);

        var request = await dbContext.DonationRequests
            .Include(r => r.Donation)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
        {
            logger.LogWarning("Donation Request #{RequestId} not found during timeout check", requestId);
            return;
        }

        // If request is still pending approval after 12 hours
        if (request.RequestStatus == RequestStatus.Pending)
        {
            logger.LogWarning("Donation Request #{RequestId} timed out after 12 hours. Releasing donation to public pool.", requestId);

            request.Reject();

            if (request.Donation != null && request.Donation.Status == DonationStatus.Pending)
            {
                request.Donation.ReleaseCharityRequest();
            }

            await dbContext.SaveChangesAsync();

            // Notify Charity
            await notificationService.CreateNotificationAsync(
                request.CharityId,
                "Request Timed Out",
                $"Your request for donation #{request.DonationId} has timed out after 12 hours without confirmation.",
                NotificationType.DonationStatusChanged,
                request.DonationId
            );

            // Notify Donor
            if (request.Donation != null)
            {
                await notificationService.CreateNotificationAsync(
                    request.Donation.DonorId,
                    "Donation Restored to Public Pool",
                    $"A pending request for your donation '{request.Donation.Title ?? request.Donation.FoodType}' timed out and has been returned to the public pool.",
                    NotificationType.DonationStatusChanged,
                    request.Donation.Id
                );
            }
        }
        else
        {
            logger.LogInformation("Request #{RequestId} is in status {Status}. No timeout action needed.",
                requestId, request.RequestStatus);
        }
    }
}
