// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Log;

namespace Squidex.Web.Pipeline
{
    public sealed class ActionContextLogAppender : ILogAppender
    {
        private readonly IServiceProvider services;

        public ActionContextLogAppender(IServiceProvider services)
        {
            this.services = services;
        }

        public void Append(IObjectWriter writer, SemanticLogLevel logLevel, Exception? exception)
        {
            var httpContextAccessor = services.GetService<IHttpContextAccessor>();

            if (string.IsNullOrEmpty(httpContextAccessor?.HttpContext?.Request?.Method))
            {
                return;
            }

            var actionContext = services.GetRequiredService<IActionContextAccessor>()?.ActionContext;

            try
            {
                var httpContext = httpContextAccessor.HttpContext;

                if (string.IsNullOrEmpty(httpContext?.Request?.Method))
                {
                    return;
                }

                var requestId = GetRequestId(httpContext);

                var logContext = (requestId, context: httpContext, actionContext);

                writer.WriteObject("web", logContext, (ctx, w) =>
                {
                    w.WriteProperty("requestId", ctx.requestId);
                    w.WriteProperty("requestPath", ctx.context.Request.Path);
                    w.WriteProperty("requestMethod", ctx.context.Request.Method);

                    var actionContext = ctx.actionContext;

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
            catch
            {
                return;
            }
        }

        private static string GetRequestId(HttpContext httpContext)
        {
            return Activity.Current?.Id ?? httpContext.TraceIdentifier ?? Guid.NewGuid().ToString();
        }
    }
}
