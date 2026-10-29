using EcoBridge.API.Filters;
using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.Users.Auth.Users;
using EcoBridge.Application.DTO.Users.Auth.Users.Common;
using EcoBridge.Application.DTO.Users.Charity;
using EcoBridge.Application.DTO.Users.Volunteer;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.Features.Auth.Commands.Profiles;
using EcoBridge.Application.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpPost("register")]
    [EnableRateLimiting("SecurityFixedWindow")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDTO request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new RegisterCommand(request), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost("login")]
    [EnableRateLimiting("SecurityFixedWindow")]

    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDTO request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new LoginCommand(request), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("SecurityFixedWindow")]
    [ValidateCsrfToken]

    public async Task<IActionResult> Refresh(
        CancellationToken ct)
    {
        var result = await _sender.Send(new RefreshTokenCommand(new RefreshTokenRequestDTO { AccessToken = AccessToken??"",RefreshToken=RefreshToken??""}), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost("revoke")]
    [Authorize]
    [EnableRateLimiting("GeneralTokenBucket")]
    [ValidateCsrfToken]

    public async Task<IActionResult> Revoke(
        [FromQuery] bool revokeAll = false,
        CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        Guid? nullableUserId = userId != Guid.Empty ? userId : null;

        var result = await _sender.Send(
            new RevokeTokenCommand( RefreshToken, nullableUserId, revokeAll), ct);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("profile")]
    [Authorize]
    [EnableRateLimiting("GeneralTokenBucket")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var result = await _sender.Send(new GetUserProfileQuery(CurrentUserId), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPut("volunteer/profile")]
    [Authorize(Roles = "Volunteer")]
    [EnableRateLimiting("GeneralTokenBucket")]
    [ValidateCsrfToken]

    public async Task<IActionResult> UpdateProfile(
        [FromForm] UpdateVolunteerProfileDTO request,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new UpdateVolunteerProfileCommand(CurrentUserId, request), ct);

        return ApiOk(result.Data!, result.Code);
    }
    [HttpPut("charity/profile")]
    [Authorize(Roles = "Charity")]
    [EnableRateLimiting("GeneralTokenBucket")]
    [ValidateCsrfToken]

    public async Task<IActionResult> UpdateProfile(
        [FromForm] UpdateCharityProfileDTO request,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new UpdateCharityProfileCommand(CurrentUserId, request), ct);

        return ApiOk(result.Data!, result.Code);
    }
    [HttpPut("profile")]
    [Authorize(Roles = "Donor,Admin")]
    [EnableRateLimiting("GeneralTokenBucket")]
    [ValidateCsrfToken]

    public async Task<IActionResult> UpdateProfile(
        [FromForm] MiniUpdateDto request,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new UpdateDonorAdminProfileCommand(CurrentUserId, request), ct);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("GeneralTokenBucket")]
    [ValidateCsrfToken]

    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDTO request,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new ChangeUserPasswordCommand(CurrentUserId, request), ct);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("SecurityFixedWindow")]
    [ValidateCsrfToken]

    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequestDTO request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new ForgotPasswordCommand(request), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("SecurityFixedWindow")]
    [ValidateCsrfToken]

    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequestDTO request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new ResetPasswordCommand(request), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost("verify-email")]
    [EnableRateLimiting("SecurityFixedWindow")]
    [ValidateCsrfToken]

    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailRequestDTO request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new VerifyEmailCommand(request), ct);
        return ApiOk(result.Data!, result.Code);
    }
}