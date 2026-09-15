using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Contracts;
using EcoBridge.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Infrastructure.Data;

public class EcoBridgeDbContext(DbContextOptions<EcoBridgeDbContext> options) 
    : IdentityDbContext<Account, ApplicationRole, Guid>(options), IEcoBridgeDbContext
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Donor> Donors => Set<Donor>();
    public DbSet<Charity> Charities => Set<Charity>();
    public DbSet<Volunteer> Volunteers => Set<Volunteer>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Domain tables
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<DonationRequest> DonationRequests => Set<DonationRequest>();
    public DbSet<DeliveryMission> DeliveryMissions => Set<DeliveryMission>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<ReviewReason> ReviewReasons => Set<ReviewReason>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<PointTransaction> PointTransactions => Set<PointTransaction>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PenaltyLog> PenaltyLogs => Set<PenaltyLog>();

    public override int SaveChanges()
    {
        HandleSoftDelete();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        HandleSoftDelete();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        HandleSoftDelete();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void HandleSoftDelete()
    {
        var entries = ChangeTracker.Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = DateTime.UtcNow;

            if (entry.Entity is Account account)
            {
                account.Deactivate();
            }
            else if (entry.Entity is Volunteer volunteer)
            {
                volunteer.SetAvailability(false);
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map selective Identity tables to existing conventions
        modelBuilder.Entity<Account>(b =>
        {
            b.ToTable("Accounts");
            b.Property(u => u.FullName).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<ApplicationRole>(b =>
        {
            b.ToTable("Roles");
        });

        modelBuilder.Entity<IdentityUserRole<Guid>>(b =>
        {
            b.ToTable("UserRoles");
        });

        // Exclude unneeded bloated Identity tables
        modelBuilder.Ignore<IdentityUserClaim<Guid>>();
        modelBuilder.Ignore<IdentityUserLogin<Guid>>();
        modelBuilder.Ignore<IdentityUserToken<Guid>>();
        modelBuilder.Ignore<IdentityRoleClaim<Guid>>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EcoBridgeDbContext).Assembly);
    }
}
