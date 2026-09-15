using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Queries;

public record GetDonationByIdQuery(Guid DonationId) : IRequest<Result<DonationResponseDTO>>;

public class GetDonationByIdQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetDonationByIdQuery, Result<DonationResponseDTO>>
{
    public async Task<Result<DonationResponseDTO>> Handle(GetDonationByIdQuery request, CancellationToken cancellationToken)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (donation == null)
            return Result<DonationResponseDTO>.FailResult(new DonationResponseDTO(), AppMessageCodes.DONATION_NOT_FOUND, ErrorType.NotFound);

        return Result<DonationResponseDTO>.SuccessResult(donation, AppMessageCodes.DONATION_RETRIEVED);
    }
}
