// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Squidex.Shared.Identity;

namespace Squidex.Areas.OrleansDashboard.Middlewares
{
    public sealed class OrleansDashboardAuthenticationMiddleware
    {
        private readonly RequestDelegate next;

        public OrleansDashboardAuthenticationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var authentication = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authentication.Succeeded || !authentication.Principal.IsInRole(SquidexRoles.Administrator))
            {
                await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
                {
                    RedirectUri = context.Request.PathBase + context.Request.Path
                });
            }
            else
            {
                await next(context);
            }
        }
    }
}
