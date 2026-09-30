using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EcoBridge.API.Filters
{
    public class ValidateCsrfTokenAttribute : Attribute,IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var csrfFromCookie = httpContext.Request.Cookies["csrfToken"];
            var csrfFromHeader = httpContext.Request.Headers["X-CSRF-TOKEN"].FirstOrDefault();

            if (string.IsNullOrEmpty(csrfFromCookie) ||
                string.IsNullOrEmpty(csrfFromHeader) ||
                !string.Equals(csrfFromCookie, csrfFromHeader, StringComparison.Ordinal))
            {
                context.Result = new ObjectResult(new { message = "Invalid or missing CSRF token" })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next();
        }
    }
}
