using EcoBridge.Application.DTO.Users.Auth.Users;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Accounts;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EcoBridge.Application.Features.Auth.Commands;

public record VerifyEmailCommand(VerifyEmailRequestDTO Request) : IRequest<ApiResult<bool>>;

public class VerifyEmailCommandHandler(
    UserManager<Account> userManager
) : IRequestHandler<VerifyEmailCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(
        VerifyEmailCommand command,
        CancellationToken ct)
    {
        var account = await userManager.FindByEmailAsync(command.Request.Email);

        if (account == null || !account.IsActive || account.IsDeleted)
            throw new DomainException.NotFoundException(AppMessageCodes.USER_NOT_FOUND);

        var result = await userManager.ConfirmEmailAsync(account, command.Request.Token);

        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description
                        ?? AppMessageCodes.INVALID_TOKEN;

            throw new DomainException.ValidationException(error);
        }

        return ApiResult<bool>.Ok(true, AppMessageCodes.EMAIL_VERIFIED_SUCCESS);
    }
}