namespace EcoBridge.Application.DTO.Users.Admin;

public class AdminStatsDTO
{
    public int TotalDonations { get; set; }
    public int TotalVolunteers { get; set; }
    public int CompletedDeliveries { get; set; }
    public int ActiveDonors { get; set; }

    public int AvailableDonations { get; set; }
    public int AcceptedDonations { get; set; }
    public int PickedUpDonations { get; set; }
    public int DeliveredDonations { get; set; }
    public int CancelledDonations { get; set; }
    public int CancelledMissions { get; set; }

    public int TotalCharities { get; set; }
}

