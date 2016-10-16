// ==========================================================================
//  DeactivateForAppDomainAttribute.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PinkParrot.Pipeline
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
