using EcoBridge.Application.Features.Reviews.Commands;
using EcoBridge.Domain.Errors;
using FluentValidation;

namespace EcoBridge.Application.Features.Reviews.Validation;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.ReviewerId)
            .NotEmpty()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        RuleFor(x => x.Dto)
            .NotNull()
            .WithErrorCode(AppMessageCodes.INVALID_INPUT);

        When(x => x.Dto != null, () =>
        {
            RuleFor(x => x.Dto.MissionId)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.INVALID_INPUT);

            RuleFor(x => x.Dto.RevieweeId)
                .NotEmpty()
                .WithErrorCode(AppMessageCodes.INVALID_INPUT);

            RuleFor(x => x.Dto.ReviewReasonId)
                .GreaterThan(0)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT);

            RuleFor(x => x.Dto.Comment)
                .MaximumLength(1000)
                .WithErrorCode(AppMessageCodes.INVALID_INPUT)
                .When(x => !string.IsNullOrWhiteSpace(x.Dto.Comment));
        });
    }
}