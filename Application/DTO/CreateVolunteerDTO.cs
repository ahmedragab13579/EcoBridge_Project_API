namespace EcoBridge.Application.DTO
{
    public class CreateVolunteerDTO
    {
        public Guid AccountId { get; set; }
        public string VehicleDetails { get; set; } = string.Empty;

        // Optional account data when creating a volunteer as admin
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; }
    }
}
