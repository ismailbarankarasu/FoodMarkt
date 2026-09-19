using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Bson;

namespace FoodMart.Filters;

// Runs asynchronous DTO validators before the existing MVC ModelState checks.
public sealed class FormValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var (name, value) in context.ActionArguments)
        {
            if (name.Equals("id", StringComparison.OrdinalIgnoreCase) &&
                (value is not string id || !ObjectId.TryParse(id, out _)))
            {
                context.Result = new NotFoundResult();
                return;
            }
            if (value is null) continue;
            var type = typeof(IValidator<>).MakeGenericType(value.GetType());
            if (context.HttpContext.RequestServices.GetService(type) is not IValidator validator) continue;
            var result = await validator.ValidateAsync(new ValidationContext<object>(value), context.HttpContext.RequestAborted);
            foreach (var error in result.Errors)
                context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
        await next();
    }
}
