// ==========================================================================
//  InfrastructureUsage.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using PinkParrot.Pipeline;

namespace PinkParrot.Configurations
{
    public static class InfrastructureUsage
    {
        public static void UseTenants(this IApplicationBuilder app)
        {
            app.UseMiddleware<TenantMiddleware>();
        }
    }
}
