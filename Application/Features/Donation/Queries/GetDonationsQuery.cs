using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;

namespace EcoBridge.Application.Features.Donation.Queries;

public record GetDonationsQuery(
    int? CityId,
    string? Status,
    int? CategoryId = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<DonationResponseDTO>>>;
