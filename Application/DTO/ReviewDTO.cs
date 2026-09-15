using System.ComponentModel.DataAnnotations;

namespace EcoBridge.Application.DTO;

public class CreateReviewDTO
{
    [Required]
    public Guid MissionId { get; set; }

    [Required]
    public Guid RevieweeId { get; set; }

    [Required]
    public int ReviewReasonId { get; set; }

    public int? Rating { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}

public class ReviewResponseDTO
{
    public int Id { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid RevieweeId { get; set; }
    public Guid MissionId { get; set; }
    public int ReviewReasonId { get; set; }
    public string ReviewReasonCode { get; set; } = string.Empty;
    public bool IsPositive { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
