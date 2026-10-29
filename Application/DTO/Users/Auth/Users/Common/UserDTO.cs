using EcoBridge.Domain.Enums;

namespace EcoBridge.Application.DTO.Users.Auth.Users.Common;

public class UserDTO : MiniUserDto
{

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalWalletPoints { get; set; }
    public int TrustScore { get; set; } = 100;
    public int? CityId { get; set; }
    public string? Address { get; set; }
    public string? AddressDetails { get; set; }
    public string? ImagePath { get; set; }

}
