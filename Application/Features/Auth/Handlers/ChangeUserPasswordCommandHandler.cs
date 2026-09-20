using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Accounts;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EcoBridge.Application.Features.Auth.Handlers;

public class ChangeUserPasswordCommandHandler(
    UserManager<Account> userManager
) : IRequestHandler<ChangeUserPasswordCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(
        ChangeUserPasswordCommand command,
        CancellationToken ct)
    {
        if (command.UserId == Guid.Empty)
            throw new DomainException.UnauthorizedException(AppMessageCodes.UNAUTHORIZED);

        var account = await userManager.FindByIdAsync(command.UserId.ToString());

        if (account is null)
            throw new DomainException.NotFoundException(AppMessageCodes.USER_NOT_FOUND);

        var result = await userManager.ChangePasswordAsync(
            account,
            command.Request.CurrentPassword,
            command.Request.NewPassword);

        if (!result.Succeeded)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_CREDENTIALS);

        return ApiResult<bool>.Ok(true, AppMessageCodes.PASSWORD_CHANGE_SUCCESS);
    }
}