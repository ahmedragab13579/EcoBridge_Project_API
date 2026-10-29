using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.Statics.Lookups;
using EcoBridge.Application.Features.Reviews.Queries;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Reviews.Handlers;

public class GetReviewReasonsByReviewIdQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetReviewReasonsByReviewIdQuery, ApiResult<List<ReviewReasonLookupDTO>>>
{
    public async Task<ApiResult<List<ReviewReasonLookupDTO>>> Handle(
        GetReviewReasonsByReviewIdQuery request,
        CancellationToken cancellationToken)
    {
        var review = await dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId && !r.IsDeleted, cancellationToken);

        if (review == null)
            throw new DomainException.NotFoundException(AppMessageCodes.REVIEW_NOT_FOUND);

        var fromRole = review.Reviewer.Role;
        var toRole = review.Reviewee.Role;

        var reasons = await dbContext.ReviewReasons
            .AsNoTracking()
            .Where(rr => rr.FromRole == fromRole && rr.ToRole == toRole)
            .Select(rr => new ReviewReasonLookupDTO
            {
                Id = rr.Id,
                Code = rr.Code,
                Name = rr.Code,        
                IsPositive = rr.IsPositive,
                FromRole = rr.FromRole.ToString(),
                ToRole = rr.ToRole.ToString()
            })
            .ToListAsync(cancellationToken);

        return ApiResult<List<ReviewReasonLookupDTO>>.Ok(
            reasons,
            AppMessageCodes.REVIEW_REASONS_RETRIEVED);
    }
}