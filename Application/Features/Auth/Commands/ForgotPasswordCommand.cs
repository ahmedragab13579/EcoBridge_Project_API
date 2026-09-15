using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EcoBridge.Application.Features.Auth.Commands;

public record ForgotPasswordCommand(ForgotPasswordRequestDTO Request) : IRequest<Result<bool>>;

public class ForgotPasswordCommandHandler(
    UserManager<Account> userManager,
    IEmailService emailService
) : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ForgotPasswordCommand command, CancellationToken ct)
    {
        var account = await userManager.FindByEmailAsync(command.Request.Email);
        if (account == null || !account.IsActive || account.IsDeleted)
        {
            // Return success to avoid email enumeration attacks
            return Result<bool>.SuccessResult(true, AppMessageCodes.PASSWORD_RESET_EMAIL_SENT);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(account);
        await emailService.SendPasswordResetEmailAsync(account.Email!, token, ct);

        return Result<bool>.SuccessResult(true, AppMessageCodes.PASSWORD_RESET_EMAIL_SENT);
    }
}
