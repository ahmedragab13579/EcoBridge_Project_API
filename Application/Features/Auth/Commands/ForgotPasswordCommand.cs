using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.Users.Auth.Users;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models.Accounts;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EcoBridge.Application.Features.Auth.Commands;

public record ForgotPasswordCommand(ForgotPasswordRequestDTO Request) : IRequest<ApiResult<bool>>;

public class ForgotPasswordCommandHandler(
    UserManager<Account> userManager,
    IEmailService emailService
) : IRequestHandler<ForgotPasswordCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(ForgotPasswordCommand command, CancellationToken ct)
    {
        var account = await userManager.FindByEmailAsync(command.Request.Email);
        if (account == null || !account.IsActive || account.IsDeleted)
        {
            // Return success to avoid email enumeration attacks
            return ApiResult<bool>.Ok(true, AppMessageCodes.PASSWORD_RESET_EMAIL_SENT);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(account);
        await emailService.SendPasswordResetEmailAsync(account.Email!, token, ct);

        return ApiResult<bool>.Ok(true, AppMessageCodes.PASSWORD_RESET_EMAIL_SENT);
    }
}
