using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using EcoBridge.Domain.Models.Mission;
using EcoBridge.Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EcoBridge.Infrastructure.Data;

public class EcoBridgeDbContext(DbContextOptions<EcoBridgeDbContext> options)
    : IdentityDbContext<Account, ApplicationRole, Guid>(options), IEcoBridgeDbContext
{
    // ═════════════════════════════════════════════════════════════
    //  DbSets — grouped by domain area
    // ═════════════════════════════════════════════════════════════

    // ── Identity ────────────────────────────────────────────────
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // ── Donations & Missions ────────────────────────────────────
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<DeliveryMission> DeliveryMissions => Set<DeliveryMission>();
    public DbSet<Category> Categories => Set<Category>();

    // ── Reviews & Trust ─────────────────────────────────────────
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewReason> ReviewReasons => Set<ReviewReason>();
    public DbSet<PenaltyLog> PenaltyLogs => Set<PenaltyLog>();

    // ── Points & Notifications ──────────────────────────────────
    public DbSet<PointTransaction> PointTransactions => Set<PointTransaction>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    // ── Locations (Lookup tables — intentionally anemic) ────────
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<City> Cities => Set<City>();

    public DbSet<TrustCreditLog> TrustCreditLogs => Set<TrustCreditLog>();

    // ═════════════════════════════════════════════════════════════
    //  SaveChanges — soft delete interception & outbox collection
    // ═════════════════════════════════════════════════════════════
    public override int SaveChanges()
    {
        ApplySoftDelete();
        CollectDomainEventsToOutbox();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplySoftDelete();
        CollectDomainEventsToOutbox();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySoftDelete();
        CollectDomainEventsToOutbox();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplySoftDelete();
        CollectDomainEventsToOutbox();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void CollectDomainEventsToOutbox()
    {
        var aggregateRoots = ChangeTracker
            .Entries<EcoBridge.Domain.Models.Common.AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        if (aggregateRoots.Count == 0)
            return;

        foreach (var aggregateRoot in aggregateRoots)
        {
            foreach (var domainEvent in aggregateRoot.DomainEvents)
            {
                var eventType = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().FullName!;
                var content = Newtonsoft.Json.JsonConvert.SerializeObject(domainEvent);
                var outboxMessage = OutboxMessage.Create(eventType, content, domainEvent.OccurredOnUtc);
                OutboxMessages.Add(outboxMessage);
            }

            aggregateRoot.ClearDomainEvents();
        }
    }

    /// <summary>
    /// Intercepts physical deletes on <see cref="ISoftDeletable"/> entities and
    /// converts them into soft deletes by calling the entity's own
    /// <c>SoftDelete()</c> behavior — the entity is responsible for enforcing
    /// its own invariants (no external state mutation here).
    /// </summary>
    private void ApplySoftDelete()
    {
        var deletedEntries = ChangeTracker
            .Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted);

        foreach (var entry in deletedEntries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.SoftDelete();
        }
    }

    // ═════════════════════════════════════════════════════════════
    //  Model creation
    // ═════════════════════════════════════════════════════════════
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureIdentity(modelBuilder);          // 1. Identity tables
        ApplyGlobalConventions(modelBuilder);     // 2. Global rules
        ApplyEntityConfigurations(modelBuilder);  // 3. Per-entity
    }

    // ─────────────────────────────────────────────────────────────
    //  1. Identity
    // ─────────────────────────────────────────────────────────────
    private static void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(b =>
        {
            b.ToTable("Accounts");
            b.Property(a => a.FullName).HasMaxLength(150).IsRequired();
            b.HasIndex(a => a.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique(false);
            b.HasIndex(a => a.NormalizedEmail).HasDatabaseName("EmailIndex").IsUnique(false);
        });

        modelBuilder.Entity<ApplicationRole>(b => b.ToTable("Roles"));

        modelBuilder.Entity<IdentityUserRole<Guid>>(b => b.ToTable("UserRoles"));

        modelBuilder.Ignore<IdentityUserClaim<Guid>>();
        modelBuilder.Ignore<IdentityUserLogin<Guid>>();
        modelBuilder.Ignore<IdentityUserToken<Guid>>();
        modelBuilder.Ignore<IdentityRoleClaim<Guid>>();
    }
    // ─────────────────────────────────────────────────────────────
    //  2. Global conventions
    // ─────────────────────────────────────────────────────────────
    private static void ApplyGlobalConventions(ModelBuilder modelBuilder)
    {
        // ── Soft-delete query filters — applied selectively ─────
        //
        //  Included : Account, Donation, Review
        //  Excluded : DeliveryMission — always loaded through its parent
        //             Donation (which is itself filtered). Applying a filter
        //             here would break Donation.GetActiveMission().
        //
        var softDeletableTypes = new[]
        {
            typeof(Account),
            typeof(Donation),
            typeof(Review),
        };

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!softDeletableTypes.Contains(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isDeleted = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(isDeleted), parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  3. Per-entity configurations
    // ─────────────────────────────────────────────────────────────
    private static void ApplyEntityConfigurations(ModelBuilder modelBuilder)
    {
        // ── Identity & Auth ─────────────────────────────────────
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

        // ── Donations & Missions ────────────────────────────────
        modelBuilder.ApplyConfiguration(new DonationConfiguration());
        modelBuilder.ApplyConfiguration(new DeliveryMissionConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());

        // ── Reviews & Trust ─────────────────────────────────────
        modelBuilder.ApplyConfiguration(new ReviewConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewReasonConfiguration());
        modelBuilder.ApplyConfiguration(new PenaltyLogConfiguration());

        // ── Points & Notifications ──────────────────────────────
        modelBuilder.ApplyConfiguration(new PointTransactionConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new TrustCreditLogConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        // ── Locations ───────────────────────────────────────────
        modelBuilder.ApplyConfiguration(new CountryConfiguration());
        modelBuilder.ApplyConfiguration(new GovernorateConfiguration());
        modelBuilder.ApplyConfiguration(new CityConfiguration());
    }
}