using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.Statics.Gamification;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Gamification.Queries;

public record GetMyPointsQuery(Guid UserId, int Page = 1, int PageSize = 10)
    : IRequest<ApiResult<PagedResult<PointTransactionDTO>>>;

public class GetMyPointsQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetMyPointsQuery, ApiResult<PagedResult<PointTransactionDTO>>>
{
    public async Task<ApiResult<PagedResult<PointTransactionDTO>>> Handle(
        GetMyPointsQuery request,
        CancellationToken ct)
    {
        if (request.UserId == Guid.Empty)
            throw new DomainException.UnauthorizedException(AppMessageCodes.UNAUTHORIZED);

        var account = await dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.UserId && !a.IsDeleted, ct);

        if (account == null)
            throw new DomainException.NotFoundException(AppMessageCodes.USER_NOT_FOUND);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        var query = dbContext.PointTransactions
            .AsNoTracking()
            .Where(pt => pt.UserId == request.UserId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(pt => pt.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(pt => new PointTransactionDTO
            {
                Id = pt.Id,
                Amount = pt.Amount,
                TransactionType = pt.TransactionType.ToString(),
                ReferenceId = pt.ReferenceId,
                CreatedAt = pt.CreatedAt
            })
            .ToListAsync(ct);

        var pagedTransactions = new PagedResult<PointTransactionDTO>(
            items, totalCount, page, pageSize);

        return ApiResult<PagedResult<PointTransactionDTO>>.Ok(pagedTransactions, AppMessageCodes.POINTS_RETRIEVED);
    }
}