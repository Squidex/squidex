// ==========================================================================
//  AppApiFilter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Pipeline
{
    public sealed class AppApiFilter : IAsyncActionFilter
    {
        private readonly IAppProvider appProvider;
        private readonly IAppPlansProvider appPlanProvider;
        private readonly IUsageTracker usageTracker;

        public AppApiFilter(IAppProvider appProvider, IAppPlansProvider appPlanProvider, IUsageTracker usageTracker)
        {
            this.appProvider = appProvider;
            this.appPlanProvider = appPlanProvider;

            this.usageTracker = usageTracker;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var appName = context.RouteData.Values["app"]?.ToString();

            if (!string.IsNullOrWhiteSpace(appName))
            {
                var app = await appProvider.FindAppByNameAsync(appName);

                if (app == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var plan = appPlanProvider.GetPlanForApp(app);

                var usage = await usageTracker.GetMonthlyCalls(app.Id.ToString(), DateTime.Today);

                if (plan.MaxApiCalls >= 0 && (usage * 1.1) > plan.MaxApiCalls)
                {
                    context.Result = new StatusCodeResult(429);
                    return;
                }

                context.HttpContext.Features.Set<IAppFeature>(new AppFeature(app));
            }

            await next();
        }
    }
}
