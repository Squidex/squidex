// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure.Log;

namespace Squidex.Pipeline
{
    public sealed class RequestLogPerformanceMiddleware : IMiddleware
    {
        private readonly RequestLogProfilerSessionProvider requestSession;
        private readonly ISemanticLog log;

        public RequestLogPerformanceMiddleware(RequestLogProfilerSessionProvider requestSession, ISemanticLog log)
        {
            this.requestSession = requestSession;

            this.log = log;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var stopWatch = Stopwatch.StartNew();

            var session = new ProfilerSession();

            try
            {
                requestSession.Start(context, session);

                await next(context);
            }
            finally
            {
                stopWatch.Stop();

                log.LogInformation(w =>
                {
                    session.Write(w);

                    w.WriteProperty("elapsedRequestMs", stopWatch.ElapsedMilliseconds);
                });
            }
        }
    }
}
