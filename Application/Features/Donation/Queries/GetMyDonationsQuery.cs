using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Queries;

public record GetMyDonationsQuery(Guid AccountId) : IRequest<Result<List<DonationResponseDTO>>>;

public class GetMyDonationsQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetMyDonationsQuery, Result<List<DonationResponseDTO>>>
{
    public async Task<Result<List<DonationResponseDTO>>> Handle(GetMyDonationsQuery request, CancellationToken cancellationToken)
    {
        var donations = await dbContext.Donations
            .AsNoTracking()
            .Where(d => d.DonorId == request.AccountId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DonationResponseDTO
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
                CityId = d.CityId ?? d.Donor.Account.CityId,
                ImageUrl = d.ImageUrl,
                Status = d.Status.ToString(),
                DonorId = d.DonorId,
                DonorName = d.Donor.Account != null ? d.Donor.Account.FullName : string.Empty,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<DonationResponseDTO>>.SuccessResult(donations, AppMessageCodes.DONATIONS_RETRIEVED);
    }
}
