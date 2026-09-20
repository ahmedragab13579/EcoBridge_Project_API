using EcoBridge.Application.Contracts;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Handlers;

public class RevokeTokenCommandHandler(IRefreshTokenService refreshTokenService)
    : IRequestHandler<RevokeTokenCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(
        RevokeTokenCommand command,
        CancellationToken ct)
    {
        // 1. Revoke all tokens for a user (requires auth)
        if (command.RevokeAll)
        {
            if (!command.UserId.HasValue || command.UserId.Value == Guid.Empty)
                throw new DomainException.UnauthorizedException(AppMessageCodes.UNAUTHORIZED);

            await refreshTokenService.RevokeAllUserTokensAsync(command.UserId.Value, ct);
            return ApiResult<bool>.Ok(true, AppMessageCodes.LOGOUT_SUCCESS);
        }

        // 2. Revoke a specific refresh token (allows anonymous revoke)
        if (!string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            var revoked = await refreshTokenService
                .RevokeRefreshTokenAsync(command.RefreshToken, ct);

            if (revoked)
                return ApiResult<bool>.Ok(true, AppMessageCodes.LOGOUT_SUCCESS);
        }

        // 3. Nothing to revoke
        throw new DomainException.ValidationException(AppMessageCodes.INVALID_INPUT);
    }
}