using System.ComponentModel.DataAnnotations;

namespace EcoBridge.Application.DTO;

public class CancelDonationDTO
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
