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

            if (httpContext.Items.TryGetValue(nameof(requestId), out var value) && value is Guid requestIdValue)
            {
                requestId = requestIdValue;
            }
            else
            {
                httpContext.Items[nameof(requestId)] = requestId = Guid.NewGuid();
            }

            writer.WriteObject("web", w =>
            {
                w.WriteProperty("requestId", requestId.ToString());
                w.WriteProperty("requestPath", httpContext.Request.Path);
                w.WriteProperty("requestMethod", httpContext.Request.Method);

                var actionContext = actionContextAccessor.ActionContext;

                if (actionContext != null)
                {
                    w.WriteObject("routeValues", r =>
                    {
                        foreach (var kvp in actionContext.ActionDescriptor.RouteValues)
                        {
                            r.WriteProperty(kvp.Key, kvp.Value);
                        }
                    });
                }
            });
        }
    }
}
