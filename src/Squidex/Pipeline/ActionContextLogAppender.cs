// ==========================================================================
//  ActionContextLogAppender.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Pipeline
{
    public class ActionContextLogAppender : ILogAppender
    {
        private readonly IActionContextAccessor actionContextAccessor;

        public ActionContextLogAppender(IActionContextAccessor actionContextAccessor)
        {
            this.actionContextAccessor = actionContextAccessor;
        }

        public void Append(IObjectWriter writer)
        {
            var actionContext = actionContextAccessor.ActionContext;

            if (actionContext == null)
            {
                return;
            }

            var httpContext = actionContext.HttpContext;

            Guid requestId;

            if (httpContext.Items.TryGetValue(nameof(requestId), out var value) && value is Guid requestIdValue)
            {
                requestId = requestIdValue;
            }
            else
            {
                httpContext.Items[nameof(requestId)] = requestId = Guid.NewGuid();
            }

            writer.WriteObject("web", w => w
                .WriteProperty("requestId", requestId.ToString())
                .WriteProperty("requestPath", httpContext.Request.Path)
                .WriteProperty("requestMethod", httpContext.Request.Method)
                .WriteObject("routeValues", r =>
                {
                    foreach (var kvp in actionContext.ActionDescriptor.RouteValues)
                    {
                        r.WriteProperty(kvp.Key, kvp.Value);
                    }
                }));
        }
    }
}
