using EcoBridge.Application.Features.Lookups.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/lookups")]
[ApiController]
[EnableRateLimiting("GeneralTokenBucket")]
public class LookupsController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var result = await _sender.Send(new GetLookupCategoriesQuery(), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries(CancellationToken ct)
    {
        var result = await _sender.Send(new GetLookupCountriesQuery(), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("governorates/{countryId:int}")]
    public async Task<IActionResult> GetGovernorates(int countryId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetLookupGovernoratesQuery(countryId), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("cities/{governorateId:int}")]
    public async Task<IActionResult> GetCities(int governorateId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetLookupCitiesQuery(governorateId), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("review-reasons")]
    public async Task<IActionResult> GetReviewReasons(CancellationToken ct)
    {
        var result = await _sender.Send(new GetLookupReviewReasonsQuery(), ct);
        return ApiOk(result.Data!, result.Code);
    }
}