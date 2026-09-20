using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.Features.Geographic;

public class GeographicService : IGeographicService
{
    private readonly IEcoBridgeDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<GeographicService> _logger;

    public const string CountriesCacheKey = "Geo_Countries_Hierarchy";
    public const string ReviewReasonsCacheKey = "Meta_ReviewReasons";

    public GeographicService(
        IEcoBridgeDbContext dbContext,
        IMemoryCache memoryCache,
        ILogger<GeographicService> logger)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<ApiResult<List<CountryDTO>>> GetAllCountriesAsync(CancellationToken ct = default)
    {
        if (!_memoryCache.TryGetValue(CountriesCacheKey, out List<CountryDTO>? countries) || countries == null)
        {
            _logger.LogInformation("Geographic cache miss. Fetching from database...");
            countries = await FetchGeographicHierarchyFromDbAsync(ct);
            _memoryCache.Set(CountriesCacheKey, countries, new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            });
        }

        return ApiResult<List<CountryDTO>>.Ok(countries, AppMessageCodes.COUNTRIES_RETRIEVED);
    }

    public async Task<ApiResult<List<GovernorateDTO>>> GetAllGovernoratesAsync(CancellationToken ct = default)
    {
        var allCountriesApiResult = await GetAllCountriesAsync(ct);
        if (!allCountriesApiResult.IsSuccess || allCountriesApiResult.Data == null)
        {
            return ApiResult<List<GovernorateDTO>>.Fail( AppMessageCodes.GOVERNORATE_NOT_FOUND);
        }

        var governorates = allCountriesApiResult.Data
            .SelectMany(c => c.Governorates)
            .ToList();

        return ApiResult<List<GovernorateDTO>>.Ok(governorates, AppMessageCodes.GOVERNORATES_RETRIEVED);
    }

    public async Task<ApiResult<List<GovernorateDTO>>> GetGovernoratesByCountryIdAsync(int countryId, CancellationToken ct = default)
    {
        var allCountriesApiResult = await GetAllCountriesAsync(ct);
        if (!allCountriesApiResult.IsSuccess || allCountriesApiResult.Data == null)
        {
            return ApiResult<List<GovernorateDTO>>.Fail( AppMessageCodes.GOVERNORATE_NOT_FOUND);
        }

        var country = allCountriesApiResult.Data.FirstOrDefault(c => c.Id == countryId);
        if (country == null)
        {
            return ApiResult<List<GovernorateDTO>>.Fail( AppMessageCodes.COUNTRY_NOT_FOUND);
        }

        return ApiResult<List<GovernorateDTO>>.Ok(country.Governorates, AppMessageCodes.GOVERNORATES_RETRIEVED);
    }

    public async Task<ApiResult<List<CityDTO>>> GetCitiesByGovernorateIdAsync(int governorateId, CancellationToken ct = default)
    {
        var allCountriesApiResult = await GetAllCountriesAsync(ct);
        if (!allCountriesApiResult.IsSuccess || allCountriesApiResult.Data == null)
        {
            return ApiResult<List<CityDTO>>.Fail(AppMessageCodes.CITY_NOT_FOUND);
        }

        var governorate = allCountriesApiResult.Data
            .SelectMany(c => c.Governorates)
            .FirstOrDefault(g => g.Id == governorateId);

        if (governorate == null)
        {
            return ApiResult<List<CityDTO>>.Fail(AppMessageCodes.GOVERNORATE_NOT_FOUND);
        }

        return ApiResult<List<CityDTO>>.Ok(governorate.Cities, AppMessageCodes.CITIES_RETRIEVED);
    }

    public async Task<ApiResult<List<ReviewReasonDTO>>> GetReviewReasonsAsync(CancellationToken ct = default)
    {
        if (!_memoryCache.TryGetValue(ReviewReasonsCacheKey, out List<ReviewReasonDTO>? reasons) || reasons == null)
        {
            _logger.LogInformation("Review reasons cache miss. Fetching from database...");
            reasons = await _dbContext.ReviewReasons
                .AsNoTracking()
                .Select(r => new ReviewReasonDTO
                {
                    Id = r.Id,
                    Code = r.Code,
                    IsPositive = r.IsPositive,
                    FromRole = r.FromRole.ToString(),
                    ToRole = r.ToRole.ToString()
                })
                .ToListAsync(ct);

            _memoryCache.Set(ReviewReasonsCacheKey, reasons, new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            });
        }

        return ApiResult<List<ReviewReasonDTO>>.Ok(reasons, AppMessageCodes.REVIEW_REASONS_RETRIEVED);
    }

    public async Task WarmupCacheAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting Geographic & Meta Cache Warm-Up...");
        
        var hierarchy = await FetchGeographicHierarchyFromDbAsync(ct);
        _memoryCache.Set(CountriesCacheKey, hierarchy, new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.NeverRemove
        });

        var reasons = await _dbContext.ReviewReasons
            .AsNoTracking()
            .Select(r => new ReviewReasonDTO
            {
                Id = r.Id,
                Code = r.Code,
                IsPositive = r.IsPositive,
                FromRole = r.FromRole.ToString(),
                ToRole = r.ToRole.ToString()
            })
            .ToListAsync(ct);

        _memoryCache.Set(ReviewReasonsCacheKey, reasons, new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.NeverRemove
        });

        _logger.LogInformation("Cache Warm-Up completed. Loaded {CountriesCount} countries, {ReasonsCount} review reasons.", hierarchy.Count, reasons.Count);
    }

    private async Task<List<CountryDTO>> FetchGeographicHierarchyFromDbAsync(CancellationToken ct)
    {
        return await _dbContext.Countries
            .AsNoTracking()
            .Select(c => new CountryDTO
            {
                Id = c.Id,
                Code = c.Code,
                NameAr = c.NameAr,
                NameEn = c.NameEn,
                Governorates = c.Governorates.Select(g => new GovernorateDTO
                {
                    Id = g.Id,
                    CountryId = g.CountryId,
                    Code = g.Code,
                    NameAr = g.NameAr,
                    NameEn = g.NameEn,
                    Cities = g.Cities.Select(city => new CityDTO
                    {
                        Id = city.Id,
                        GovernorateId = city.GovernorateId,
                        Code = city.Code,
                        NameAr = city.NameAr,
                        NameEn = city.NameEn
                    }).ToList()
                }).ToList()
            })
            .ToListAsync(ct);
    }
}
