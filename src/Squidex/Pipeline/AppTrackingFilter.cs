// ==========================================================================
//  AppTrackingFilter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Infrastructure.UsageTracking;

// ReSharper disable InvertIf

namespace Squidex.Pipeline
{
    public sealed class AppTrackingFilter : ActionFilterAttribute
    {
        private readonly IUsageTracker usageTracker;

        public AppTrackingFilter(IUsageTracker usageTracker)
        {
            this.usageTracker = usageTracker;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var appFeature = context.HttpContext.Features.Get<IAppFeature>();

            if (appFeature?.App != null)
            {
                context.HttpContext.Items["AppWatch"] = Stopwatch.StartNew();
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var appFeature = context.HttpContext.Features.Get<IAppFeature>();

            if (appFeature?.App != null)
            {
                var stopWatch = (Stopwatch)context.HttpContext.Items["AppWatch"];

                stopWatch.Stop();

                usageTracker.TrackAsync(appFeature.App.Id.ToString(), stopWatch.ElapsedMilliseconds);
            }
        }
    }
}
