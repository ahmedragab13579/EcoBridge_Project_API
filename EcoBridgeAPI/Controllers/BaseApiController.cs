using System.Security.Claims;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.API.Controllers.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcoBridge.API.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
        }
    }

    /// <summary>
    /// Centralized Result mapper for Result<T> to eliminate manual if (result.Success) checks across all controllers.
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result, Func<IActionResult>? onSuccess = null)
    {
        return result.ToActionResult(this, onSuccess);
    }

    /// <summary>
    /// Centralized Result mapper for non-generic Result.
    /// </summary>
    protected IActionResult HandleResult(Result result, Func<IActionResult>? onSuccess = null)
    {
        return result.ToActionResult(this, onSuccess);
    }

    protected IActionResult ApiOk<T>(T data, string code = AppMessageCodes.SUCCESS)
    {
        return Ok(ApiResult<T>.Ok(data, code));
    }

    protected IActionResult ApiOk(string code = AppMessageCodes.SUCCESS)
    {
        return Ok(ApiResult.Ok(code));
    }

    protected IActionResult ApiFail(string errorCode, int statusCode = StatusCodes.Status400BadRequest)
    {
        return StatusCode(statusCode, ApiResult.Fail(errorCode));
    }

    protected IActionResult ApiFail<T>(string errorCode, int statusCode = StatusCodes.Status400BadRequest, T? data = default)
    {
        return StatusCode(statusCode, ApiResult<T>.Fail(errorCode, data));
    }

    protected IActionResult ApiBadRequest(string errorCode = AppMessageCodes.VALIDATION_ERROR)
    {
        return BadRequest(ApiResult.Fail(errorCode));
    }

    protected IActionResult ApiNotFound(string errorCode = AppMessageCodes.NOT_FOUND)
    {
        return NotFound(ApiResult.Fail(errorCode));
    }

    protected IActionResult ApiUnauthorized(string errorCode = AppMessageCodes.UNAUTHORIZED)
    {
        return Unauthorized(ApiResult.Fail(errorCode));
    }

    protected IActionResult ApiForbidden(string errorCode = AppMessageCodes.FORBIDDEN)
    {
        return StatusCode(StatusCodes.Status403Forbidden, ApiResult.Fail(errorCode));
    }

    protected IActionResult ApiConflict(string errorCode = AppMessageCodes.CONCURRENCY_CONFLICT)
    {
        return Conflict(ApiResult.Fail(errorCode));
    }
}
