using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Models;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EcoBridge.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDTO>>
{
    private readonly IEcoBridgeDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JWTSettings _jwtSettings;
    private readonly PasswordHasher<Account> _passwordHasher = new();

    public LoginCommandHandler(
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

    public async Task<Result<AuthResponseDTO>> Handle(LoginCommand command, CancellationToken ct)
    {
        var request = command.Request;
        var email = request.Email.Trim().ToLowerInvariant();

        var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (account is null)
        {
            return Result<AuthResponseDTO>.FailResult(new AuthResponseDTO(), AppMessageCodes.INVALID_CREDENTIALS, ErrorType.Unauthorized);
        }

        if (!account.IsActive)
        {
            return Result<AuthResponseDTO>.FailResult(new AuthResponseDTO(), AppMessageCodes.ACCOUNT_DEACTIVATED, ErrorType.Forbidden);
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash ?? string.Empty, request.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return Result<AuthResponseDTO>.FailResult(new AuthResponseDTO(), AppMessageCodes.INVALID_CREDENTIALS, ErrorType.Unauthorized);
        }

        if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            account.ChangePassword(_passwordHasher.HashPassword(account, request.Password));
            await _dbContext.SaveChangesAsync(ct);
        }

        var accessToken = await _tokenService.GenerateAccessToken(account, ct);
        var refreshToken = await _refreshTokenService.GenerateAndSaveRefreshTokenAsync(account.Id, ct);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

        var response = new AuthResponseDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = accessTokenExpiresAt
        };

        return Result<AuthResponseDTO>.SuccessResult(response, AppMessageCodes.LOGIN_SUCCESS);
    }
}
