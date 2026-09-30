using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Queries;

public record GetAvailableDonationsQuery(int page,int pageSize) : IRequest<ApiResult<PagedResult<DonationResponseDTO>>>;

public class GetAvailableTasksQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetAvailableDonationsQuery, ApiResult<PagedResult<DonationResponseDTO>>>
{
    public async Task<ApiResult<PagedResult<DonationResponseDTO>>> Handle(
        GetAvailableDonationsQuery request,
        CancellationToken cancellationToken)
    {
        var donations = await dbContext.Donations
            .AsNoTracking()
            .Where(d => d.Status == DonationStatus.Available
                     && d.VolunteerId == null
                     && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Skip((request.page-1)*request.pageSize).Take(request.pageSize)
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
            .ToListAsync(cancellationToken);

        return ApiResult<PagedResult<DonationResponseDTO>>.Ok(
            new PagedResult<DonationResponseDTO>
            {
                Items = donations,
                PageSize = request.pageSize,
                PageNumber= request.page,
                Page = request.page,
                TotalCount = donations.Count,
            }
            ,
            AppMessageCodes.DONATIONS_RETRIEVED);
    }
}