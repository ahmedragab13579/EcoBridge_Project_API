using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Donation.Commands;

public record UnClaimCommand(
    Guid DonationId,
    Guid CharityId
) : IRequest<ApiResult<bool>>;