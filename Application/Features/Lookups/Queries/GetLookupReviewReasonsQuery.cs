using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Lookups.Queries;

public record GetLookupReviewReasonsQuery : IRequest<Result<List<ReviewReasonLookupDTO>>>;

public class GetLookupReviewReasonsQueryHandler(
    IEcoBridgeDbContext dbContext,
    IRedisServices redisServices
) : IRequestHandler<GetLookupReviewReasonsQuery, Result<List<ReviewReasonLookupDTO>>>
{
    private const string CacheKey = "Lookup:ReviewReasons";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(24);

    public async Task<Result<List<ReviewReasonLookupDTO>>> Handle(GetLookupReviewReasonsQuery request, CancellationToken cancellationToken)
    {
        var cached = await redisServices.GetDataAsync<List<ReviewReasonLookupDTO>>(CacheKey);
        if (cached != null)
        {
            return Result<List<ReviewReasonLookupDTO>>.SuccessResult(cached, AppMessageCodes.REVIEW_REASONS_RETRIEVED);
        }

        var reasons = await dbContext.ReviewReasons
            .AsNoTracking()
            .OrderBy(r => r.Code)
            .Select(r => new ReviewReasonLookupDTO
            {
                Id = r.Id,
                Code = r.Code,
                Name = r.Code,
                IsPositive = r.IsPositive,
                FromRole = r.FromRole.ToString(),
                ToRole = r.ToRole.ToString()
            })
            .ToListAsync(cancellationToken);

        await redisServices.SetDataAsync(CacheKey, reasons, CacheExpiry);

        return Result<List<ReviewReasonLookupDTO>>.SuccessResult(reasons, AppMessageCodes.REVIEW_REASONS_RETRIEVED);
    }
}
