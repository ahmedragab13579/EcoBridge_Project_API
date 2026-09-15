namespace EcoBridge.Application.DTO;

public class DonorDashboardDTO
{
    public int TotalDonationsCreated { get; set; }
    public int TotalItemsSaved { get; set; }
    public int ActivePendingRequests { get; set; }
    public int TotalCompletedDonations { get; set; }
    public int TotalPoints { get; set; }
    public int TrustScore { get; set; }
}

public class VolunteerDashboardDTO
{
    public int CompletedDeliveries { get; set; }
    public int TotalPoints { get; set; }
    public int ActivePenaltyWarnings { get; set; }
    public int ActiveDeliveriesCount { get; set; }
    public int TrustScore { get; set; }
}

public class CharityDashboardDTO
{
    public int TotalFoodReceived { get; set; }
    public int PendingRequests { get; set; }
    public int ActiveDeliveriesInTransit { get; set; }
    public int ApprovedRequestsCount { get; set; }
    public int TrustScore { get; set; }
}
