using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Donation.Queries;

public record GetFinishedMissionByDonationIdQuery(
    Guid DonationId,
    Guid CallerId
) : IRequest<ApiResult<DonationMissionDetailDTO>>;