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
using NodaTime;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Web.Pipeline
{
    public sealed class ApiCostsFilter : IAsyncActionFilter, IFilterContainer
    {
        private readonly IAppPlansProvider appPlansProvider;
        private readonly IAppLogStore appLogStore;
        private readonly IUsageTracker usageTracker;
        private readonly IClock clock;

        public ApiCostsFilter(IAppLogStore appLogStore, IAppPlansProvider appPlansProvider, IUsageTracker usageTracker, IClock clock)
        {
            Guard.NotNull(appLogStore);
            Guard.NotNull(appPlansProvider);
            Guard.NotNull(usageTracker);
            Guard.NotNull(clock);

            this.appLogStore = appLogStore;
            this.appPlansProvider = appPlansProvider;

            this.usageTracker = usageTracker;

            this.clock = clock;
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

                        var usage = await usageTracker.GetMonthlyCallsAsync(appId, DateTime.Today);

                        if (plan.MaxApiCalls >= 0 && usage > plan.MaxApiCalls * 1.1)
                        {
                            context.Result = new StatusCodeResult(429);
                            return;
                        }
                    }
                }

                var watch = ValueStopwatch.StartNew();

                try
                {
                    await next();
                }
                finally
                {
                    var elapsedMs = watch.Stop();

                    await appLogStore.LogAsync(app.Id, clock.GetCurrentInstant(),
                        context.HttpContext.Request.Method,
                        context.HttpContext.Request.Path,
                        context.HttpContext.User.OpenIdSubject(),
                        context.HttpContext.User.OpenIdClientId(),
                        elapsedMs,
                        FilterDefinition.Weight);

                    if (FilterDefinition.Weight > 0)
                    {
                        await usageTracker.TrackAsync(appId, context.HttpContext.User.OpenIdClientId(), FilterDefinition.Weight, elapsedMs);
                    }
                }
            }
            else
            {
                await next();
            }
        }
    }
}
