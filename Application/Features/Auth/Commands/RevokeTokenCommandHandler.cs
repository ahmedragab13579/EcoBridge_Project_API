using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.Features.Auth;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Commands;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result<bool>>
{
    private readonly IRefreshTokenService _refreshTokenService;

    public RevokeTokenCommandHandler(IRefreshTokenService refreshTokenService)
    {
        _refreshTokenService = refreshTokenService;
    }

    public async Task<Result<bool>> Handle(RevokeTokenCommand command, CancellationToken ct)
    {
        if (command.RevokeAll && command.UserId.HasValue)
        {
            await _refreshTokenService.RevokeAllUserTokensAsync(command.UserId.Value, ct);
            return Result<bool>.SuccessResult(true, AppMessageCodes.LOGOUT_SUCCESS);
        }

        if (!string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            var revoked = await _refreshTokenService.RevokeRefreshTokenAsync(command.RefreshToken, ct);
            if (revoked)
                return Result<bool>.SuccessResult(true, AppMessageCodes.LOGOUT_SUCCESS);
        }

        return Result<bool>.FailResult(false, AppMessageCodes.INVALID_INPUT, ErrorType.Validation);
    }
}
