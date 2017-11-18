// ==========================================================================
//  Extensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Squidex.Areas.IdentityServer.Controllers
{
    [Area("IdentityServer")]
    public abstract class IdentityServerController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.PathBase.StartsWithSegments("/identity-server"))
            {
                context.Result = new RedirectResult("/");
            }
        }
    }
}
