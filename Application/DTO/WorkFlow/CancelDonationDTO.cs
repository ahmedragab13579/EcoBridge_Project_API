using System.ComponentModel.DataAnnotations;

namespace EcoBridge.Application.DTO.WorkFlow;

public class CancelDonationDTO
{
    public string Reason { get; set; } = string.Empty;
}
