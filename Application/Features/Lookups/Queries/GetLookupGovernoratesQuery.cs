using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Lookups.Queries;

public record GetLookupGovernoratesQuery(int CountryId) : IRequest<Result<List<LookupDTO>>>;

public class GetLookupGovernoratesQueryHandler(
    IEcoBridgeDbContext dbContext,
    IRedisServices redisServices
) : IRequestHandler<GetLookupGovernoratesQuery, Result<List<LookupDTO>>>
{
    public async Task<Result<List<LookupDTO>>> Handle(GetLookupGovernoratesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"Lookup:Governorates:{request.CountryId}";
        var cached = await redisServices.GetDataAsync<List<LookupDTO>>(cacheKey);
        if (cached != null)
        {
            return Result<List<LookupDTO>>.SuccessResult(cached, AppMessageCodes.GOVERNORATES_RETRIEVED);
        }

        var governorates = await dbContext.Governorates
            .AsNoTracking()
            .Where(g => g.CountryId == request.CountryId)
            .OrderBy(g => g.NameEn)
            .Select(g => new LookupDTO(g.Id, g.NameEn, g.Code))
            .ToListAsync(cancellationToken);

        await redisServices.SetDataAsync(cacheKey, governorates, TimeSpan.FromHours(24));

        return Result<List<LookupDTO>>.SuccessResult(governorates, AppMessageCodes.GOVERNORATES_RETRIEVED);
    }
}
