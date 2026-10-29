using EcoBridge.Application.Results;

namespace EcoBridge.Application.DTO.Statics.Gamification;

public class PointTransactionDTO
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MyPointsResponseDTO
{
    public int TotalPoints { get; set; }
    public int TrustScore { get; set; }
    public List<PointTransactionDTO> items { get; set; }
}

public class LeaderboardUserDTO
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int TrustScore { get; set; }
    public string? City { get; set; }
}
