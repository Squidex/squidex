// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Security;

namespace Squidex.Pipeline
{
    public sealed class RequestLogPerformanceMiddleware : IMiddleware
    {
        private readonly ISemanticLog log;

        public RequestLogPerformanceMiddleware(ISemanticLog log)
        {
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

                    log.LogInformation((elapsedMs, context), (ctx, w) =>
                    {
                        Profiler.Session?.Write(w);

                        w.WriteObject("filters", ctx.context, LogFilters);
                        w.WriteProperty("elapsedRequestMs", ctx.elapsedMs);
                    });
                }
            }
        }

        private static void LogFilters(HttpContext context, IObjectWriter c)
        {
            var app = context.Features.Get<IAppFeature>()?.App;

            if (app != null)
            {
                c.WriteProperty("appId", app.Id.ToString());
                c.WriteProperty("appName", app.Name);
            }

            var userId = context.User.OpenIdSubject();

            if (!string.IsNullOrWhiteSpace(userId))
            {
                c.WriteProperty(nameof(userId), userId);
            }

            var clientId = context.User.OpenIdClientId();

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                c.WriteProperty(nameof(clientId), clientId);
            }

            var costs = context.Features.Get<IApiCostsFeature>()?.Weight ?? 0;

            c.WriteProperty(nameof(costs), costs);
        }
    }
}
