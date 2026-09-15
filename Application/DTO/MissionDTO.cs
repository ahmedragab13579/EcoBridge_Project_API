namespace EcoBridge.Application.DTO;

public class MissionDTO
{
    public Guid Id { get; set; }
    public Guid DonationRequestId { get; set; }
    public Guid? VolunteerId { get; set; }
    public string? VolunteerName { get; set; }
    public string MissionStatus { get; set; } = string.Empty;
    public string PickupLocation { get; set; } = string.Empty;
    public string DropOffLocation { get; set; } = string.Empty;
    public DateTime? PickupTime { get; set; }
    public DateTime? DeliveryTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? FoodType { get; set; }
    public string? Quantity { get; set; }
}

public class CompleteMissionRequestDTO
{
    public string OtpCode { get; set; } = string.Empty;
}

public class AcceptMissionResponseDTO
{
    public Guid MissionId { get; set; }
    public string Status { get; set; } = "Accepted";
    public string PickupLocation { get; set; } = string.Empty;
}
