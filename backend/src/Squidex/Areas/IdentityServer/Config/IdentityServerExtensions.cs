// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;

namespace Squidex.Areas.IdentityServer.Config
{
    public static class IdentityServerExtensions
    {
        public static IApplicationBuilder UseSquidexIdentityServer(this IApplicationBuilder app)
        {
             app.UseIdentityServer();

             return app;
        }
    }
}
