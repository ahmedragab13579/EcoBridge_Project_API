using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
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
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request, CancellationToken ct)
    {
        var result = await _sender.Send(new RegisterCommand(request), ct);
        return HandleResult(result);
    }

    [HttpPost("login")]
    [EnableRateLimiting("SecurityFixedWindow")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request, CancellationToken ct)
    {
        var result = await _sender.Send(new LoginCommand(request), ct);
        return HandleResult(result);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("SecurityFixedWindow")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDTO request, CancellationToken ct)
    {
        var result = await _sender.Send(new RefreshTokenCommand(request), ct);
        return HandleResult(result);
    }

    [HttpPost("revoke")]
    [Authorize]
    [EnableRateLimiting("GeneralTokenBucket")]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequestDTO? request, [FromQuery] bool revokeAll = false, CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        var result = await _sender.Send(new RevokeTokenCommand(request?.RefreshToken, userId != Guid.Empty ? userId : null, revokeAll), ct);
        return HandleResult(result);
    }

    [HttpGet("me")]
    [Authorize]
    [EnableRateLimiting("GeneralTokenBucket")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new GetUserProfileQuery(userId), ct);
        return HandleResult(result);
    }

    [HttpPut("me")]
    [Authorize]
    [EnableRateLimiting("GeneralTokenBucket")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDTO request, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new UpdateUserProfileCommand(userId, request), ct);
        return HandleResult(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("GeneralTokenBucket")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO request, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new ChangeUserPasswordCommand(userId, request), ct);
        return HandleResult(result);
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("SecurityFixedWindow")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request, CancellationToken ct)
    {
        var result = await _sender.Send(new ForgotPasswordCommand(request), ct);
        return HandleResult(result);
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("SecurityFixedWindow")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request, CancellationToken ct)
    {
        var result = await _sender.Send(new ResetPasswordCommand(request), ct);
        return HandleResult(result);
    }

    [HttpPost("verify-email")]
    [EnableRateLimiting("SecurityFixedWindow")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDTO request, CancellationToken ct)
    {
        var result = await _sender.Send(new VerifyEmailCommand(request), ct);
        return HandleResult(result);
    }
}
