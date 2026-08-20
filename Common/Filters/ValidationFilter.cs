using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;
using AppValidationException = DulceAtardecer.Common.Exceptions.ValidationException;

namespace DulceAtardecer.Common.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (object? arg in context.ActionArguments.Values)
        {
            if (arg is null)
            {
                continue;
            }

            Type validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is IValidator validator)
            {
                ValidationResult result = await validator.ValidateAsync(new ValidationContext<object>(arg));
                if (!result.IsValid)
                {
                    throw new AppValidationException(result.Errors);
                }
            }
        }

        await next();
    }
}
