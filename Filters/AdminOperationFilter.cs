using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FoodMart.Filters;

public sealed class AdminOperationFilter(ILogger<AdminOperationFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = await next();
        if (HttpMethods.IsPost(context.HttpContext.Request.Method) &&
            context.RouteData.Values["area"]?.ToString() == "Admin" &&
            context.RouteData.Values["controller"]?.ToString() != "Account" &&
            result.Exception is null && result.Result is RedirectToActionResult)
            logger.LogInformation("Admin {AdminId} completed {Controller}/{Action}.",
                context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
                context.RouteData.Values["controller"], context.RouteData.Values["action"]);
    }
}
