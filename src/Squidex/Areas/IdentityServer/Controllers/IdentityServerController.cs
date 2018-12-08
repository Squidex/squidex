// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
                context.Result = new NotFoundResult();
            }
        }
    }
}
