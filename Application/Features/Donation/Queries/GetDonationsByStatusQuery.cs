using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using MediatR;

namespace EcoBridge.Application.Features.Donation.Queries;

public record GetDonationsByStatusQuery(
    int? CityId,
    string? Status,
    int? CategoryId = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<ApiResult<PagedResult<DonationResponseDTO>>>;
