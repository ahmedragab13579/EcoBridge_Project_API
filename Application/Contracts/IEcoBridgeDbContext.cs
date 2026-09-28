using EcoBridge.Domain.Models;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using EcoBridge.Domain.Models.Mission;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Contracts;

public interface IEcoBridgeDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<Donation> Donations { get; }
    DbSet<TrustCreditLog> TrustCreditLogs { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    // Domain tables
    DbSet<Category> Categories { get; }
    DbSet<DeliveryMission> DeliveryMissions { get; }
    DbSet<Country> Countries { get; }
    DbSet<Governorate> Governorates { get; }
    DbSet<City> Cities { get; }
    DbSet<ReviewReason> ReviewReasons { get; }
    DbSet<Review> Reviews { get; }
    DbSet<PointTransaction> PointTransactions { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<PenaltyLog> PenaltyLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

      
}
