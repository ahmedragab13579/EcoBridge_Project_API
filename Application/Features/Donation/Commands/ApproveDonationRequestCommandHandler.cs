using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.Features.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public class ApproveDonationRequestCommandHandler(
    IEcoBridgeDbContext dbContext,
    IPublisher publisher
) : IRequestHandler<ApproveDonationRequestCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ApproveDonationRequestCommand request, CancellationToken cancellationToken)
    {
        var donationRequest = await dbContext.DonationRequests
            .Include(r => r.Donation)
                .ThenInclude(d => d.City)
                    .ThenInclude(c => c!.Governorate)
                        .ThenInclude(g => g.Country)
            .Include(r => r.Donation)
                .ThenInclude(d => d.Donor)
                    .ThenInclude(donor => donor.Account)
                        .ThenInclude(a => a.City)
                            .ThenInclude(c => c!.Governorate)
                                .ThenInclude(g => g.Country)
            .Include(r => r.Charity)
                .ThenInclude(c => c.Account)
            .FirstOrDefaultAsync(r => r.Id == request.DonationRequestId, cancellationToken);

        if (donationRequest == null)
            return Result<Guid>.FailResult(Guid.Empty, AppMessageCodes.DONATION_REQUEST_NOT_FOUND, ErrorType.NotFound);

        if (donationRequest.Donation.DonorId != request.DonorAccountId)
            return Result<Guid>.FailResult(Guid.Empty, AppMessageCodes.FORBIDDEN, ErrorType.Forbidden);
      

        if (donationRequest.RequestStatus != RequestStatus.Pending)
            return Result<Guid>.FailResult(Guid.Empty, AppMessageCodes.DONATION_CANNOT_BE_MODIFIED, ErrorType.Conflict);

        if (donationRequest.Donation.Status != DonationStatus.Pending && donationRequest.Donation.Status != DonationStatus.Accepted)
            return Result<Guid>.FailResult(Guid.Empty, AppMessageCodes.DONATION_CANNOT_BE_MODIFIED, ErrorType.Conflict);

        // Generate 4-digit OTP
        var otpCode = Random.Shared.Next(1000, 9999).ToString("D4");

        donationRequest.Approve();
        donationRequest.Donation.Status = DonationStatus.SearchingForVolunteer;

        var dropOffLocation = !string.IsNullOrWhiteSpace(donationRequest.Charity.Account.FullName)
            ? $"{donationRequest.Charity.Account.FullName} Distribution Center"
            : "Charity Dropoff Center";

        var mission = DeliveryMission.Create(
            donationRequest.Id,
            donationRequest.Donation.PickupLocation,
            dropOffLocation,
            otpCode,
            DateTime.UtcNow.AddDays(3)
        );

        dbContext.DeliveryMissions.Add(mission);
        await dbContext.SaveChangesAsync(cancellationToken);

        var city = donationRequest.Donation.City ?? donationRequest.Donation.Donor?.Account?.City;
        var cityName = city?.NameEn ?? donationRequest.Donation.PickupLocation;
        var cityId = donationRequest.Donation.CityId ?? donationRequest.Donation.Donor?.Account?.CityId;
        var governorateName = city?.Governorate?.NameEn ?? string.Empty;
        var countryName = city?.Governorate?.Country?.NameEn ?? string.Empty;

        // Publish event for real-time notification / SignalR broadcast
        await publisher.Publish(new MissionCreatedEvent(
            mission.Id,
            donationRequest.DonationId,
            donationRequest.Id,
            mission.PickupLocation,
            mission.DropOffLocation,
            donationRequest.Donation.DonorId,
            donationRequest.CharityId,
            cityName,
            cityId,
            governorateName,
            countryName
        ), cancellationToken);

        return Result<Guid>.SuccessResult(mission.Id, AppMessageCodes.DONATION_REQUEST_APPROVED);
    }
}
