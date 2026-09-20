using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Lookups.Queries;

public record GetLookupCategoriesQuery : IRequest<ApiResult<List<LookupDTO>>>;

public class GetLookupCategoriesQueryHandler(
    IEcoBridgeDbContext dbContext,
    IRedisServices redisServices
) : IRequestHandler<GetLookupCategoriesQuery, ApiResult<List<LookupDTO>>>
{
    private const string CacheKey = "Lookup:Categories";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(24);

    public async Task<ApiResult<List<LookupDTO>>> Handle(GetLookupCategoriesQuery request, CancellationToken cancellationToken)
    {
        var cached = await redisServices.GetDataAsync<List<LookupDTO>>(CacheKey);
        if (cached != null)
        {
            return ApiResult<List<LookupDTO>>.Ok(cached, AppMessageCodes.CATEGORIES_RETRIEVED);
        }

        var categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.NameEn)
            .Select(c => new LookupDTO(c.Id, c.NameEn, null))
            .ToListAsync(cancellationToken);

        await redisServices.SetDataAsync(CacheKey, categories, CacheExpiry);

        return ApiResult<List<LookupDTO>>.Ok(categories, AppMessageCodes.CATEGORIES_RETRIEVED);
    }
}
