using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public class CreateDonationCommandHandler(
    IEcoBridgeDbContext dbContext,
    IPhotoService photoService
) : IRequestHandler<CreateDonationCommand, Result<DonationResponseDTO>>
{
    public async Task<Result<DonationResponseDTO>> Handle(CreateDonationCommand request, CancellationToken cancellationToken)
    {
        var donor = await dbContext.Donors
            .Include(d => d.Account)
            .FirstOrDefaultAsync(d => d.AccountId == request.AccountId, cancellationToken);

        if (donor == null)
            return Result<DonationResponseDTO>.FailResult(new DonationResponseDTO(), AppMessageCodes.DONOR_NOT_FOUND, ErrorType.NotFound);

        string? imageUrl = null;
        if (request.Dto.Image != null)
        {
            if (photoService.IsEnabled)
            {
                var uploadResult = await photoService.UploadImageAsync(request.Dto.Image, cancellationToken);
                if (!uploadResult.Success)
                    return Result<DonationResponseDTO>.FailResult(new DonationResponseDTO(), uploadResult.Message, uploadResult.ErrorType);

                imageUrl = uploadResult.Value;
            }
        }

        var foodType = !string.IsNullOrWhiteSpace(request.Dto.FoodType)
            ? request.Dto.FoodType.Trim()
            : (!string.IsNullOrWhiteSpace(request.Dto.Title) ? request.Dto.Title.Trim() : "General Food");

        var cityId = request.Dto.CityId ?? donor.Account?.CityId;

        var donation = EcoBridge.Domain.Models.Donation.Create(
            donor.AccountId,
            foodType,
            request.Dto.Quantity.Trim(),
            request.Dto.ExpiryDate,
            request.Dto.PickupLocation.Trim(),
            request.Dto.CategoryId,
            request.Dto.Title?.Trim(),
            request.Dto.Description?.Trim(),
            request.Dto.Weight,
            imageUrl,
            cityId
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
            Quantity = donation.Quantity,
            Weight = donation.Weight,
            ExpiryDate = donation.ExpiryDate,
            PickupLocation = donation.PickupLocation,
            CityId = donation.CityId ?? cityId,
            ImageUrl = donation.ImageUrl,
            Status = donation.Status.ToString(),
            DonorId = donor.AccountId,
            DonorName = donor.Account?.FullName ?? string.Empty,
            CreatedAt = donation.CreatedAt
        };

        return Result<DonationResponseDTO>.SuccessResult(responseDto, AppMessageCodes.DONATION_CREATED);
    }
}
