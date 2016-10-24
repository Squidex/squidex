// ==========================================================================
//  InfrastructureUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Squidex.Pipeline;

namespace Squidex.Configurations.Domain
{
    public static class InfrastructureUsage
    {
        public static IApplicationBuilder UseMyApps(this IApplicationBuilder app)
        {
            app.UseMiddleware<AppMiddleware>();

            return app;
        }
    }
}
