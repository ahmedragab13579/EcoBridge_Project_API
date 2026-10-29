namespace EcoBridge.Application.DTO.Users.Auth;

public class PublicProfileDTO
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int AverageRating { get; set; }
    public int AverageImpression { get; set; }
    public int TotalReviews { get; set; }
    public int TotalActivities { get; set; } 
    public DateTime JoinDate { get; set; }
    public string Role { get; set; } = string.Empty;
    public int TrustScore { get; set; }
    public string? City { get; set; }
    public string? OrganizationName { get; set; }
}
