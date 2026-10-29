using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Reviews.Queries;

public record GetReviewByIdQuery(int ReviewId) : IRequest<ApiResult<ReviewResponseDTO>>;