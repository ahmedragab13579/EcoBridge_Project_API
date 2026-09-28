using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Queries;

public record GetAvailableDonationsQuery : IRequest<ApiResult<List<DonationResponseDTO>>>;

public class GetAvailableTasksQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetAvailableDonationsQuery, ApiResult<List<DonationResponseDTO>>>
{
    public async Task<ApiResult<List<DonationResponseDTO>>> Handle(
        GetAvailableDonationsQuery request,
        CancellationToken cancellationToken)
    {
        var donations = await dbContext.Donations
            .AsNoTracking()
            .Where(d => d.Status == DonationStatus.Available
                     && d.VolunteerId == null
                     && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DonationResponseDTO
            {
                Id = d.Id,
                CategoryId = d.CategoryId,
                Title = d.Title,
                Description = d.Description,
                FoodType = d.FoodType,
                QuantityDes = d.QuantityDescription,
                Quantity = d.Quantity ?? 0,
                Weight = d.WeightKg,
                ExpiryDate = d.ExpiryDate,
                PickupLocation = d.PickupLocation,
                CityId = d.CityId ?? d.Donor.CityId,
                ImageUrl = d.ImageUrl,
                Status = d.Status.ToString(),
                DonorId = d.DonorId,
                DonorName = d.Donor != null ? d.Donor.FullName : string.Empty,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ApiResult<List<DonationResponseDTO>>.Ok(
            donations,
            AppMessageCodes.DONATIONS_RETRIEVED);
    }
}