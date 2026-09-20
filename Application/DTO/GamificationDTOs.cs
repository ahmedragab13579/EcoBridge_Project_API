
using EcoBridge.Application.Results;

namespace EcoBridge.Application.DTO;

public class PointTransactionDTO
{
    public int Id { get; set; }
    public int Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MyPointsResponseDTO
{
    public int TotalPoints { get; set; }
    public int TrustScore { get; set; }
    public PagedResult<PointTransactionDTO> Transactions { get; set; } = new();
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
