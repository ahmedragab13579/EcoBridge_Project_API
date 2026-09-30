using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public record UpdateDonationCommand(
    Guid DonationId,
    Guid AccountId,
    UpdateDonationDTO Dto
) : IRequest<ApiResult<bool>>;

public class UpdateDonationCommandHandler(
    IEcoBridgeDbContext dbContext,
    IPhotoService photoService
) : IRequestHandler<UpdateDonationCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(
        UpdateDonationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch donation directly
        var donation = await dbContext.Donations
            .FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);

        if (donation is null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONATION_NOT_FOUND);

        // 2. Direct Ownership check (Saves an unnecessary DB query)
        if (donation.DonorId != request.AccountId)
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        // 3. Status Guard: Only editable when still Available
        if (donation.Status != DonationStatus.Available)
            throw new DomainException.ValidationException(AppMessageCodes.DONATION_CANNOT_BE_MODIFIED);

        // 4. Handle image upload if a new photo is provided
        string? newImageUrl = donation.ImageUrl;
        if (request.Dto.Image is not null && photoService.IsEnabled)
        {
            var uploadResult = await photoService.UploadImageAsync(request.Dto.Image, cancellationToken);

            if (string.IsNullOrWhiteSpace(uploadResult))
                throw new DomainException.FailureException(AppMessageCodes.IMAGE_UPLOAD_FAILED);

            newImageUrl = uploadResult;
        }

        // 5. Fallback to existing values if properties in DTO are null/empty
        var foodType = !string.IsNullOrWhiteSpace(request.Dto.FoodType)
            ? request.Dto.FoodType
            : donation.FoodType;

        var quantity = !string.IsNullOrWhiteSpace(request.Dto.Quantity)
            ? request.Dto.Quantity
            : donation.QuantityDescription;

        var expiry = request.Dto.ExpiryDate;

        var pickup = !string.IsNullOrWhiteSpace(request.Dto.PickupLocation)
            ? request.Dto.PickupLocation
            : donation.PickupLocation;

        // 6. Update domain model
        donation.UpdateDetails(
            donation.Title,
            donation.Description,
            foodType,
            quantity,
            donation.WeightKg,
            expiry,
            pickup,
            donation.CategoryId,
            request.Dto.CityId,
            newImageUrl
        );

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.DONATION_UPDATED);
    }
}