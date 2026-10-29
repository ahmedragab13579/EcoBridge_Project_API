using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.Users.Auth;
using EcoBridge.Application.DTO.Users.Auth.Users.Common;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Queries;

public record GetUserProfileQuery(Guid UserId) : IRequest<ApiResult<UserDTO>>, ICacheableQuery
{
    public string CacheKey => $"user_profile_{UserId}";
    public int CacheDurationMinutes => 15;
}
