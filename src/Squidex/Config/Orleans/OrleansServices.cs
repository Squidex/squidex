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
                .As<IInitializable>();
        }

        public static void AddOrleansClient(this IServiceCollection services)
        {
            services.AddServicesForSelfHostedDashboard(null, options =>
            {
                options.HideTrace = true;
            });

            services.AddSingletonAs<ClientWrapper>()
                .As<IInitializable>()
                .AsSelf();

            services.AddSingletonAs(c => c.GetRequiredService<ClientWrapper>().Client)
                .As<IClusterClient>();

            services.AddSingletonAs(c => c.GetRequiredService<ClientWrapper>().Client)
                .As<IGrainFactory>();
        }
    }
}
