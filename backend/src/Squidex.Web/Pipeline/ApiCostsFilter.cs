// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Infrastructure;

namespace Squidex.Web.Pipeline;

public sealed class ApiCostsFilter : IAsyncActionFilter, IFilterContainer
{
    private readonly IUsageGate usageGate;

    public ApiCostsFilter(IUsageGate usageGate)
    {
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
                using (Telemetry.Activities.StartActivity("CheckUsage"))
                {
                    var (_, clientId) = context.HttpContext.User.GetClient();

                    var isBlocked = await usageGate.IsBlockedAsync(app, clientId, DateTime.Today, context.HttpContext.RequestAborted);

                    if (isBlocked)
                    {
                        context.Result = new StatusCodeResult(429);
                        return;
                    }
                }
            }

            context.HttpContext.Response.Headers.Add("X-Costs", FilterDefinition.Costs.ToString(CultureInfo.InvariantCulture));
        }

        await next();
    }
}
