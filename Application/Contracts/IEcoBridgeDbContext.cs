using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Contracts;

public interface IEcoBridgeDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<Donor> Donors { get; }
    DbSet<Charity> Charities { get; }
    DbSet<Volunteer> Volunteers { get; }
    DbSet<Admin> Admins { get; }
    DbSet<Donation> Donations { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    // Domain tables
    DbSet<Category> Categories { get; }
    DbSet<DonationRequest> DonationRequests { get; }
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
