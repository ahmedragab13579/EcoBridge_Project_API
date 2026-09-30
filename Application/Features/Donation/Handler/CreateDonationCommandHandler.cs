using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Handler;

public class CreateDonationCommandHandler(
    IEcoBridgeDbContext dbContext,
    IPhotoService photoService
) : IRequestHandler<CreateDonationCommand, ApiResult<DonationResponseDTO>>
{
    public async Task<ApiResult<DonationResponseDTO>> Handle(
        CreateDonationCommand request,
        CancellationToken cancellationToken)
    {
        var donor = await dbContext.Accounts
            .FirstOrDefaultAsync(d => d.Id == request.AccountId, cancellationToken);

        if (donor == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONOR_NOT_FOUND);

        string? imageUrl = null;
        if (request.Dto.Image != null && photoService.IsEnabled)
        {
            var uploadResult = await photoService.UploadImageAsync(request.Dto.Image, cancellationToken);

            if (string.IsNullOrWhiteSpace(uploadResult))
                throw new DomainException.FailureException(AppMessageCodes.IMAGE_UPLOAD_FAILED);

            imageUrl = uploadResult;
        }

        var foodType = !string.IsNullOrWhiteSpace(request.Dto.FoodType)
            ? request.Dto.FoodType.Trim()
            : (!string.IsNullOrWhiteSpace(request.Dto.Title) ? request.Dto.Title.Trim() : "General Food");

        var cityId = request.Dto.CityId;

        var donation = Domain.Models.Donation.Donation.Create(
            donor.Id,
            foodType,
            request.Dto.Quantity.Trim(),
            request.Dto.ExpiryDate,
            request.Dto.PickupLocation.Trim(),
            cityId,
            request.Dto.Title.Trim(),
            request.Dto.Description.Trim(),
            request.Dto.CategoryId,
            request.Dto.Weight,
            imageUrl
        );

        dbContext.Donations.Add(donation);
        await dbContext.SaveChangesAsync(cancellationToken);

        var responseDto = new DonationResponseDTO
        {
            Id = donation.Id,
            CategoryId = donation.CategoryId,
            Title = donation.Title,
            Description = donation.Description,
            FoodType = donation.FoodType,
            QuantityDes = donation.QuantityDescription,
            Weight = donation.WeightKg,
            ExpiryDate = donation.ExpiryDate,
            PickupLocation = donation.PickupLocation,
            CityId = donation.CityId,
            ImageUrl = donation.ImageUrl,
            Status = donation.Status.ToString(),
            DonorId = donor.Id,
            DonorName = donor.FullName ?? string.Empty,
            CreatedAt = donation.CreatedAt
        };

        return ApiResult<DonationResponseDTO>.Ok(responseDto, AppMessageCodes.DONATION_CREATED);
    }
}