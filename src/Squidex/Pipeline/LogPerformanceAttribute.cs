// ==========================================================================
//  LogPerformanceAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Infrastructure.Log;

namespace Squidex.Pipeline
{
    public sealed class LogPerformanceAttribute : ActionFilterAttribute
    {
        private readonly ISemanticLog log;

        public LogPerformanceAttribute(ISemanticLog log)
        {
            this.log = log;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Items["Watch"] = Stopwatch.StartNew();
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var stopWatch = (Stopwatch)context.HttpContext.Items["Watch"];
            
            stopWatch.Stop();

            log.LogInformation(w => w.WriteProperty("elapsedRequestMs", stopWatch.ElapsedMilliseconds));
        }
    }
}
