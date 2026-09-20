using EcoBridge.Application.Contracts;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Volunteer.Commands;

public record ToggleVolunteerAvailabilityCommand(
    Guid AccountId,
    bool IsAvailable
) : IRequest<ApiResult<bool>>;

public class ToggleVolunteerAvailabilityCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<ToggleVolunteerAvailabilityCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(ToggleVolunteerAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts.FindAsync([request.AccountId], cancellationToken);
        if (account == null)
            return ApiResult<bool>.Fail( AppMessageCodes.VOLUNTEER_NOT_FOUND);

        account.SetAvailability(request.IsAvailable);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.VOLUNTEER_AVAILABILITY_UPDATED);
    }
}
