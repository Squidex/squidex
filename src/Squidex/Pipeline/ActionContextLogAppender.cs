// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Pipeline
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

        public void Append(IObjectWriter writer)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (string.IsNullOrEmpty(httpContext?.Request?.Method))
            {
                return;
            }

            Guid requestId;

            if (httpContext.Items.TryGetValue(nameof(requestId), out var requestIdvalue) && requestIdvalue is Guid requestIdValue)
            {
                requestId = requestIdValue;
            }
            else
            {
                httpContext.Items[nameof(requestId)] = requestId = Guid.NewGuid();
            }

            var logContext = (requestId, context: httpContext, actionContextAccessor);

            writer.WriteObject("web", logContext, (ctx, w) =>
            {
                w.WriteProperty("requestId", ctx.requestId.ToString());
                w.WriteProperty("requestPath", ctx.context.Request.Path);
                w.WriteProperty("requestMethod", ctx.context.Request.Method);

                var actionContext = ctx.actionContextAccessor.ActionContext;

                if (actionContext != null)
                {
                    w.WriteObject("routeValues", actionContext.ActionDescriptor.RouteValues, (routeValues, r) =>
                    {
                        foreach (var (key, val) in routeValues)
                        {
                            r.WriteProperty(key, val);
                        }
                    });
                }
            });
        }
    }
}
