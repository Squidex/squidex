// ==========================================================================
//  ClientServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime.Configuration;
using Squidex.Domain.Users.DataProtection.Orleans.Grains.Implementations;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.Events.Orleans;
using Squidex.Infrastructure.CQRS.Events.Orleans.Grains.Implementation;

namespace Squidex.Config.Orleans
{
    public static class ClientServices
    {
        public static void AddAppClient(this IServiceCollection services)
        {
            services.AddSingleton<OrleansClientEventNotifier>()
                .As<IEventNotifier>();

            services.AddSingleton(c =>
            {
                var client = new ClientBuilder()
                    .UseConfiguration(ClientConfiguration.LocalhostSilo())
                    .AddApplicationPartsFromReferences(typeof(EventConsumerGrain).Assembly)
                    .AddApplicationPartsFromReferences(typeof(XmlRepositoryGrain).Assembly)
                    .Build();

                client.Connect().Wait();

                return client;
            });
        }
    }
}
