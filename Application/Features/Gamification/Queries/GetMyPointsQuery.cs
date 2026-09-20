using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Gamification.Queries;

public record GetMyPointsQuery(Guid UserId, int Page = 1, int PageSize = 10) : IRequest<ApiResult<MyPointsResponseDTO>>;

public class GetMyPointsQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetMyPointsQuery, ApiResult<MyPointsResponseDTO>>
{
    public async Task<ApiResult<MyPointsResponseDTO>> Handle(GetMyPointsQuery request, CancellationToken ct)
    {
        var account = await dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.UserId && !a.IsDeleted, ct);

        if (account == null)
        {
            return ApiResult<MyPointsResponseDTO>.Fail( AppMessageCodes.USER_NOT_FOUND);
        }

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 || request.PageSize > 100 ? 10 : request.PageSize;

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

        var pagedTransactions = new PagedResult<PointTransactionDTO>(items, totalCount, page, pageSize);

        var response = new MyPointsResponseDTO
        {
            TotalPoints = account.TotalWalletPoints,
            TrustScore = account.TrustScore,
            Transactions = pagedTransactions
        };

        return ApiResult<MyPointsResponseDTO>.Ok(response, AppMessageCodes.POINTS_RETRIEVED);
    }
}
