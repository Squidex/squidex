// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Pipeline
{
    public sealed class ApiCostsFilter : IAsyncActionFilter, IFilterContainer
    {
        private readonly IAppPlansProvider appPlanProvider;
        private readonly IUsageTracker usageTracker;

        public ApiCostsFilter(IAppPlansProvider appPlanProvider, IUsageTracker usageTracker)
        {
            this.appPlanProvider = appPlanProvider;

            this.usageTracker = usageTracker;
        }

        IFilterMetadata IFilterContainer.FilterDefinition { get; set; }

        public ApiCostsAttribute FilterDefinition
        {
            get
            {
                return (ApiCostsAttribute)((IFilterContainer)this).FilterDefinition;
            }
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var appFeature = context.HttpContext.Features.Get<IAppFeature>();

            if (appFeature?.App != null && FilterDefinition.Weight > 0)
            {
                var stopWatch = Stopwatch.StartNew();

                try
                {
                    var plan = appPlanProvider.GetPlanForApp(appFeature.App);

                    var usage = await usageTracker.GetMonthlyCalls(appFeature.App.Id.ToString(), DateTime.Today);

                    if (plan.MaxApiCalls >= 0 && (usage * 1.1) > plan.MaxApiCalls)
                    {
                        context.Result = new StatusCodeResult(429);
                        return;
                    }

                    await next();
                }
                finally
                {
                    stopWatch.Stop();

                    await usageTracker.TrackAsync(appFeature.App.Id.ToString(), FilterDefinition.Weight, stopWatch.ElapsedMilliseconds);
                }
            }
            else
            {
                await next();
            }
        }
    }
}
