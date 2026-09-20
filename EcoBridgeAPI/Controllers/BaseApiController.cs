using System.Security.Claims;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
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
    /// Extracts Access Token from Request HttpOnly Cookie.
    /// </summary>
    protected string? AccessToken => Request.Cookies["accessToken"];

    /// <summary>
    /// Extracts Refresh Token from Request HttpOnly Cookie.
    /// </summary>
    protected string? RefreshToken => Request.Cookies["refreshToken"];

    // ─────────────────────────────────────────────────────────────
    // Success mappings only.
    // Failure is handled by GlobalExceptionMiddleware via DomainException.
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// 200 OK with a generic ApiResult payload.
    /// </summary>
    protected IActionResult ApiOk<T>(T data, string code = AppMessageCodes.SUCCESS)
        => Ok(ApiResult<T>.Ok(data, code));

    /// <summary>
    /// 200 OK with a non-generic ApiResult payload (no data).
    /// </summary>
    protected IActionResult ApiOk(string code = AppMessageCodes.SUCCESS)
        => Ok(ApiResult.Ok(code));

    /// <summary>
    /// 201 Created with a Location header pointing to the created resource.
    /// </summary>
    protected IActionResult ApiCreated<T>(
        T data,
        string actionName,
        object? routeValues = null,
        string code = AppMessageCodes.SUCCESS)
        => CreatedAtAction(actionName, routeValues, ApiResult<T>.Ok(data, code));

    /// <summary>
    /// 204 No Content — use for DELETE or operations with no response body.
    /// Note: 204 cannot carry a body per HTTP spec.
    /// </summary>
    protected IActionResult ApiNoContent()
        => NoContent();

    /// <summary>
    /// 200 OK with no data but with a code (e.g. "CATEGORY_DELETED").
    /// Use this instead of ApiNoContent when the frontend needs the code to show a message.
    /// </summary>
    protected IActionResult ApiOkWithCode(string code)
        => Ok(ApiResult.Ok(code));
}