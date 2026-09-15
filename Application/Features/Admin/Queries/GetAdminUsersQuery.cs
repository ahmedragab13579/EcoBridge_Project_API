using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Admin.Queries;

public record GetAdminUsersQuery(UserRole? RoleFilter) : IRequest<Result<List<UserDTO>>>;

public class GetAdminUsersQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetAdminUsersQuery, Result<List<UserDTO>>>
{
    public async Task<Result<List<UserDTO>>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Accounts.AsNoTracking();

        if (request.RoleFilter.HasValue)
        {
            query = query.Where(u => u.RoleId == request.RoleFilter.Value);
        }

        var users = await query
            .Select(u => new UserDTO
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                Phone = u.Phone,
                RoleId = u.RoleId,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<UserDTO>>.SuccessResult(users, AppMessageCodes.USERS_RETRIEVED);
    }
}
