using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Lookups.Queries;

public record GetLookupCategoriesQuery : IRequest<Result<List<LookupDTO>>>;

public class GetLookupCategoriesQueryHandler(
    IEcoBridgeDbContext dbContext,
    IRedisServices redisServices
) : IRequestHandler<GetLookupCategoriesQuery, Result<List<LookupDTO>>>
{
    private const string CacheKey = "Lookup:Categories";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(24);

    public async Task<Result<List<LookupDTO>>> Handle(GetLookupCategoriesQuery request, CancellationToken cancellationToken)
    {
        var cached = await redisServices.GetDataAsync<List<LookupDTO>>(CacheKey);
        if (cached != null)
        {
            return Result<List<LookupDTO>>.SuccessResult(cached, AppMessageCodes.CATEGORIES_RETRIEVED);
        }

        var categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.NameEn)
            .Select(c => new LookupDTO(c.Id, c.NameEn, null))
            .ToListAsync(cancellationToken);

        await redisServices.SetDataAsync(CacheKey, categories, CacheExpiry);

        return Result<List<LookupDTO>>.SuccessResult(categories, AppMessageCodes.CATEGORIES_RETRIEVED);
    }
}
