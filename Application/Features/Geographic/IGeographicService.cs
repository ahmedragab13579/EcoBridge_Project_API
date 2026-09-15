using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;

namespace EcoBridge.Application.Features.Geographic;

public interface IGeographicService
{
    Task<Result<List<CountryDTO>>> GetAllCountriesAsync(CancellationToken ct = default);
    Task<Result<List<GovernorateDTO>>> GetAllGovernoratesAsync(CancellationToken ct = default);
    Task<Result<List<GovernorateDTO>>> GetGovernoratesByCountryIdAsync(int countryId, CancellationToken ct = default);
    Task<Result<List<CityDTO>>> GetCitiesByGovernorateIdAsync(int governorateId, CancellationToken ct = default);
    Task<Result<List<ReviewReasonDTO>>> GetReviewReasonsAsync(CancellationToken ct = default);
    Task WarmupCacheAsync(CancellationToken ct = default);
}
