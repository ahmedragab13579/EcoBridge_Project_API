using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Donation.Commands;

public record ApproveDonationRequestCommand(
    Guid DonationRequestId,
    Guid DonorAccountId
) : IRequest<Result<Guid>>;
