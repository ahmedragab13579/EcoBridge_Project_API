using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.Users.Auth.Users;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.Result;
using EcoBridge.Application.Services;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Models.Accounts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EcoBridge.Application.Features.Auth.Handlers;

public class RegisterCommandHandler(
    UserManager<Account> userManager,
    RoleManager<ApplicationRole> roleManager,
    ITokenService tokenService,
    ICookieService cookieService,
    IRefreshTokenService refreshTokenService,
    IOptions<JWTSettings> jwtOptions
) : IRequestHandler<RegisterCommand, ApiResult<AuthResponseDTO>>
{
    private readonly JWTSettings _jwtSettings = jwtOptions.Value;

    public async Task<ApiResult<AuthResponseDTO>> Handle(
        RegisterCommand command,
        CancellationToken ct)
    {
        var request = command.Request;

        // 1. Prevent Admin registration from this endpoint
        if (request.Role == UserRole.Admin.ToString())
            throw new DomainException.ForbiddenException(AppMessageCodes.NOT_SUPPORT_REGEST_ADMIN);

        // 2. Parse Role
        if (!Enum.TryParse<UserRole>(request.Role, out var parsedRole)
            || !Enum.IsDefined(parsedRole))
        {
            throw new DomainException.ValidationException(AppMessageCodes.VALIDATION_ERROR);
        }
        if (!Enum.TryParse<UserType>(request.Type, out var parsedType)
            || !Enum.IsDefined(parsedType))
        {
            throw new DomainException.ValidationException(AppMessageCodes.VALIDATION_ERROR);
        }

        var email = request.Email.Trim().ToLowerInvariant();

        // 3. Fast-path check
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
            throw new DomainException.ConflictException(AppMessageCodes.EMAIL_ALREADY_EXISTS);

        // 4. Create Entity via Domain Factory Method
        var account = Account.Create(
            request.FullName.Trim(),
            email,
            request.Phone.Trim(),
            "hash", // UserManager will override this upon creation
            parsedRole,
            parsedType,
            request.CityId,
            request.AddressDetails,request.OrganizationName,request.RegistrationNumber,request.VehicleType,request.VehicleDetails
            
        );

        // 5. Create user via Identity (Handles password hashing & security stamp)
        IdentityResult result;
     
        result = await userManager.CreateAsync(account, request.Password);
        if (!result.Succeeded)
            throw new DomainException.ValidationException(AppMessageCodes.VALIDATION_ERROR);
        var roleResult = await roleManager.FindByNameAsync(parsedRole.ToString());
        if (roleResult is null)
           await roleManager.CreateAsync(new ApplicationRole(parsedRole.ToString()));
        var Result = await userManager.AddToRoleAsync(account, parsedRole.ToString());
        if (!Result.Succeeded)
            throw new DomainException.ValidationException(AppMessageCodes.VALIDATION_ERROR);
        // 6. Issue Tokens
        var accessToken = await tokenService.GenerateAccessToken(account, ct);
        var refreshToken = await refreshTokenService.GenerateAndSaveRefreshTokenAsync(account.Id, ct);

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDurationInDays);

        // 3. إنشاء CSRF Token عشوائي وفريد
        var csrfToken = Guid.NewGuid().ToString("N");

        // 4. حفظ الكوكيز بالتاريخ والمكونات المظبوطة
        cookieService.SetAuthCookies(refreshToken, csrfToken, refreshTokenExpiresAt);

        // 5. إرجاع الـ AccessToken في الـ Response
        var response = new AuthResponseDTO
        {
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = accessTokenExpiresAt
        };

        return ApiResult<AuthResponseDTO>.Ok(response, AppMessageCodes.REGISTER_SUCCESS);
    }
}