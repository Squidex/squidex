// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Web.Pipeline
{
    public sealed class ApiCostsFilter : IAsyncActionFilter, IFilterContainer
    {
        private readonly IAppPlansProvider appPlansProvider;
        private readonly IApiUsageTracker usageTracker;

        public ApiCostsFilter(IAppPlansProvider appPlansProvider, IApiUsageTracker usageTracker)
        {
            Guard.NotNull(appPlansProvider);
            Guard.NotNull(usageTracker);

            this.appPlansProvider = appPlansProvider;

            this.usageTracker = usageTracker;
        }

        IFilterMetadata IFilterContainer.FilterDefinition { get; set; }

        public ApiCostsAttribute FilterDefinition
        {
            get
            {
                return (ApiCostsAttribute)((IFilterContainer)this).FilterDefinition;
            }
            set
            {
                ((IFilterContainer)this).FilterDefinition = value;
            }
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.HttpContext.Features.Set<IApiCostsFeature>(FilterDefinition);

            var app = context.HttpContext.Context().App;

            if (app != null)
            {
                var appId = app.Id.ToString();

                if (FilterDefinition.Weight > 0)
                {
                    using (Profiler.Trace("CheckUsage"))
                    {
                        var (plan, _) = appPlansProvider.GetPlanForApp(app);

                        var usage = await usageTracker.GetMonthlyWeightAsync(appId, DateTime.Today);

                        if (plan.BlockingApiCalls >= 0 && usage > plan.BlockingApiCalls)
                        {
                            context.Result = new StatusCodeResult(429);
                            return;
                        }
                    }
                }
            }

            await next();
        }
    }
}
