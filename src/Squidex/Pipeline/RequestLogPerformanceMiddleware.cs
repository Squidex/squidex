// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Infrastructure.Log;

namespace Squidex.Pipeline
{
    public sealed class RequestLogPerformanceMiddleware : ActionFilterAttribute
    {
        private readonly RequestLogProfilerSessionProvider requestSession;
        private readonly RequestDelegate next;
        private readonly ISemanticLog log;

        public RequestLogPerformanceMiddleware(RequestLogProfilerSessionProvider requestSession, RequestDelegate next, ISemanticLog log)
        {
            this.requestSession = requestSession;
            this.next = next;
            this.log = log;
        }

        public async Task Invoke(HttpContext context)
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
