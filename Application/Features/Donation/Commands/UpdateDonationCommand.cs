using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public record UpdateDonationCommand(
    Guid DonationId,
    Guid AccountId,
    UpdateDonationDTO Dto
) : IRequest<Result<bool>>;

public class UpdateDonationCommandHandler(
    IEcoBridgeDbContext dbContext,
    IPhotoService photoService
) : IRequestHandler<UpdateDonationCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateDonationCommand request, CancellationToken cancellationToken)
    {
        var donor = await dbContext.Donors.FirstOrDefaultAsync(d => d.AccountId == request.AccountId, cancellationToken);
        if (donor == null)
            return Result<bool>.FailResult(false, AppMessageCodes.DONOR_NOT_FOUND, ErrorType.NotFound);

        var donation = await dbContext.Donations.FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);
        if (donation == null)
            return Result<bool>.FailResult(false, AppMessageCodes.DONATION_NOT_FOUND, ErrorType.NotFound);

        if (donation.DonorId != donor.AccountId)
            return Result<bool>.FailResult(false, AppMessageCodes.FORBIDDEN, ErrorType.Forbidden);

        if (donation.Status != DonationStatus.Pending && donation.Status != DonationStatus.Available)
            return Result<bool>.FailResult(false, AppMessageCodes.DONATION_CANNOT_BE_MODIFIED, ErrorType.Conflict);

        string? newImageUrl = donation.ImageUrl;
        if (request.Dto.Image != null && photoService.IsEnabled)
        {
            var uploadResult = await photoService.UploadImageAsync(request.Dto.Image, cancellationToken);
            if (!uploadResult.Success)
                return Result<bool>.FailResult(false, uploadResult.Message, uploadResult.ErrorType);

            newImageUrl = uploadResult.Value;
        }

        var foodType = !string.IsNullOrWhiteSpace(request.Dto.FoodType) ? request.Dto.FoodType : donation.FoodType;
        var quantity = !string.IsNullOrWhiteSpace(request.Dto.Quantity) ? request.Dto.Quantity : donation.Quantity;
        var expiry = request.Dto.ExpiryDate ?? donation.ExpiryDate;
        var pickup = !string.IsNullOrWhiteSpace(request.Dto.PickupLocation) ? request.Dto.PickupLocation : donation.PickupLocation;

        donation.UpdateDetails(donation.Title, donation.Description, foodType, quantity, donation.Weight, expiry, pickup, donation.CategoryId, newImageUrl, request.Dto.CityId);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.SuccessResult(true, AppMessageCodes.DONATION_UPDATED);
    }
}
