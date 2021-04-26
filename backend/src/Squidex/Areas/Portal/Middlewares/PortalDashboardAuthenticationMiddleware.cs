// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;

namespace Squidex.Areas.Portal.Middlewares
{
    public sealed class PortalDashboardAuthenticationMiddleware
    {
        private readonly RequestDelegate next;

        public PortalDashboardAuthenticationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authentication = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authentication.Succeeded)
            {
                var properties = new AuthenticationProperties
                {
                    RedirectUri = context.Request.PathBase + context.Request.Path
                };

                await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, properties);
            }
            else
            {
                context.User = authentication.Principal!;

                await next(context);
            }
        }
    }
}
