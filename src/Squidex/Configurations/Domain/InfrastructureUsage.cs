// ==========================================================================
//  InfrastructureUsage.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using PinkParrot.Pipeline;

namespace PinkParrot.Configurations.Domain
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
