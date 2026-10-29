using EcoBridge.Application.DTO.Statics.Lookups;
using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Reviews.Queries;

public record GetReviewReasonsByReviewIdQuery(
    int ReviewId
) : IRequest<ApiResult<List<ReviewReasonLookupDTO>>>;