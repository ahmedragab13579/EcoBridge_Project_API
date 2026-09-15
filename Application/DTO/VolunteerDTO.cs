namespace EcoBridge.Application.DTO;

public class VolunteerDTO
{
    public Guid AccountId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string VehicleDetails { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}
