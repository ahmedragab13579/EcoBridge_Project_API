using EcoBridge.Application.Contracts;
using EcoBridge.Application.Extensions;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Queries;

public class GetDonationsQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetDonationsQuery, Result<PagedResult<DonationResponseDTO>>>
{
    public async Task<Result<PagedResult<DonationResponseDTO>>> Handle(GetDonationsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 || request.PageSize > 100 ? 20 : request.PageSize;

        var query = dbContext.Donations
            .AsNoTracking()
            .Where(d => !d.IsDeleted);

        if (request.CityId.HasValue)
        {
            query = query.Where(d => (d.CityId ?? d.Donor.Account.CityId) == request.CityId.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(d => d.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<DonationStatus>(request.Status, true, out var parsedStatus))
        {
            query = query.Where(d => d.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var rawItems = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new
            {
                d.Id,
                d.CategoryId,
                d.Title,
                d.Description,
                d.FoodType,
                d.Quantity,
                d.Weight,
                d.ExpiryDate,
                d.PickupLocation,
                CityId = d.CityId ?? d.Donor.Account.CityId,
                d.ImageUrl,
                Status = d.Status.ToString(),
                d.DonorId,
                DonorName = d.Donor.Account != null ? d.Donor.Account.FullName : string.Empty,
                d.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var items = rawItems.Select(d => new DonationResponseDTO
        {
            Id = d.Id,
            CategoryId = d.CategoryId,
            Title = d.Title,
            Description = d.Description,
            FoodType = d.FoodType,
            Quantity = d.Quantity,
            Weight = d.Weight,
            ExpiryDate = d.ExpiryDate,
            PickupLocation = d.PickupLocation,
            CityId = d.CityId,
            ImageUrl = d.ImageUrl.ToCloudinaryUrl(),
            Status = d.Status,
            DonorId = d.DonorId,
            DonorName = d.DonorName,
            CreatedAt = d.CreatedAt
        }).ToList();

        var pagedResult = new PagedResult<DonationResponseDTO>(items, totalCount, page, pageSize);
        return Result<PagedResult<DonationResponseDTO>>.SuccessResult(pagedResult, AppMessageCodes.SUCCESS);
    }
}
