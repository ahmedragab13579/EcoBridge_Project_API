using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Lookups.Queries;

public record GetLookupCountriesQuery : IRequest<Result<List<LookupDTO>>>;

public class GetLookupCountriesQueryHandler(
    IEcoBridgeDbContext dbContext,
    IRedisServices redisServices
) : IRequestHandler<GetLookupCountriesQuery, Result<List<LookupDTO>>>
{
    private const string CacheKey = "Lookup:Countries";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(24);

    public async Task<Result<List<LookupDTO>>> Handle(GetLookupCountriesQuery request, CancellationToken cancellationToken)
    {
        var cached = await redisServices.GetDataAsync<List<LookupDTO>>(CacheKey);
        if (cached != null)
        {
            return Result<List<LookupDTO>>.SuccessResult(cached, AppMessageCodes.COUNTRIES_RETRIEVED);
        }

        var countries = await dbContext.Countries
            .AsNoTracking()
            .OrderBy(c => c.NameEn)
            .Select(c => new LookupDTO(c.Id, c.NameEn, c.Code))
            .ToListAsync(cancellationToken);

        await redisServices.SetDataAsync(CacheKey, countries, CacheExpiry);

        return Result<List<LookupDTO>>.SuccessResult(countries, AppMessageCodes.COUNTRIES_RETRIEVED);
    }
}
