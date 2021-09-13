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
using Squidex.Shared;
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

        public async Task InvokeAsync(HttpContext context)
        {
            var authentication = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (authentication.Succeeded)
            {
                var permissions = authentication.Principal?.Claims.Permissions();

                if (permissions?.Allows(Permissions.AdminOrleans) == true)
                {
                    await next(context);
                }
                else
                {
                    context.Response.StatusCode = 403;
                }
            }
            else
            {
                await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
                {
                    RedirectUri = context.Request.PathBase + context.Request.Path
                });
            }
        }
    }
}
