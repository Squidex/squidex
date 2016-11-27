// ==========================================================================
//  RandomErrorFilter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Squidex.Pipeline
{
    public class RandomErrorAttribute : ActionFilterAttribute
    {
        private static readonly Random random = new Random();

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (random.Next(10) < 5)
            {
                context.Result = new StatusCodeResult(500);
            }
        }
    }
}
