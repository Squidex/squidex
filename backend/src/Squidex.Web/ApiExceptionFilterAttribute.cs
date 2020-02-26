// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Web.Pipeline;

namespace Squidex.Web
{
    public sealed class ApiExceptionFilterAttribute : ActionFilterAttribute, IExceptionFilter, IAsyncActionFilter
    {
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var resultContext = await next();

            if (resultContext.Result is ObjectResult objectResult && objectResult.Value is ProblemDetails problem)
            {
                var (error, _) = problem.ToErrorDto(context.HttpContext);

                context.Result = GetResult(error);
            }
        }

        public void OnException(ExceptionContext context)
        {
            var (error, wellKnown) = context.Exception.ToErrorDto(context.HttpContext);

            if (!wellKnown)
            {
                var exceptionHandler = context.HttpContext.RequestServices.GetService<IExceptionHandler>();

                exceptionHandler?.Handle(context.Exception);
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
}
