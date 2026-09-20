using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Donation.Commands;

public record RequestDonationCommand(
    Guid DonationId,
    Guid CharityAccountId
) : IRequest<ApiResult<Guid>>;
