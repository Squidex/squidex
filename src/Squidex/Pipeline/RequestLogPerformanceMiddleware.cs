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
        private readonly ISemanticLog log;

        public RequestLogPerformanceMiddleware(ISemanticLog log)
        {
            this.log = log;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var startTime = Stopwatch.GetTimestamp();

            using (Profiler.StartSession())
            {
                try
                {
                    await next(context);
                }
                finally
                {
                    var endTime = Stopwatch.GetTimestamp();
                    var elapsed = endTime - startTime;

                    log.LogInformation(w =>
                    {
                        Profiler.Session?.Write(w);

                        w.WriteProperty("elapsedRequestMs", elapsed);
                    });
                }
            }
        }
    }
}
