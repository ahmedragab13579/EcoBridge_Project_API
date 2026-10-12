using System;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using EcoBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.UnitTests.Helpers;

public static class TestDbContextFactory
{
    public static EcoBridgeDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<EcoBridgeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationTestDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private sealed class ApplicationTestDbContext(DbContextOptions<EcoBridgeDbContext> options)
        : EcoBridgeDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(builder =>
            {
                builder.Ignore(account => account.City);
                builder.Ignore(account => account.DonationsMade);
                builder.Ignore(account => account.DonationsReceived);
                builder.Ignore(account => account.DonationsHandled);
                builder.Ignore(account => account.DeliveryMissions);
                builder.Ignore(account => account.RefreshTokens);
                builder.Ignore(account => account.PointTransactions);
                builder.Ignore(account => account.ReviewsGiven);
                builder.Ignore(account => account.ReviewsReceived);
                builder.Ignore(account => account.Notifications);
                builder.Ignore(account => account.PenaltyLogs);
                builder.Ignore(account => account.trustCreditLogs);
            });

            modelBuilder.Entity<Donation>(builder =>
            {
                builder.Ignore(donation => donation.Donor);
                builder.Ignore(donation => donation.Charity);
                builder.Ignore(donation => donation.Volunteer);
                builder.Ignore(donation => donation.Category);
                builder.Ignore(donation => donation.City);
            });

            modelBuilder.Ignore<EcoBridge.Domain.Models.Review>();
            modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityPasskeyData>();
            modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>();
            modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>();
            modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>();
            modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>();
            modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserPasskey<Guid>>();
            modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>();
        }
    }
}
