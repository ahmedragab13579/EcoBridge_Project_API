namespace EcoBridge.Domain.Enums;

public enum DonationStatus : byte
{
    Available = 0,
    Pending = 1,
    SearchingForVolunteer = 2,
    Accepted = 3,
    InTransit = 4,
    Delivered = 5,
    Cancelled = 6,
    Expired = 7,
    VolunteerAssigned = 8,
}
