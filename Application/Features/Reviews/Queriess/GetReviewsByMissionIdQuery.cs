using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using MediatR;

namespace EcoBridge.Application.Features.Reviews.Queries;

public record GetReviewsByMissionIdQuery(
    Guid MissionId,
    int page,
    int pageSize
) : IRequest<ApiResult<PagedResult<ReviewResponseDTO>>>;