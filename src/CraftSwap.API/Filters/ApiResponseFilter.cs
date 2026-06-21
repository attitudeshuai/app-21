using CraftSwap.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CraftSwap.Filters;

public class ApiResponseFilter : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.Value is ApiResponse || objectResult.Value?.GetType().IsGenericType == true &&
                objectResult.Value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                return;
            }

            var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            var isSuccess = statusCode >= 200 && statusCode < 300;

            if (isSuccess)
            {
                context.Result = new ObjectResult(ApiResponse.Success(objectResult.Value))
                {
                    StatusCode = statusCode
                };
            }
        }
        else if (context.Result is EmptyResult)
        {
            context.Result = new ObjectResult(ApiResponse.Success())
            {
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
