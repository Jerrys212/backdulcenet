using DulceAtardecer.Common.Responses;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace DulceAtardecer.Common.Filters;

public class ApiResponseWrapperFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: not null } objectResult)
        {
            Type valueType = objectResult.Value.GetType();
            bool alreadyWrapped = valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(ApiResponse<>);

            if (!alreadyWrapped)
            {
                Type responseType = typeof(ApiResponse<>).MakeGenericType(valueType);
                object wrapped = Activator.CreateInstance(responseType, true, objectResult.Value, null)!;
                objectResult.Value = wrapped;
            }
        }

        await next();
    }
}
