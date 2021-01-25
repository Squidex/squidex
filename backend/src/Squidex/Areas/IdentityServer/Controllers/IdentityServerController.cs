﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Hosting;

namespace Squidex.Areas.IdentityServer.Controllers
{
    [Area("IdentityServer")]
    public abstract class IdentityServerController : Controller
    {
        public SignInManager<IdentityUser> SignInManager
        {
            get
            {
                return HttpContext.RequestServices.GetRequiredService<SignInManager<IdentityUser>>();
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            if (!request.PathBase.HasValue || request.PathBase.Value?.EndsWith("/identity-server", StringComparison.OrdinalIgnoreCase) != true)
            {
                context.Result = new NotFoundResult();
            }
        }

        protected IActionResult RedirectToReturnUrl(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect("~/../");
            }

            var urlGenerator = HttpContext.RequestServices.GetRequiredService<IUrlGenerator>();

            if (urlGenerator.IsAllowedHost(returnUrl))
            {
                return Redirect(returnUrl);
            }

            var interactions = HttpContext.RequestServices.GetRequiredService<IIdentityServerInteractionService>();

            if (interactions.IsValidReturnUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("~/../");
        }
    }
}
