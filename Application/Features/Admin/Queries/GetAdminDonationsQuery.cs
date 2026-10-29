using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Admin.Queries;

public record GetAdminDonationsQuery(int Page, int PageSize)
    : IRequest<ApiResult<PagedResult<DonationResponseDTO>>>;

public class GetAdminDonationsQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetAdminDonationsQuery, ApiResult<PagedResult<DonationResponseDTO>>>
{
    public async Task<ApiResult<PagedResult<DonationResponseDTO>>> Handle(
        GetAdminDonationsQuery request,
        CancellationToken cancellationToken)
    {

        var baseQuery = dbContext.Donations
            .AsNoTracking()
            .Where(d => !d.IsDeleted);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(d => d.CreatedAt)
            .Skip((request.Page  - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DonationResponseDTO
            {
                Id = d.Id,
                CategoryId = d.CategoryId,
                Title = d.Title,
                Description = d.Description,
                FoodType = d.FoodType,
                QuantityDes=d.QuantityDescription,
                Weight = d.WeightKg,
                ExpiryDate = d.ExpiryDate,
                PickupLocation = d.PickupLocation,
                CityId = d.CityId != 0 ? d.CityId : d.Donor.CityId,
                ImageUrl = d.ImageUrl,
                Status = d.Status.ToString(),
                DonorId = d.DonorId,
                DonorName = d.Donor != null ? d.Donor.FullName : string.Empty,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var paged = new PagedResult<DonationResponseDTO>(items, totalCount, request.Page, request.PageSize);

        return ApiResult<PagedResult<DonationResponseDTO>>.Ok(
            paged,
            AppMessageCodes.DONATIONS_RETRIEVED);
    }
}