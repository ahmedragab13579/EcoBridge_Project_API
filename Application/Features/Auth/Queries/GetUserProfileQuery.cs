using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.Contracts;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Queries;

public record GetUserProfileQuery(Guid UserId) : IRequest<Result<UserProfileDTO>>, ICacheableQuery
{
    public string CacheKey => $"user_profile_{UserId}";
    public int CacheDurationMinutes => 15;
}
