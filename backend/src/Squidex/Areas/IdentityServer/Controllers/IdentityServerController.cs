// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Squidex.Areas.IdentityServer.Controllers
{
    [Area("IdentityServer")]
    public abstract class IdentityServerController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            if (!request.PathBase.HasValue || request.PathBase.Value?.EndsWith("/identity-server", StringComparison.OrdinalIgnoreCase) != true)
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}
