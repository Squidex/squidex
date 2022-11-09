// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Squidex.Web;

public sealed class ApiExceptionFilterAttribute : ActionFilterAttribute, IExceptionFilter, IAsyncActionFilter
{
    public override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: ProblemDetails problem })
        {
            var (error, _) = problem.ToErrorDto(context.HttpContext);

            context.Result = GetResult(error);
        }

        return next();
    }

    public void OnException(ExceptionContext context)
    {
        var (error, unhandled) = context.Exception.ToErrorDto(context.HttpContext);

        if (unhandled != null)
        {
            var log = context.HttpContext.RequestServices.GetRequiredService<ILogger<ApiExceptionFilterAttribute>>();

            log.LogError(unhandled, "An unexpected exception has occurred.");
        }

        context.Result = GetResult(error);
    }

    private static IActionResult GetResult(ErrorDto error)
    {
        if (error.StatusCode == 404)
        {
            return new NotFoundResult();
        }

        return new ObjectResult(error)
        {
            StatusCode = error.StatusCode
        };
    }
}
