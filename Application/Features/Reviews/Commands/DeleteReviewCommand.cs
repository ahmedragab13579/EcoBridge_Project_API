using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Reviews.Commands;

public record DeleteReviewCommand(
    int ReviewId,
    Guid ReviewerId
) : IRequest<ApiResult<bool>>;