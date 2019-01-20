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

                        w.WriteObject("ctx", ctx.context, (innerHttpContext, c) =>
                        {
                            var app = innerHttpContext.Features.Get<IAppFeature>()?.App;

                            if (app != null)
                            {
                                c.WriteProperty("appId", app.Id.ToString());
                                c.WriteProperty("appName", app.Name);
                            }

                            var subjectId = innerHttpContext.User.OpenIdSubject();

                            if (!string.IsNullOrWhiteSpace(subjectId))
                            {
                                c.WriteProperty("userId", subjectId);
                            }

                            var clientId = innerHttpContext.User.OpenIdClientId();

                            if (!string.IsNullOrWhiteSpace(subjectId))
                            {
                                c.WriteProperty("clientId", subjectId);
                            }
                        });

                        w.WriteProperty("elapsedRequestMs", ctx.elapsedMs);
                    });
                }
            }
        }
    }
}
