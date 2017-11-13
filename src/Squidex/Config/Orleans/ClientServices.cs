// ==========================================================================
//  ClientServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

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
            services.AddSingleton(c => c.GetRequiredService<IClusterClient>())
                .As<IGrainFactory>();

            services.AddSingleton(c =>
            {
                var configuration = ClientConfiguration.LocalhostSilo();

                var client = new ClientBuilder()
                    .UseConfiguration(ClientConfiguration.LocalhostSilo().WithJsonSerializer())
                    .AddApplicationPartsFromReferences(typeof(AppStateGrain).Assembly)
                    .AddApplicationPartsFromReferences(typeof(EventConsumerGrain).Assembly)
                    .AddApplicationPartsFromReferences(typeof(XmlRepositoryGrain).Assembly)
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
