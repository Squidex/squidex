// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Log;

namespace Squidex.Web.Pipeline
{
    public sealed class RequestLogPerformanceMiddleware
    {
        private readonly RequestLogOptions requestLogOptions;
        private readonly RequestDelegate next;

        public RequestLogPerformanceMiddleware(RequestDelegate next, IOptions<RequestLogOptions> requestLogOptions)
        {
            this.requestLogOptions = requestLogOptions.Value;

            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, ISemanticLog log)
        {
            var shouldStartSession = requestLogOptions.LogRequests || Profiler.HasListener;

            if (requestLogOptions.LogRequests || shouldStartSession)
            {
                var session =
                    shouldStartSession ?
                    Profiler.StartSession() :
                    default;

                var watch =
                    requestLogOptions.LogRequests ?
                    ValueStopwatch.StartNew() :
                    default;

                using (session)
                {
                    try
                    {
                        await next(context);
                    }
                    finally
                    {
                        if (requestLogOptions.LogRequests)
                        {
                            var elapsedMs = watch.Stop();

                            log.LogInformation((elapsedMs, context), (ctx, w) =>
                            {
                                if (requestLogOptions.LogProfiler)
                                {
                                    Profiler.Session?.Write(w);
                                }

                                w.WriteObject("filters", ctx.context, LogFilters);
                                w.WriteProperty("elapsedRequestMs", ctx.elapsedMs);
                            });
                        }
                    }
                }
            }
            else
            {
                await next(context);
            }
        }

        private static void LogFilters(HttpContext httpContext, IObjectWriter c)
        {
            var app = httpContext.Context().App;

            if (app != null)
            {
                c.WriteProperty("appId", app.Id.ToString());
                c.WriteProperty("appName", app.Name);
            }

            var userId = httpContext.User.OpenIdSubject();

            if (!string.IsNullOrWhiteSpace(userId))
            {
                c.WriteProperty(nameof(userId), userId);
            }

            var clientId = httpContext.User.OpenIdClientId();

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                c.WriteProperty(nameof(clientId), clientId);
            }

            var costs = httpContext.Features.Get<IApiCostsFeature>()?.Costs ?? 0;

            c.WriteProperty(nameof(costs), costs);
        }
    }
}
