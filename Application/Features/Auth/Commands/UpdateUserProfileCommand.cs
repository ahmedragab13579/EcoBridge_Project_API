using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Commands;

public record UpdateUserProfileCommand(Guid UserId, UpdateProfileDTO Request) : IRequest<ApiResult<UserProfileDTO>>;
