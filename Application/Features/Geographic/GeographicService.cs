using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
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

    public async Task<Result<List<CountryDTO>>> GetAllCountriesAsync(CancellationToken ct = default)
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

        return Result<List<CountryDTO>>.SuccessResult(countries, AppMessageCodes.COUNTRIES_RETRIEVED);
    }

    public async Task<Result<List<GovernorateDTO>>> GetAllGovernoratesAsync(CancellationToken ct = default)
    {
        var allCountriesResult = await GetAllCountriesAsync(ct);
        if (!allCountriesResult.Success || allCountriesResult.Value == null)
        {
            return Result<List<GovernorateDTO>>.FailResult(new List<GovernorateDTO>(), AppMessageCodes.GOVERNORATE_NOT_FOUND, ErrorType.NotFound);
        }

        var governorates = allCountriesResult.Value
            .SelectMany(c => c.Governorates)
            .ToList();

        return Result<List<GovernorateDTO>>.SuccessResult(governorates, AppMessageCodes.GOVERNORATES_RETRIEVED);
    }

    public async Task<Result<List<GovernorateDTO>>> GetGovernoratesByCountryIdAsync(int countryId, CancellationToken ct = default)
    {
        var allCountriesResult = await GetAllCountriesAsync(ct);
        if (!allCountriesResult.Success || allCountriesResult.Value == null)
        {
            return Result<List<GovernorateDTO>>.FailResult(new List<GovernorateDTO>(), AppMessageCodes.GOVERNORATE_NOT_FOUND, ErrorType.NotFound);
        }

        var country = allCountriesResult.Value.FirstOrDefault(c => c.Id == countryId);
        if (country == null)
        {
            return Result<List<GovernorateDTO>>.FailResult(new List<GovernorateDTO>(), AppMessageCodes.COUNTRY_NOT_FOUND, ErrorType.NotFound);
        }

        return Result<List<GovernorateDTO>>.SuccessResult(country.Governorates, AppMessageCodes.GOVERNORATES_RETRIEVED);
    }

    public async Task<Result<List<CityDTO>>> GetCitiesByGovernorateIdAsync(int governorateId, CancellationToken ct = default)
    {
        var allCountriesResult = await GetAllCountriesAsync(ct);
        if (!allCountriesResult.Success || allCountriesResult.Value == null)
        {
            return Result<List<CityDTO>>.FailResult(new List<CityDTO>(), AppMessageCodes.CITY_NOT_FOUND, ErrorType.NotFound);
        }

        var governorate = allCountriesResult.Value
            .SelectMany(c => c.Governorates)
            .FirstOrDefault(g => g.Id == governorateId);

        if (governorate == null)
        {
            return Result<List<CityDTO>>.FailResult(new List<CityDTO>(), AppMessageCodes.GOVERNORATE_NOT_FOUND, ErrorType.NotFound);
        }

        return Result<List<CityDTO>>.SuccessResult(governorate.Cities, AppMessageCodes.CITIES_RETRIEVED);
    }

    public async Task<Result<List<ReviewReasonDTO>>> GetReviewReasonsAsync(CancellationToken ct = default)
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

        return Result<List<ReviewReasonDTO>>.SuccessResult(reasons, AppMessageCodes.REVIEW_REASONS_RETRIEVED);
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
