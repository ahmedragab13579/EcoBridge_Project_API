using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.DTO;
using MediatR;
using EcoBridge.Application.Result;

namespace EcoBridge.Application.Features.Volunteer.Commands;

public record UpdateVolunteerCommand(
    Guid Id,
    CreateVolunteerDTO Dto
) : IRequest<ApiResult<bool>>;

public class UpdateVolunteerCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<UpdateVolunteerCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(UpdateVolunteerCommand request, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts.FindAsync([request.Id], cancellationToken);
        if (account == null)
            return ApiResult<bool>.Fail( AppMessageCodes.VOLUNTEER_NOT_FOUND);

        account.SetVehicleInfo(account.VehicleType, request.Dto.VehicleDetails);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.VOLUNTEER_UPDATED);
    }
}
