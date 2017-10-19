// ==========================================================================
//  AuthenticationUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Builder;

namespace Squidex.Config.Identity
{
    public static class AuthenticationUsage
    {
        public static IApplicationBuilder UseMyAuthentication(this IApplicationBuilder app)
        {
            app.UseAuthentication();

            return app;
        }
    }
}
