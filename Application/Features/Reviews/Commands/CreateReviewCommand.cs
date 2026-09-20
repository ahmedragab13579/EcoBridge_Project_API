using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Reviews.Commands;

public record CreateReviewCommand(
    Guid ReviewerId,
    CreateReviewDTO Dto
) : IRequest<ApiResult<ReviewResponseDTO>>;
