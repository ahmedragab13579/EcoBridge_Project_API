using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Models;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Auth.Commands;

public class ChangeUserPasswordCommandHandler : IRequestHandler<ChangeUserPasswordCommand, Result<bool>>
{
    private readonly IEcoBridgeDbContext _dbContext;
    private readonly PasswordHasher<Account> _passwordHasher = new();

    public ChangeUserPasswordCommandHandler(IEcoBridgeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(ChangeUserPasswordCommand command, CancellationToken ct)
    {
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == command.UserId, ct);
        if (account is null)
        {
            return Result<bool>.FailResult(false, AppMessageCodes.USER_NOT_FOUND, ErrorType.NotFound);
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash ?? string.Empty, command.Request.CurrentPassword);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return Result<bool>.FailResult(false, AppMessageCodes.INVALID_CREDENTIALS, ErrorType.Unauthorized);
        }

        account.ChangePassword(_passwordHasher.HashPassword(account, command.Request.NewPassword));
        await _dbContext.SaveChangesAsync(ct);

        return Result<bool>.SuccessResult(true, AppMessageCodes.PASSWORD_CHANGE_SUCCESS);
    }
}
