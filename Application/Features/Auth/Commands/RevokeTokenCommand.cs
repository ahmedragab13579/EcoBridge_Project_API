using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Commands;

public record RevokeTokenCommand(string? RefreshToken, Guid? UserId, bool RevokeAll = false) : IRequest<ApiResult<bool>>;
