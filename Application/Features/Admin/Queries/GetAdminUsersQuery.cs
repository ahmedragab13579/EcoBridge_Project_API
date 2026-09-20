using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Admin.Queries;

public record GetAdminUsersQuery(
    UserRole? RoleFilter,
    int Page = 1,
    int PageSize = 20
) : IRequest<ApiResult<PagedResult<UserDTO>>>;

public class GetAdminUsersQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetAdminUsersQuery, ApiResult<PagedResult<UserDTO>>>
{
    public async Task<ApiResult<PagedResult<UserDTO>>> Handle(
        GetAdminUsersQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, request.PageSize);

        var query = dbContext.Accounts
            .AsNoTracking()
            .Where(u => !u.IsDeleted);

        if (request.RoleFilter.HasValue)
            query = query.Where(u => u.RoleId == request.RoleFilter.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDTO
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                Phone = u.PhoneNumber ?? string.Empty,
                RoleId = u.RoleId,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<UserDTO>(users, totalCount, page, pageSize);

        return ApiResult<PagedResult<UserDTO>>.Ok(pagedResult, AppMessageCodes.USERS_RETRIEVED);
    }
}