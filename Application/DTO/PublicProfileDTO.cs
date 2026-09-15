namespace EcoBridge.Application.DTO;

public class PublicProfileDTO
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public float AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalActivities { get; set; } // TotalMissions or Donations based on role
    public DateTime JoinDate { get; set; }
    public string Role { get; set; } = string.Empty;
    public int TrustScore { get; set; }
    public string? City { get; set; }
    public string? OrganizationName { get; set; }
}
