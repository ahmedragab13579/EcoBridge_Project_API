using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Lookups.Queries;

public record GetLookupCitiesQuery(int GovernorateId) : IRequest<Result<List<LookupDTO>>>;

public class GetLookupCitiesQueryHandler(
    IEcoBridgeDbContext dbContext,
    IRedisServices redisServices
) : IRequestHandler<GetLookupCitiesQuery, Result<List<LookupDTO>>>
{
    public async Task<Result<List<LookupDTO>>> Handle(GetLookupCitiesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"Lookup:Cities:{request.GovernorateId}";
        var cached = await redisServices.GetDataAsync<List<LookupDTO>>(cacheKey);
        if (cached != null)
        {
            return Result<List<LookupDTO>>.SuccessResult(cached, AppMessageCodes.CITIES_RETRIEVED);
        }

        var cities = await dbContext.Cities
            .AsNoTracking()
            .Where(c => c.GovernorateId == request.GovernorateId)
            .OrderBy(c => c.NameEn)
            .Select(c => new LookupDTO(c.Id, c.NameEn, c.Code))
            .ToListAsync(cancellationToken);

        await redisServices.SetDataAsync(cacheKey, cities, TimeSpan.FromHours(24));

        return Result<List<LookupDTO>>.SuccessResult(cities, AppMessageCodes.CITIES_RETRIEVED);
    }
}
