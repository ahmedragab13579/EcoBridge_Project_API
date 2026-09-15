using EcoBridge.Application.DTO;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EcoBridge.Application.Features.Auth.Commands;

public record ResetPasswordCommand(ResetPasswordRequestDTO Request) : IRequest<Result<bool>>;

public class ResetPasswordCommandHandler(
    UserManager<Account> userManager
) : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        var account = await userManager.FindByEmailAsync(command.Request.Email);
        if (account == null || !account.IsActive || account.IsDeleted)
        {
            return Result<bool>.FailResult(false, AppMessageCodes.USER_NOT_FOUND, ErrorType.NotFound);
        }

        var result = await userManager.ResetPasswordAsync(account, command.Request.Token, command.Request.NewPassword);
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description ?? AppMessageCodes.INVALID_TOKEN;
            return Result<bool>.FailResult(false, error, ErrorType.Validation);
        }

        return Result<bool>.SuccessResult(true, AppMessageCodes.PASSWORD_RESET_SUCCESS);
    }
}
