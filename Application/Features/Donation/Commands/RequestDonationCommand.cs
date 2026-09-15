using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Donation.Commands;

public record RequestDonationCommand(
    Guid DonationId,
    Guid CharityAccountId
) : IRequest<Result<Guid>>;
