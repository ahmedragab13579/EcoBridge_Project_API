namespace EcoBridge.Domain.Enums;

public enum DonationStatus : byte
{
    Available = 0,
  
    SearchingForVolunteer = 2,
  
    VolunteerAssigned = 3,

    Cancelled = 4,

    Expired = 5,

    Completed = 6
}
