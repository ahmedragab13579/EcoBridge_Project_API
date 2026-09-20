using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.Result;
using EcoBridge.Application.Services;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EcoBridge.Application.Features.Auth.Handlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResult<AuthResponseDTO>>
{
    private readonly IEcoBridgeDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JWTSettings _jwtSettings;

    public RefreshTokenCommandHandler(
        IEcoBridgeDbContext dbContext,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        IOptions<JWTSettings> jwtOptions)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<ApiResult<AuthResponseDTO>> Handle(
        RefreshTokenCommand command,
        CancellationToken ct)
    {
        var request = command.Request;

        // 1. Validate refresh token in Redis
        var userIdFromRedis = await _refreshTokenService
            .ValidateRefreshTokenAsync(request.RefreshToken, ct);

        if (userIdFromRedis == null)
            throw new DomainException.ValidationException(AppMessageCodes.TOKEN_EXPIRED);

        // 2. Validate access token claims
        ClaimsPrincipal principal;
        try
        {
            principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        }
        catch (SecurityTokenException)
        {
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_TOKEN);
        }

        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                          ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var tokenUserId) || tokenUserId != userIdFromRedis.Value)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_TOKEN);

        // 3. Verify user account status in database
        var account = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == tokenUserId, ct);

        if (account is null || !account.IsActive || account.IsDeleted)
        {
            // Revoke tokens if account is deactivated, deleted, or missing
            await _refreshTokenService.RevokeAllUserTokensAsync(tokenUserId, ct);
            throw new DomainException.ForbiddenException(AppMessageCodes.ACCOUNT_DEACTIVATED);
        }

        // 4. Rotate refresh token in Redis (Revoke old + generate new)
        await _refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken, ct);
        var newRefreshToken = await _refreshTokenService.GenerateAndSaveRefreshTokenAsync(account.Id, ct);
        var newAccessToken = await _tokenService.GenerateAccessToken(account, ct);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

        var response = new AuthResponseDTO
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAtUtc = accessTokenExpiresAt
        };

        return ApiResult<AuthResponseDTO>.Ok(response, AppMessageCodes.LOGIN_SUCCESS);
    }
}