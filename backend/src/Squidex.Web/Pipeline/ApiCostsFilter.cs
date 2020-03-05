﻿// ==========================================================================
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

namespace Squidex.Web.Pipeline
{
    public sealed class ApiCostsFilter : IAsyncActionFilter, IFilterContainer
    {
        private readonly UsageGate usageGate;

        public ApiCostsFilter(UsageGate usageGate)
        {
            Guard.NotNull(usageGate);

            this.usageGate = usageGate;
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
                if (FilterDefinition.Costs > 0)
                {
                    using (Profiler.Trace("CheckUsage"))
                    {
                        var isBlocked = await usageGate.IsBlockedAsync(app, DateTime.Today);

                        if (isBlocked)
                        {
                            context.Result = new StatusCodeResult(429);
                            return;
                        }
                    }
                }

                context.HttpContext.Response.Headers.Add("X-Costs", FilterDefinition.Costs.ToString());
            }

            await next();
        }
    }
}
