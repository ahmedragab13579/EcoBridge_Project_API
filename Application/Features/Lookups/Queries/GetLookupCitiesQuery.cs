using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.Statics.Lookups;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Lookups.Queries;

public record GetLookupCitiesQuery(int GovernorateId) : IRequest<ApiResult<List<LookupDTO>>>;

public class GetLookupCitiesQueryHandler(
    IEcoBridgeDbContext dbContext,
    IRedisServices redisServices
) : IRequestHandler<GetLookupCitiesQuery, ApiResult<List<LookupDTO>>>
{
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(24);

    public async Task<ApiResult<List<LookupDTO>>> Handle(
        GetLookupCitiesQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"Lookup:Cities:{request.GovernorateId}";

        var cached = await redisServices.GetDataAsync<List<LookupDTO>>(cacheKey);
        if (cached != null)
        {
            return ApiResult<List<LookupDTO>>.Ok(
                cached,
                AppMessageCodes.CITIES_RETRIEVED);
        }

        var cities = await dbContext.Cities
            .AsNoTracking()
            .Where(c => c.GovernorateId == request.GovernorateId)
            .OrderBy(c => c.NameEn)
            .Select(c => new LookupDTO(c.Id, c.NameEn, c.Code))
            .ToListAsync(cancellationToken);

        await redisServices.SetDataAsync(cacheKey, cities, CacheExpiry);

        return ApiResult<List<LookupDTO>>.Ok(
            cities,
            AppMessageCodes.CITIES_RETRIEVED);
    }
}