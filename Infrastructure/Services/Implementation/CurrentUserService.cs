using System.Security.Claims;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace EcoBridge.Infrastructure.Services.Implementation;

public class CurrentUserService : ICurrentUserServices
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var id = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value
                     ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            return Guid.TryParse(id, out var parsedId) ? parsedId : Guid.Empty;
        }
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public Result<GetMeResponseDto> GetMe()
    {
        var id = UserId;
        if (id == Guid.Empty)
        {
            return Result<GetMeResponseDto>.Fail(AppMessageCodes.INVALID_TOKEN, ErrorType.Unauthorized);
        }

        var email = User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.FindFirst("Email")?.Value ?? string.Empty;
        var role = User?.FindFirst(ClaimTypes.Role)?.Value ?? User?.FindFirst("Role")?.Value ?? string.Empty;
        var name = User?.FindFirst(ClaimTypes.Name)?.Value ?? User?.FindFirst("Name")?.Value ?? string.Empty;

        int? cityId = null;
        if (int.TryParse(User?.FindFirst("CityId")?.Value, out var parsedCityId))
        {
            cityId = parsedCityId;
        }

        var result = new GetMeResponseDto
        {
            UserId = id,
            Email = email,
            Role = role,
            Name = name,
            IsAuthenticated = this.IsAuthenticated,
            CityId = cityId
        };

        return Result<GetMeResponseDto>.SuccessResult(result, AppMessageCodes.ME_SUCCESS);
    }
}
