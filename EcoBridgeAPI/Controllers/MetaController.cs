using EcoBridge.Application.Features.Geographic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("GeneralTokenBucket")]
public class MetaController(IGeographicService geographicService) : BaseApiController
{
    private readonly IGeographicService _geographicService = geographicService;

    [HttpGet("governorates")]
    public async Task<IActionResult> GetGovernorates(CancellationToken ct)
    {
        var result = await _geographicService.GetAllGovernoratesAsync(ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("governorates/{id:int}/cities")]
    public async Task<IActionResult> GetCitiesByGovernorate(int id, CancellationToken ct)
    {
        var result = await _geographicService.GetCitiesByGovernorateIdAsync(id, ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("review-reasons")]
    public async Task<IActionResult> GetReviewReasons(CancellationToken ct)
    {
        var result = await _geographicService.GetReviewReasonsAsync(ct);
        return ApiOk(result.Data!, result.Code);
    }
}