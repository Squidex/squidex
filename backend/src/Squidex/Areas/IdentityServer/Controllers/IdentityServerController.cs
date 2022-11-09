// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Squidex.Hosting;
using Squidex.Web;

namespace Squidex.Areas.IdentityServer.Controllers;

[Area("IdentityServer")]
[Route(Constants.PrefixIdentityServer)]
public abstract class IdentityServerController : Controller
{
    public SignInManager<IdentityUser> SignInManager
    {
        get => HttpContext.RequestServices.GetRequiredService<SignInManager<IdentityUser>>();
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

        return Redirect("~/../");
    }
}
