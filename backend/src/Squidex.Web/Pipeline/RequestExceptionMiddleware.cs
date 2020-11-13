// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Squidex.Infrastructure;
using Squidex.Log;

namespace Squidex.Web.Pipeline
{
    public sealed class RequestExceptionMiddleware : IMiddleware
    {
        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();
        private static readonly RouteData EmptyRouteData = new RouteData();
        private readonly IActionResultExecutor<ObjectResult> resultWriter;
        private readonly ISemanticLog log;

        public RequestExceptionMiddleware(IActionResultExecutor<ObjectResult> resultWriter, ISemanticLog log)
        {
            Guard.NotNull(resultWriter, nameof(resultWriter));
            Guard.NotNull(log, nameof(log));

            this.resultWriter = resultWriter;

            this.log = log;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Query.TryGetValue("error", out var header) && int.TryParse(header, out var statusCode) && IsErrorStatusCode(statusCode))
            {
                var (error, _) = ApiExceptionConverter.ToErrorDto(statusCode, context);

                await WriteErrorAsync(context, error);
                return;
            }

            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w.WriteProperty("message", "An unexpected exception has occurred."));

                if (!context.Response.HasStarted)
                {
                    var (error, _) = ex.ToErrorDto(context);

                    await WriteErrorAsync(context, error);
                }
            }

            if (IsErrorStatusCode(context.Response.StatusCode) && !context.Response.HasStarted)
            {
                var (error, _) = ApiExceptionConverter.ToErrorDto(context.Response.StatusCode, context);

                await WriteErrorAsync(context, error);
            }
        }

        private async Task WriteErrorAsync(HttpContext context, ErrorDto error)
        {
            var actionRouteData = context.GetRouteData() ?? EmptyRouteData;
            var actionContext = new ActionContext(context, actionRouteData, EmptyActionDescriptor);

            await resultWriter.ExecuteAsync(actionContext, new ObjectResult(error)
            {
                StatusCode = error.StatusCode
            });
        }

        private static bool IsErrorStatusCode(int statusCode)
        {
            return statusCode >= 400 && statusCode < 600;
        }
    }
}
