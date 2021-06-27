// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Squidex.Log;

namespace Squidex.Web.Pipeline
{
    public sealed class ActionContextLogAppender : ILogAppender
    {
        private readonly IActionContextAccessor actionContextAccessor;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ActionContextLogAppender(IActionContextAccessor actionContextAccessor, IHttpContextAccessor httpContextAccessor)
        {
            this.actionContextAccessor = actionContextAccessor;

            this.httpContextAccessor = httpContextAccessor;
        }

        public void Append(IObjectWriter writer, SemanticLogLevel logLevel, Exception? exception)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (string.IsNullOrEmpty(httpContext?.Request?.Method))
            {
                return;
            }

            var requestId = GetRequestId(httpContext);

            var logContext = (requestId, context: httpContext, actionContextAccessor);

            writer.WriteObject("web", logContext, (ctx, w) =>
            {
                w.WriteProperty("requestId", ctx.requestId);
                w.WriteProperty("requestPath", ctx.context.Request.Path);
                w.WriteProperty("requestMethod", ctx.context.Request.Method);

                var actionContext = ctx.actionContextAccessor.ActionContext;

                if (actionContext != null)
                {
                    w.WriteObject("routeValues", actionContext.ActionDescriptor.RouteValues, (routeValues, r) =>
                    {
                        foreach (var (key, value) in routeValues)
                        {
                            r.WriteProperty(key, value);
                        }
                    });
                }
            });
        }

        private static string GetRequestId(HttpContext httpContext)
        {
            return Activity.Current?.Id ?? httpContext.TraceIdentifier ?? Guid.NewGuid().ToString();
        }
    }
}
