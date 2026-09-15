using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Reviews.Commands;

public record CreateReviewCommand(
    Guid ReviewerId,
    CreateReviewDTO Dto
) : IRequest<Result<ReviewResponseDTO>>;
