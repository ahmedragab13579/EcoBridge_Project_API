using EcoBridge.Application.Contracts;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Volunteer.Commands;

public record DeleteVolunteerCommand(Guid Id) : IRequest<ApiResult<bool>>;

public class DeleteVolunteerCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<DeleteVolunteerCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(DeleteVolunteerCommand request, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts.FindAsync([request.Id], cancellationToken);
        if (account == null)
            return ApiResult<bool>.Fail( AppMessageCodes.VOLUNTEER_NOT_FOUND);

        account.SoftDelete();
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.VOLUNTEER_DELETED);
    }
}
