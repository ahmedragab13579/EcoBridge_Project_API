using System;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.Features.Auth.Validations;
using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Application.Features.Donation.Validation;
using EcoBridge.Application.Features.Reviews.Commands;
using EcoBridge.Application.Features.Reviews.Validation;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Application.UnitTests.Validation;

public class ApplicationValidatorTests
{
    [Fact]
    public void LoginValidator_Should_Reject_Missing_Request_And_Invalid_Email()
    {
        var validator = new LoginCommandValidator();

        var missingRequest = validator.Validate(new LoginCommand(null!));
        var invalidEmail = validator.Validate(new LoginCommand(new LoginRequestDTO
        {
            Email = "invalid-email",
            Password = "password"
        }));

        missingRequest.IsValid.Should().BeFalse();
        invalidEmail.IsValid.Should().BeFalse();
        invalidEmail.Errors.Should().Contain(x => x.PropertyName == "Request.Email");
    }

    [Fact]
    public void ClaimDonationValidator_Should_Require_Donation_And_Charity()
    {
        var result = new ClaimDonationCommandValidator().Validate(
            new ClaimDonationCommand(Guid.Empty, Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void CreateDonationValidator_Should_Reject_Invalid_Account_And_Category()
    {
        var result = new CreateDonationCommandValidator().Validate(
            new CreateDonationCommand(Guid.Empty, new CreateDonationDTO
            {
                Quantity = "10 boxes",
                PickupLocation = "Location",
                CategoryId = 0,
                ExpiryDate = DateTime.UtcNow.AddDays(1)
            }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "AccountId");
        result.Errors.Should().Contain(x => x.PropertyName == "Dto.CategoryId");
    }

    [Fact]
    public void CreateReviewValidator_Should_Reject_Invalid_Rating_And_Participants()
    {
        var result = new CreateReviewCommandValidator().Validate(
            new CreateReviewCommand(Guid.Empty, new CreateReviewDTO
            {
                Rating = 0,
                RevieweeId = Guid.Empty,
                MissionId = Guid.Empty,
                ReviewReasonId = 0
            }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateReviewCommand.ReviewerId));
        result.Errors.Should().Contain(x => x.PropertyName == "Dto.Rating");
        result.Errors.Should().Contain(x => x.PropertyName == "Dto.RevieweeId");
        result.Errors.Should().Contain(x => x.PropertyName == "Dto.MissionId");
    }
}
