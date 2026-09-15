using EcoBridge.Application.Features.Geographic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("GeneralTokenBucket")]
public class GeographicController(IGeographicService geographicService) : BaseApiController
{
    private readonly IGeographicService _geographicService = geographicService;

    [HttpGet("countries")]
    public async Task<IActionResult> GetAllCountries(CancellationToken ct)
    {
        var result = await _geographicService.GetAllCountriesAsync(ct);
        return HandleResult(result);
    }

    [HttpGet("countries/{countryId:int}/governorates")]
    public async Task<IActionResult> GetGovernorates(int countryId, CancellationToken ct)
    {
        var result = await _geographicService.GetGovernoratesByCountryIdAsync(countryId, ct);
        return HandleResult(result);
    }

    [HttpGet("governorates/{governorateId:int}/cities")]
    public async Task<IActionResult> GetCities(int governorateId, CancellationToken ct)
    {
        var result = await _geographicService.GetCitiesByGovernorateIdAsync(governorateId, ct);
        return HandleResult(result);
    }
}
