using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.Result;
using EcoBridge.Application.Services;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Accounts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace EcoBridge.Application.Features.Auth.Handlers;

public class LoginCommandHandler(
    UserManager<Account> userManager,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService,
    IOptions<JWTSettings> jwtOptions
) : IRequestHandler<LoginCommand, ApiResult<AuthResponseDTO>>
{
    private readonly JWTSettings _jwtSettings = jwtOptions.Value;

    public async Task<ApiResult<AuthResponseDTO>> Handle(
        LoginCommand command,
        CancellationToken ct)
    {
        var email = command.Request.Email.Trim().ToLowerInvariant();

        var account = await userManager.FindByEmailAsync(email);

        if (account is null)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_CREDENTIALS);

        if (!account.IsActive)
            throw new DomainException.ForbiddenException(AppMessageCodes.ACCOUNT_DEACTIVATED);

        var isPasswordValid = await userManager.CheckPasswordAsync(account, command.Request.Password);

        if (!isPasswordValid)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_CREDENTIALS);

        var accessToken = await tokenService.GenerateAccessToken(account, ct);
        var refreshToken = await refreshTokenService.GenerateAndSaveRefreshTokenAsync(account.Id, ct);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

        var response = new AuthResponseDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = accessTokenExpiresAt
        };

        return ApiResult<AuthResponseDTO>.Ok(response, AppMessageCodes.LOGIN_SUCCESS);
    }
}