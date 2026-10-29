using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Result;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace EcoBridge.Application.Features.Reviews.Commands;

public record UpdateReviewCommand(
    int ReviewId,
    Guid ReviewerId,
    UpdateReviewDTO Dto
) : IRequest<ApiResult<ReviewResponseDTO>>
{



};

public class UpdateReviewDTO
{
    [MaxLength(500)]
    public string? Comment { get; set; }

    [Range(1, 5)]
    public int? Rating { get; set; }
}