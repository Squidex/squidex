// ==========================================================================
//  AppTrackingMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Pipeline
{
    public sealed class AppTrackingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IUsageTracker usageTracker;

        public AppTrackingMiddleware(RequestDelegate next, IUsageTracker usageTracker)
        {
            this.next = next;
            this.usageTracker = usageTracker;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopWatch = Stopwatch.StartNew();

            await next(context);

            var appFeature = context.Features.Get<IAppFeature>();

            if (appFeature?.App != null)
            {
                stopWatch.Stop();

                var weight = context.Features.Get<IAppTrackingWeightFeature>()?.Weight ?? 1;

                await usageTracker.TrackAsync(appFeature.App.Id.ToString(), weight, stopWatch.ElapsedMilliseconds);
            }
        }
    }
}
