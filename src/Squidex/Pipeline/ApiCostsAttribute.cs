// ==========================================================================
//  ApiWeightAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Squidex.Pipeline
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ApiCostsAttribute : ActionFilterAttribute
    {
        private readonly double weight;

        private sealed class WeightFeature : IAppTrackingWeightFeature
        {
            public double Weight { get; set; }
        }

        public ApiCostsAttribute(double weight)
        {
            this.weight = weight;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Features.Set<IAppTrackingWeightFeature>(new WeightFeature { Weight = weight });
        }
    }
}
