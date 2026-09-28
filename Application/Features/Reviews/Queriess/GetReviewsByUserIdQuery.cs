using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using EcoBridge.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoBridge.Application.Features.Reviews.Queriess
{
    public record GetReviewsByUserIdQuery(Guid userid, ReviewRole role = ReviewRole.Reviewee, int Page = 1,
    int PageSize = 10) : IRequest<ApiResult<PagedResult<ReviewResponseDTO>>>
    {
    }
}
