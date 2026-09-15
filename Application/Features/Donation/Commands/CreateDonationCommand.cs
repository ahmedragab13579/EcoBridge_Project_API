using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Donation.Commands;

public record CreateDonationCommand(
    Guid AccountId,
    CreateDonationDTO Dto
) : IRequest<Result<DonationResponseDTO>>;
