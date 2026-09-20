using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/[controller]")]
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
    public async Task<IActionResult> Refresh(
        CancellationToken ct)
    {
        var result = await _sender.Send(new RefreshTokenCommand(new RefreshTokenRequestDTO { AccessToken = AccessToken??"",RefreshToken=RefreshToken??""}), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost("revoke")]
    [Authorize]
    [EnableRateLimiting("GeneralTokenBucket")]
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

    [HttpGet("me")]
    [Authorize]
    [EnableRateLimiting("GeneralTokenBucket")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var result = await _sender.Send(new GetUserProfileQuery(CurrentUserId), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPut("me")]
    [Authorize]
    [EnableRateLimiting("GeneralTokenBucket")]
    public async Task<IActionResult> UpdateProfile(
        [FromForm] UpdateProfileDTO request,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new UpdateUserProfileCommand(CurrentUserId, request), ct);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("GeneralTokenBucket")]
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
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequestDTO request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new ForgotPasswordCommand(request), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("SecurityFixedWindow")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequestDTO request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new ResetPasswordCommand(request), ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost("verify-email")]
    [EnableRateLimiting("SecurityFixedWindow")]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailRequestDTO request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new VerifyEmailCommand(request), ct);
        return ApiOk(result.Data!, result.Code);
    }
}