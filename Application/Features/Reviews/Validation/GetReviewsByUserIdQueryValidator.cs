using EcoBridge.Application.Features.Reviews.Queries;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Reviews.Validation;

public class GetReviewsByUserIdQueryValidator : AbstractValidator<GetReviewsByUserIdQuery>
{
    public GetReviewsByUserIdQueryValidator()
    {
        RuleFor(x => x.userid)
            .NotEmpty()
                .WithMessage(AppMessageCodes.INVALID_USER_ID);

        RuleFor(x => x.role)
            .IsInEnum()
                .WithMessage(AppMessageCodes.INVALID_REVIEW_ROLE);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
                .WithMessage(AppMessageCodes.PAGE_INVALID);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
                .WithMessage(AppMessageCodes.PAGE_SIZE_INVALID);
    }
}