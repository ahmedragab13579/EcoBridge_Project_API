using MediatR;

namespace EcoBridge.Application.Features.Events;

public record MissionCreatedEvent(
    Guid MissionId,
    Guid DonationId,
    Guid DonationRequestId,
    string PickupLocation,
    string DropOffLocation,
    Guid DonorId,
    Guid CharityId,
    string CityName = "",
    int? CityId = null,
    string GovernorateName = "",
    string CountryName = ""
) : INotification;
