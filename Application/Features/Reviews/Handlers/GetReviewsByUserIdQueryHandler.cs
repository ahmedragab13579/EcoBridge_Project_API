using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Reviews.Queriess;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Reviews.Queries;

public class GetReviewsByUserIdQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetReviewsByUserIdQuery, ApiResult<PagedResult<ReviewResponseDTO>>>
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    public async Task<ApiResult<PagedResult<ReviewResponseDTO>>> Handle(
        GetReviewsByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        // ── Guards ─────────────────────────────────────────────
        if (request.userid == Guid.Empty)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_USER_ID);

        if (!Enum.IsDefined(request.role))
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_USER_ID);

        // ── Normalize paging (silent clamp) ────────────────────
        var page = request.Page < 1 ? 1 : request.Page;

        var pageSize = request.PageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => request.PageSize
        };

        // ── Base query: exclude soft-deleted ───────────────────
        var query = dbContext.Reviews
            .AsNoTracking()
            .Where(r => !r.IsDeleted);

        // ── Filter by role ─────────────────────────────────────
        query = request.role == ReviewRole.Reviewer
            ? query.Where(r => r.ReviewerId == request.userid)
            : query.Where(r => r.RevieweeId == request.userid);

        // ── Count before paging ────────────────────────────────
        var totalCount = await query.CountAsync(cancellationToken);

        // ── Project + page ─────────────────────────────────────
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReviewResponseDTO
            {
                Id = r.Id,
                ReviewerId = r.ReviewerId,
                RevieweeId = r.RevieweeId,
                Rating = r.Rating,
                MissionId = r.MissionId,
                ReviewReasonId = r.ReviewReasonId,
                ReviewReasonCode = r.ReviewReason.Code,
                IsPositive = r.ReviewReason.IsPositive,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // ── Wrap ───────────────────────────────────────────────
        var paged = new PagedResult<ReviewResponseDTO>(
            items,
            totalCount,
            page,
            pageSize);

        return ApiResult<PagedResult<ReviewResponseDTO>>.Ok(
            paged,
            AppMessageCodes.REVIEWS_RETRIEVED);
    }
}