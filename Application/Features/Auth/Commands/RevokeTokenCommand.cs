using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Commands;

public record RevokeTokenCommand(string? RefreshToken, Guid? UserId, bool RevokeAll = false) : IRequest<Result<bool>>;
