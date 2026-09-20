using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models.Accounts;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EcoBridge.Application.Features.Auth.Commands;

public record ResetPasswordCommand(ResetPasswordRequestDTO Request) : IRequest<ApiResult<bool>>;

public class ResetPasswordCommandHandler(
    UserManager<Account> userManager
) : IRequestHandler<ResetPasswordCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        var account = await userManager.FindByEmailAsync(command.Request.Email);
        if (account == null || !account.IsActive || account.IsDeleted)
        {
            return ApiResult<bool>.Fail( AppMessageCodes.USER_NOT_FOUND);
        }

        var result = await userManager.ResetPasswordAsync(account, command.Request.Token, command.Request.NewPassword);
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description ?? AppMessageCodes.INVALID_TOKEN;
            return ApiResult<bool>.Fail(error);
        }

        return ApiResult<bool>.Ok(true, AppMessageCodes.PASSWORD_RESET_SUCCESS);
    }
}
