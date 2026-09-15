using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
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

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDTO>>
{
    private readonly IEcoBridgeDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JWTSettings _jwtSettings;
    private readonly PasswordHasher<Account> _passwordHasher = new();

    public RegisterCommandHandler(
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

    public async Task<Result<AuthResponseDTO>> Handle(RegisterCommand command, CancellationToken ct)
    {
        var request = command.Request;

        if (request.Role == UserRole.Admin.ToString())
        {
            return Result<AuthResponseDTO>.FailResult(
                new AuthResponseDTO(),
                AppMessageCodes.NOT_SUPPORT_REGEST_ADMIN,
                ErrorType.Validation);
        }
        if (!Enum.IsDefined(typeof(UserRole), request.Role))
        {
            return Result<AuthResponseDTO>.FailResult(
                new AuthResponseDTO(),
                AppMessageCodes.VALIDATION_ERROR,
                ErrorType.Validation);
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await _dbContext.Accounts.AnyAsync(x => x.Email == email, ct);
        if (exists)
        {
            return Result<AuthResponseDTO>.FailResult(new AuthResponseDTO(), AppMessageCodes.EMAIL_ALREADY_EXISTS, ErrorType.Conflict);
        }

        var passwordHash = _passwordHasher.HashPassword(null!, request.Password);
        var account = Account.Create(
            request.FullName.Trim(),
            email,
            request.Phone.Trim(),
            passwordHash,
            Enum.Parse<UserRole>(request.Role,false) ,
            UserType.Individual,
            request.CityId,
            request.AddressDetails
        );

        await _dbContext.Accounts.AddAsync(account, ct);
        await _dbContext.SaveChangesAsync(ct);

        var accessToken = await _tokenService.GenerateAccessToken(account, ct);
        var refreshToken = await _refreshTokenService.GenerateAndSaveRefreshTokenAsync(account.Id, ct);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

        var response = new AuthResponseDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = accessTokenExpiresAt
        };

        return Result<AuthResponseDTO>.SuccessResult(response, AppMessageCodes.REGISTER_SUCCESS);
    }
}
