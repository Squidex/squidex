// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Squidex.Infrastructure.EventSourcing.Grains;

namespace Squidex.Config.Orleans
{
    public static class ClientServices
    {
        public static void AddAppClient(this IServiceCollection services)
        {
            services.AddSingletonAs(c => c.GetRequiredService<IClusterClient>())
                .As<IGrainFactory>();

            services.AddSingletonAs(c =>
            {
                var client = new ClientBuilder()
                    .ConfigureApplicationParts(builder =>
                    {
                        builder.AddApplicationPart(typeof(EventConsumerGrain).Assembly);
                    })
                    .UseStaticGatewayListProvider(options =>
                    {
                        options.Gateways.Add(new Uri("gwy.tcp://127.0.0.1:40000/0"));
                    })
                    .Build();

                client.Connect().Wait();

                return client;
            });
        }
    }
}
