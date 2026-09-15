using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcoBridge.API.Controllers.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase? controller = null, Func<IActionResult>? onSuccess = null)
    {
        if (result.Success)
        {
            if (onSuccess != null) return onSuccess();
            return new OkObjectResult(ApiResult<T>.Ok(result.Value!, result.Message));
        }

        var failBody = ApiResult<T>.Fail(result.Message, result.Value);

        return result.ErrorType switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(failBody),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(failBody),
            ErrorType.Forbidden => new ObjectResult(failBody) { StatusCode = StatusCodes.Status403Forbidden },
            ErrorType.Conflict => new ConflictObjectResult(failBody),
            ErrorType.Validation => new BadRequestObjectResult(failBody),
            _ => new BadRequestObjectResult(failBody)
        };
    }

    public static IActionResult ToActionResult(this Result result, ControllerBase? controller = null, Func<IActionResult>? onSuccess = null)
    {
        if (result.Success)
        {
            if (onSuccess != null) return onSuccess();
            return new OkObjectResult(ApiResult.Ok(result.Message));
        }

        var failBody = ApiResult.Fail(result.Message);

        return result.ErrorType switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(failBody),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(failBody),
            ErrorType.Forbidden => new ObjectResult(failBody) { StatusCode = StatusCodes.Status403Forbidden },
            ErrorType.Conflict => new ConflictObjectResult(failBody),
            ErrorType.Validation => new BadRequestObjectResult(failBody),
            _ => new BadRequestObjectResult(failBody)
        };
    }
}
