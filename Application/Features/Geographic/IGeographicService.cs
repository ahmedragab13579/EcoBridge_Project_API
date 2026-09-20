using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;

namespace EcoBridge.Application.Features.Geographic;

public interface IGeographicService
{
    Task<ApiResult<List<CountryDTO>>> GetAllCountriesAsync(CancellationToken ct = default);
    Task<ApiResult<List<GovernorateDTO>>> GetAllGovernoratesAsync(CancellationToken ct = default);
    Task<ApiResult<List<GovernorateDTO>>> GetGovernoratesByCountryIdAsync(int countryId, CancellationToken ct = default);
    Task<ApiResult<List<CityDTO>>> GetCitiesByGovernorateIdAsync(int governorateId, CancellationToken ct = default);
    Task<ApiResult<List<ReviewReasonDTO>>> GetReviewReasonsAsync(CancellationToken ct = default);
    Task WarmupCacheAsync(CancellationToken ct = default);
}
