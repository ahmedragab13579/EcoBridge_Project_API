using System;
using EcoBridge.Domain.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace EcoBridge.Domain.UnitTests;

public class SoftDeletableTests
{
    private readonly PasswordHasher<Account> _passwordHasher = new();

    [Fact]
    public void Account_Should_Support_SoftDelete_And_Restore()
    {
        // Arrange
        var account = Account.Create("User", "user@test.com", "010", _passwordHasher.HashPassword(null!, "Password123!"), UserRole.Donor);

        account.IsDeleted.Should().BeFalse();
        account.DeletedAt.Should().BeNull();
        account.IsActive.Should().BeTrue();

        // Act - Soft Delete
        account.SoftDelete();

        // Assert
        account.IsDeleted.Should().BeTrue();
        account.DeletedAt.Should().NotBeNull();
        account.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        account.IsActive.Should().BeFalse();

        // Act - Restore
        account.Restore();

        // Assert
        account.IsDeleted.Should().BeFalse();
        account.DeletedAt.Should().BeNull();
        account.IsActive.Should().BeTrue();
    }



    [Fact]
    public void Donation_Should_Support_SoftDelete()
    {
        // Arrange
        var donation = Donation.Create(
            Guid.NewGuid(),
            "Bread",
            "10",
            DateTime.UtcNow.AddDays(1),
            "Location",
            1,
            "Bread donation",
            "Fresh bread",
            1);

        donation.IsDeleted.Should().BeFalse();
        donation.DeletedAt.Should().BeNull();

        // Act - Soft Delete
        donation.SoftDelete();

        // Assert
        donation.IsDeleted.Should().BeTrue();
        donation.DeletedAt.Should().NotBeNull();

    }

    [Fact]
    public void Review_Should_Support_SoftDelete_And_Restore()
    {
        // Arrange
        var review = Review.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            1,
            Guid.NewGuid(),
            "Great donor");

        review.IsDeleted.Should().BeFalse();
        review.DeletedAt.Should().BeNull();

        // Act - Soft Delete
        review.SoftDelete();

        // Assert
        review.IsDeleted.Should().BeTrue();
        review.DeletedAt.Should().NotBeNull();

        // Act - Restore
        review.Restore();

        // Assert
        review.IsDeleted.Should().BeFalse();
        review.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void CoreEntities_Should_Implement_ISoftDeletable()
    {
        typeof(Account).Should().Implement<ISoftDeletable>();
        typeof(Donation).Should().Implement<ISoftDeletable>();
        typeof(Review).Should().Implement<ISoftDeletable>();
    }
}
