using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Lookups.Queries;

public record GetLookupGovernoratesQuery(int CountryId) : IRequest<ApiResult<List<LookupDTO>>>;

public class GetLookupGovernoratesQueryHandler(
    IEcoBridgeDbContext dbContext,
    IRedisServices redisServices
) : IRequestHandler<GetLookupGovernoratesQuery, ApiResult<List<LookupDTO>>>
{
    public async Task<ApiResult<List<LookupDTO>>> Handle(GetLookupGovernoratesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"Lookup:Governorates:{request.CountryId}";
        var cached = await redisServices.GetDataAsync<List<LookupDTO>>(cacheKey);
        if (cached != null)
        {
            return ApiResult<List<LookupDTO>>.Ok(cached, AppMessageCodes.GOVERNORATES_RETRIEVED);
        }

        var governorates = await dbContext.Governorates
            .AsNoTracking()
            .Where(g => g.CountryId == request.CountryId)
            .OrderBy(g => g.NameEn)
            .Select(g => new LookupDTO(g.Id, g.NameEn, g.Code))
            .ToListAsync(cancellationToken);

        await redisServices.SetDataAsync(cacheKey, governorates, TimeSpan.FromHours(24));

        return ApiResult<List<LookupDTO>>.Ok(governorates, AppMessageCodes.GOVERNORATES_RETRIEVED);
    }
}
