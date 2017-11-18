// ==========================================================================
//  ClientServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime.Configuration;
using Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations;
using Squidex.Domain.Users.DataProtection.Orleans.Grains.Implementations;
using Squidex.Infrastructure.CQRS.Events.Orleans.Grains.Implementation;

namespace Squidex.Config.Orleans
{
    public static class ClientServices
    {
        public static void AddAppClient(this IServiceCollection services)
        {
            services.AddSingletonAs(c => c.GetRequiredService<IClusterClient>())
                .As<IGrainFactory>();

            services.AddServicesForSelfHostedDashboard(null, options =>
            {
                options.HideTrace = true;
            });

            services.AddSingletonAs(c =>
            {
                var configuration = ClientConfiguration.LocalhostSilo();

                var client = new ClientBuilder()
                    .UseConfiguration(ClientConfiguration.LocalhostSilo().WithJsonSerializer())
                    .UseDashboard()
                    .AddApplicationPartsFromReferences(typeof(AppStateGrain).Assembly)
                    .AddApplicationPartsFromReferences(typeof(EventConsumerGrain).Assembly)
                    .AddApplicationPartsFromReferences(typeof(XmlRepositoryGrain).Assembly)
                    .UseStaticGatewayListProvider(options =>
                    {
                        options.Gateways.Add(new Uri("gwy.tcp://127.0.0.1:40000/0"));
                    })
                    .Build();

                client.Connect().Wait();

                return client;
            });
        }

        public static ClientConfiguration WithJsonSerializer(this ClientConfiguration config)
        {
            config.SerializationProviders.Add(typeof(CustomJsonSerializer).GetTypeInfo());

            return config;
        }
    }
}
