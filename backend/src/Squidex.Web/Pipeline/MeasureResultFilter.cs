// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Infrastructure;

namespace Squidex.Web.Pipeline
{
    public sealed class MeasureResultFilter : IAsyncResultFilter, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            using (Telemetry.Activities.StartActivity("ExecuteAction"))
            {
                await next();
            }
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            using (Telemetry.Activities.StartActivity("ExecuteResult"))
            {
                await next();
            }
        }
    }
}
