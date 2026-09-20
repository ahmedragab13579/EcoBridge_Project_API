using System.Security.Claims;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace EcoBridge.Infrastructure.Services.Implementation;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid UserId
    {
        get
        {
            var id = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User?.FindFirst("UserId")?.Value
                  ?? User?.FindFirst("sub")?.Value;

            return Guid.TryParse(id, out var parsedId) ? parsedId : Guid.Empty;
        }
    }

    public GetMeResponseDto GetMe()
    {
        var userId = UserId;
        if (userId == Guid.Empty)
            throw new DomainException.UnauthorizedException(AppMessageCodes.INVALID_TOKEN);

        var email = User?.FindFirst(ClaimTypes.Email)?.Value
                 ?? User?.FindFirst("Email")?.Value
                 ?? string.Empty;

        var role = User?.FindFirst(ClaimTypes.Role)?.Value
                ?? User?.FindFirst("Role")?.Value
                ?? string.Empty;

        var name = User?.FindFirst(ClaimTypes.Name)?.Value
                ?? User?.FindFirst("Name")?.Value
                ?? string.Empty;

        int? cityId = null;
        if (int.TryParse(User?.FindFirst("CityId")?.Value, out var parsedCityId))
            cityId = parsedCityId;

        return new GetMeResponseDto
        {
            UserId = userId,
            Email = email,
            Role = role,
            Name = name,
            IsAuthenticated = IsAuthenticated,
            CityId = cityId
        };
    }
}