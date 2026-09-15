using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EcoBridge.Application.DTO;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Application.UnitTests.Validation;

public class DtoValidationTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void CreateDonationDTO_Should_Fail_When_Required_Fields_Are_Missing()
    {
        // Arrange
        var dto = new CreateDonationDTO
        {
            Quantity = null!,
            PickupLocation = null!
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateDonationDTO.Quantity)));
        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateDonationDTO.PickupLocation)));
    }

    [Fact]
    public void CreateDonationDTO_Should_Pass_When_Valid()
    {
        // Arrange
        var dto = new CreateDonationDTO
        {
            Title = "Valid Donation",
            Quantity = "10 boxes",
            PickupLocation = "123 Street",
            ExpiryDate = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void ChangePasswordDTO_Should_Fail_When_Password_Length_Below_Minimum()
    {
        // Arrange
        var dto = new ChangePasswordDTO
        {
            CurrentPassword = "old_password_123",
            NewPassword = "123" // MinLength is 6
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains(nameof(ChangePasswordDTO.NewPassword)));
    }

    [Fact]
    public void ChangePasswordDTO_Should_Fail_When_Required_Fields_Are_Null()
    {
        // Arrange
        var dto = new ChangePasswordDTO
        {
            CurrentPassword = null!,
            NewPassword = null!
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains(nameof(ChangePasswordDTO.CurrentPassword)));
        results.Should().Contain(r => r.MemberNames.Contains(nameof(ChangePasswordDTO.NewPassword)));
    }

    [Fact]
    public void UpdateProfileDTO_Should_Fail_When_Required_Fields_Are_Null()
    {
        // Arrange
        var dto = new UpdateProfileDTO
        {
            FullName = null!,
            Phone = null!
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains(nameof(UpdateProfileDTO.FullName)));
        results.Should().Contain(r => r.MemberNames.Contains(nameof(UpdateProfileDTO.Phone)));
    }

    [Fact]
    public void UpdateProfileDTO_Should_Pass_When_Valid()
    {
        // Arrange
        var dto = new UpdateProfileDTO
        {
            FullName = "Ahmed Hassan",
            Phone = "+201001234567",
            CityId = 2,
            VehicleType = "Car",
            Address = "Dokki, Giza"
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CancelDonationDTO_Should_Fail_When_Reason_Is_Null()
    {
        // Arrange
        var dto = new CancelDonationDTO
        {
            Reason = null!
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains(nameof(CancelDonationDTO.Reason)));
    }

    [Fact]
    public void CancelDonationDTO_Should_Pass_When_Reason_Is_Provided()
    {
        // Arrange
        var dto = new CancelDonationDTO
        {
            Reason = "Store closing early"
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        results.Should().BeEmpty();
    }
}
