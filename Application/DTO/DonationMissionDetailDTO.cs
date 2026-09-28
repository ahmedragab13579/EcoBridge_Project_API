namespace EcoBridge.Application.DTO;

public class DonationMissionDetailDTO
{
    // ── Mission ───────────────────────────────────────────
    public Guid MissionId { get; set; }
    public string MissionStatus { get; set; } = string.Empty;
    public DateTime? PickupTime { get; set; }
    public DateTime? DeliveryTime { get; set; }
    public DateTime MissionCreatedAt { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DropOffLocation { get; set; } = string.Empty;

    // ── Volunteer ─────────────────────────────────────────
    public Guid VolunteerId { get; set; }
    public string VolunteerName { get; set; } = string.Empty;
    public string? VolunteerPhone { get; set; }
    public string? VolunteerImageUrl { get; set; }

    // ── Donation ──────────────────────────────────────────
    public Guid DonationId { get; set; }
    public string DonationTitle { get; set; } = string.Empty;
    public string? DonationDescription { get; set; }
    public string? FoodType { get; set; }
    public string QuantityDes { get; set; } = string.Empty;
    public decimal? Weight { get; set; }
    public string? DonationImageUrl { get; set; }

    // ── Charity (receiver) ────────────────────────────────
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    public string? CharityAddress { get; set; }
}