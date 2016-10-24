// ==========================================================================
//  DeactivateForAppDomainAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Squidex.Pipeline
{
    public sealed class DeactivateForAppDomainAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var app = context.HttpContext.Features.Get<IAppFeature>();

            if (app != null)
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}
