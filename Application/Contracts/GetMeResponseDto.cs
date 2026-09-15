namespace EcoBridge.Application.Contracts;

public class GetMeResponseDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; }
    public int? CityId { get; set; }
}
