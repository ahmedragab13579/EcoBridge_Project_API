using System;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Accounts;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace EcoBridge.Domain.UnitTests;

public class AccountTests
{
    private readonly PasswordHasher<Account> _passwordHasher = new();

    [Fact]
    public void Create_Should_Initialize_Active_And_Default_Scores()
    {
        // Arrange & Act
        var hash = _passwordHasher.HashPassword(null!, "TestPassword123!");
        var account = Account.Create(
            fullName: "Ahmed Ali",
            email: "ahmed@example.com",
            phone: "+201001234567",
            passwordHash: hash,
            roleId: UserRole.Donor,
            userType: UserType.Individual,
            cityId: 5
        );

        // Assert
        account.Id.Should().NotBeEmpty();
        account.FullName.Should().Be("Ahmed Ali");
        account.Email.Should().Be("ahmed@example.com");
        account.NormalizedEmail.Should().Be("AHMED@EXAMPLE.COM");
        account.PhoneNumber.Should().Be("+201001234567");
        account.RoleId.Should().Be(UserRole.Donor);
        account.UserType.Should().Be(UserType.Individual);
        account.CityId.Should().Be(5);
        account.TotalWalletPoints.Should().Be(0);
        account.TrustScore.Should().Be(100);
        account.IsActive.Should().BeTrue();
        account.SecurityStamp.Should().NotBeNullOrWhiteSpace();
        account.PasswordHash.Should().Be(hash);
    }

    [Fact]
    public void UpdateProfile_Should_Modify_Name_Phone_And_City()
    {
        // Arrange
        var account = Account.Create("John Doe", "john@example.com", "0123456789", _passwordHasher.HashPassword(null!, "TestPassword123!"), UserRole.Volunteer);

        // Act
        account.UpdateProfile("Johnathan Doe", "0198765432", 10);

        // Assert
        account.FullName.Should().Be("Johnathan Doe");
        account.PhoneNumber.Should().Be("0198765432");
        account.CityId.Should().Be(10);
    }

    [Fact]
    public void UpdateProfileImage_Should_Modify_ImagePath()
    {
        // Arrange
        var account = Account.Create("Jane", "jane@example.com", "0123", _passwordHasher.HashPassword(null!, "TestPassword123!"), UserRole.Charity);

        // Act
        account.UpdateProfileImage("/uploads/profiles/jane.png");

        // Assert
        account.ImagePath.Should().Be("/uploads/profiles/jane.png");
    }

    [Fact]
    public void ChangePassword_Should_Update_PasswordHash()
    {
        // Arrange
        var initialHash = _passwordHasher.HashPassword(null!, "OldPassword123!");
        var newHash = _passwordHasher.HashPassword(null!, "NewPassword123!");
        var account = Account.Create("Jane", "jane@example.com", "0123", initialHash, UserRole.Donor);

        // Act
        account.ChangePassword(newHash);

        // Assert
        account.PasswordHash.Should().Be(newHash);
        var verify = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash!, "NewPassword123!");
        verify.Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void AddPoints_Should_Increment_Wallet_Points()
    {
        // Arrange
        var account = Account.Create("Donor", "donor@example.com", "0123", _passwordHasher.HashPassword(null!, "TestPassword123!"), UserRole.Donor);

        // Act
        account.AddPoints(50);
        account.AddPoints(25);

        // Assert
        account.TotalWalletPoints.Should().Be(75);
    }

    [Fact]
    public void DeductPoints_Should_Reduce_Wallet_Points_And_Not_Drop_Below_Zero()
    {
        // Arrange
        var account = Account.Create("Donor", "donor@example.com", "0123", _passwordHasher.HashPassword(null!, "TestPassword123!"), UserRole.Donor);
        account.AddPoints(40);

        // Act
        account.DeductPoints(15);

        // Assert
        account.TotalWalletPoints.Should().Be(25);

        // Deducting more points than balance should clamp at 0
        account.DeductPoints(100);
        account.TotalWalletPoints.Should().Be(0);
    }

    [Fact]
    public void AdjustTrustScore_Should_Clamp_Score_Between_0_And_100()
    {
        // Arrange (Default TrustScore is 100)
        var account = Account.Create("Volunteer", "vol@example.com", "0123", _passwordHasher.HashPassword(null!, "TestPassword123!"), UserRole.Volunteer);

        // Act & Assert - Deducting penalty
        account.AdjustTrustScore(-20);
        account.TrustScore.Should().Be(80);

        // Penalty exceeding remaining points clamps at 0
        account.AdjustTrustScore(-150);
        account.TrustScore.Should().Be(0);

        // Adding points
        account.AdjustTrustScore(60);
        account.TrustScore.Should().Be(60);

        // Exceeding maximum clamps at 100
        account.AdjustTrustScore(80);
        account.TrustScore.Should().Be(100);
    }

    [Fact]
    public void Activate_And_Deactivate_Should_Toggle_IsActive()
    {
        // Arrange
        var account = Account.Create("Admin", "admin@example.com", "0123", _passwordHasher.HashPassword(null!, "TestPassword123!"), UserRole.Admin);

        // Act & Assert
        account.Deactivate();
        account.IsActive.Should().BeFalse();

        account.Activate();
        account.IsActive.Should().BeTrue();
    }
}
