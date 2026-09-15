using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Admin.Commands;

public record ToggleUserStatusCommand(Guid UserId) : IRequest<Result<bool>>;

public class ToggleUserStatusCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<ToggleUserStatusCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == request.UserId, cancellationToken);
        if (account == null)
        {
            return Result<bool>.FailResult(false, AppMessageCodes.USER_NOT_FOUND, ErrorType.NotFound);
        }

        if (account.IsActive)
        {
            account.Deactivate();
        }
        else
        {
            account.Activate();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.SuccessResult(true, AppMessageCodes.USER_STATUS_UPDATED);
    }
}
