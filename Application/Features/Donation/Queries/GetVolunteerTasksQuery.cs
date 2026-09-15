using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Queries;

public record GetVolunteerTasksQuery(Guid VolunteerId) : IRequest<Result<List<DonationResponseDTO>>>;

public class GetVolunteerTasksQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetVolunteerTasksQuery, Result<List<DonationResponseDTO>>>
{
    public async Task<Result<List<DonationResponseDTO>>> Handle(GetVolunteerTasksQuery request, CancellationToken cancellationToken)
    {
        var donations = await dbContext.Donations
            .AsNoTracking()
            .Where(d => d.VolunteerId == request.VolunteerId && !d.IsDeleted)
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
