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
        var donor = await dbContext.Accounts
            .FirstOrDefaultAsync(d => d.Id == request.AccountId, cancellationToken);

        if (donor == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONOR_NOT_FOUND);

        var donation = await dbContext.Donations
            .FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);

        if (donation == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONATION_NOT_FOUND);

        if (donation.DonorId != donor.Id)
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        if (donation.Status != DonationStatus.Pending && donation.Status != DonationStatus.Available)
            throw new DomainException.ValidationException(AppMessageCodes.DONATION_CANNOT_BE_MODIFIED);

        string? newImageUrl = donation.ImageUrl;
        if (request.Dto.Image != null && photoService.IsEnabled)
        {
            var uploadResult = await photoService.UploadImageAsync(request.Dto.Image, cancellationToken);

            if (string.IsNullOrWhiteSpace(uploadResult))
                throw new DomainException.FailureException(AppMessageCodes.IMAGE_UPLOAD_FAILED);

            newImageUrl = uploadResult;
        }

        var foodType = !string.IsNullOrWhiteSpace(request.Dto.FoodType)
            ? request.Dto.FoodType
            : donation.FoodType;

        var quantity = !string.IsNullOrWhiteSpace(request.Dto.Quantity)
            ? request.Dto.Quantity
            : donation.QuantityDescription;

        var expiry = request.Dto.ExpiryDate ?? donation.ExpiryDate;

        var pickup = !string.IsNullOrWhiteSpace(request.Dto.PickupLocation)
            ? request.Dto.PickupLocation
            : donation.PickupLocation;

        donation.UpdateDetails(
            donation.Title,
            donation.Description,
            foodType,
            quantity,
            donation.WeightKg,
            expiry,
            pickup,
            donation.CategoryId,
            newImageUrl,
            request.Dto.CityId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.DONATION_UPDATED);
    }
}