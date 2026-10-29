using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Queries;

public record GetMyDonationsByStatusQuery(Guid CharityId,int page,int pageSize) : IRequest<ApiResult<PagedResult<DonationResponseDTO>>>;

public class GetMyAcceptedDonationsQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetMyDonationsByStatusQuery, ApiResult<PagedResult<DonationResponseDTO>>>
{
    public async Task<ApiResult<PagedResult<DonationResponseDTO>>> Handle(
        GetMyDonationsByStatusQuery request,
        CancellationToken cancellationToken)
    {
        var donations = await dbContext.Donations
            .AsNoTracking()
            .Where(d => d.CharityId == request.CharityId)
            .OrderByDescending(d => d.CreatedAt).Skip((request.page - 1) * request.pageSize).Take(request.pageSize)

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
                CityId = d.CityId ,
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
                PageNumber = request.page,
                TotalCount = donations.Count,
                Page = request.page
            },
            AppMessageCodes.DONATIONS_RETRIEVED);
    }
}