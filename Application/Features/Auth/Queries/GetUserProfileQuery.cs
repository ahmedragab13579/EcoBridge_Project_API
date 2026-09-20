using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Queries;

public record GetUserProfileQuery(Guid UserId) : IRequest<ApiResult<UserProfileDTO>>, ICacheableQuery
{
    public string CacheKey => $"user_profile_{UserId}";
    public int CacheDurationMinutes => 15;
}
