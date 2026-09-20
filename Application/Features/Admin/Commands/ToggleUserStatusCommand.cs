using EcoBridge.Application.Contracts;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Admin.Commands;

public record ToggleUserStatusCommand(Guid UserId) : IRequest<ApiResult<bool>>;

public class ToggleUserStatusCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<ToggleUserStatusCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(
        ToggleUserStatusCommand request,
        CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.UserId && !a.IsDeleted, cancellationToken);

        if (account == null)
            throw new DomainException.NotFoundException(AppMessageCodes.USER_NOT_FOUND);

        if (account.IsActive)
            account.Deactivate();
        else
            account.Activate();

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Ok(account.IsActive, AppMessageCodes.USER_STATUS_UPDATED);
    }
}