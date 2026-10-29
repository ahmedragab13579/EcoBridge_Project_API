using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Queries;

public record GetDonationByIdQuery(Guid DonationId) : IRequest<ApiResult<DonationResponseDTO>>;

public class GetDonationByIdQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetDonationByIdQuery, ApiResult<DonationResponseDTO>>
{
    public async Task<ApiResult<DonationResponseDTO>> Handle(
        GetDonationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var donation = await dbContext.Donations
            .AsNoTracking()
            .Where(d => d.Id == request.DonationId && !d.IsDeleted)
            .Select(d => new DonationResponseDTO
            {
                Id = d.Id,
                CategoryId = d.CategoryId,
                Title = d.Title,
                Description = d.Description,
                FoodType = d.FoodType,
                QuantityDes = d.QuantityDescription,
                Weight = d.WeightKg,
                ExpiryDate = d.ExpiryDate,
                PickupLocation = d.PickupLocation,
                CityId = d.CityId,
                ImageUrl = d.ImageUrl,
                Status = d.Status.ToString(),
                DonorId = d.DonorId,
                DonorName = d.Donor != null ? d.Donor.FullName : string.Empty,
                CreatedAt = d.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (donation == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONATION_NOT_FOUND);

        return ApiResult<DonationResponseDTO>.Ok(donation, AppMessageCodes.DONATION_RETRIEVED);
    }
}