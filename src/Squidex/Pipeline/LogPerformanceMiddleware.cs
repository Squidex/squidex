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
    public sealed class LogPerformanceMiddleware : ActionFilterAttribute
    {
        private readonly RequestDelegate next;
        private readonly ISemanticLog log;

        public LogPerformanceMiddleware(RequestDelegate next, ISemanticLog log)
        {
            this.next = next;
            this.log = log;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopWatch = Stopwatch.StartNew();

            await next(context);

            stopWatch.Stop();

            log.LogInformation(w => w.WriteProperty("elapsedRequestMs", stopWatch.ElapsedMilliseconds));
        }
    }
}
