// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Squidex.Infrastructure;

namespace Squidex.Config.Orleans
{
    public static class OrleansServices
    {
        public static void AddOrleansSilo(this IServiceCollection services)
        {
            services.AddSingletonAs<SiloWrapper>()
                .As<IInitializable>()
                .AsSelf();

            services.AddServicesForSelfHostedDashboard(null, options =>
            {
                options.HideTrace = true;
            });

            services.AddSingletonAs(c => c.GetRequiredService<SiloWrapper>().Client)
                .As<IClusterClient>();

            services.AddSingletonAs(c => c.GetRequiredService<SiloWrapper>().Client)
                .As<IGrainFactory>();
        }
    }
}
