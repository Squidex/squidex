// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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
    public sealed class RequestLogPerformanceMiddleware : IMiddleware
    {
        private readonly RequestLogOptions requestLogOptions;
        private readonly ISemanticLog log;

        public RequestLogPerformanceMiddleware(IOptions<RequestLogOptions> requestLogOptions, ISemanticLog log)
        {
            Guard.NotNull(requestLogOptions, nameof(requestLogOptions));
            Guard.NotNull(log, nameof(log));

            this.requestLogOptions = requestLogOptions.Value;

            this.log = log;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var watch = ValueStopwatch.StartNew();

            using (Profiler.StartSession())
            {
                try
                {
                    await next(context);
                }
                finally
                {
                    var elapsedMs = watch.Stop();

                    if (requestLogOptions.LogRequests)
                    {
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
