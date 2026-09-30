using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Reviews.Queries;

public record GetReviewsByMissionIdQuery(
    Guid MissionId
) : IRequest<ApiResult<List<ReviewResponseDTO>>>;