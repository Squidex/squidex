// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Squidex.Log;

namespace Squidex.Web.Pipeline;

public sealed class ActionContextLogAppender(IHttpContextAccessor httpContextAccessor) : ILogAppender
{
    public void Append(IObjectWriter writer, SemanticLogLevel logLevel, Exception? exception)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (string.IsNullOrEmpty(httpContext?.Request?.Method))
        {
            return;
        }

        var actionDescriptor = httpContext.GetEndpoint()?.Metadata.GetMetadata<ActionDescriptor>();
        try
        {
            if (string.IsNullOrEmpty(httpContext?.Request?.Method))
            {
                return;
            }

            var requestId = GetRequestId(httpContext);

            var logContext = (requestId, context: httpContext, actionDescriptor);

            writer.WriteObject("web", logContext, (ctx, w) =>
            {
                w.WriteProperty("requestId", ctx.requestId);
                w.WriteProperty("requestPath", ctx.context.Request.Path);
                w.WriteProperty("requestMethod", ctx.context.Request.Method);

                if (ctx.actionDescriptor != null)
                {
                    w.WriteObject("routeValues", ctx.actionDescriptor.RouteValues, (routeValues, r) =>
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
