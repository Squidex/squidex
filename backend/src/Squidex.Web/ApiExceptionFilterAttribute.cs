// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Squidex.Web
{
    public sealed class ApiExceptionFilterAttribute : ActionFilterAttribute, IExceptionFilter, IAsyncActionFilter
    {
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var resultContext = await next();

            if (resultContext.Result is ObjectResult objectResult && objectResult.Value is ProblemDetails problem)
            {
                var error = new ErrorDto { Message = problem.Title, Type = problem.Type, StatusCode = problem.Status };

                if (problem.Extensions.TryGetValue("traceId", out var temp) && temp is string traceId)
                {
                    error.TraceId = traceId;
                }

                objectResult.Value = error;
            }
        }

        public void OnException(ExceptionContext context)
        {
            var error = context.Exception.ToErrorDto(context.HttpContext);

            if (error.StatusCode == 404)
            {
                context.Result = new NotFoundResult();
            }
            else
            {
                context.Result = new ObjectResult(error)
                {
                    StatusCode = error.StatusCode
                };
            }
        }
    }
}
