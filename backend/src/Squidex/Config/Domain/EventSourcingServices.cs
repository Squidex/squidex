// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using EventStore.Client;
using EventStore.ClientAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Diagnostics;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.States;

namespace Squidex.Config.Domain
{
    public static class EventSourcingServices
    {
        public static void AddSquidexEventSourcing(this IServiceCollection services, IConfiguration config)
        {
            config.ConfigureByOption("eventStore:type", new Alternatives
            {
                ["MongoDb"] = () =>
                {
                    var mongoConfiguration = config.GetRequiredValue("eventStore:mongoDb:configuration");
                    var mongoDatabaseName = config.GetRequiredValue("eventStore:mongoDb:database");

                    services.AddSingletonAs(c =>
                        {
                            var mongoClient = Singletons<IMongoClient>.GetOrAdd(mongoConfiguration, s => new MongoClient(s));
                            var mongDatabase = mongoClient.GetDatabase(mongoDatabaseName);

                            return new MongoEventStore(mongDatabase, c.GetRequiredService<IEventNotifier>());
                        })
                        .As<IEventStore>();
                },
                ["GetEventStore"] = () =>
                {
                    var configuration = config.GetRequiredValue("eventStore:getEventStore:configuration");

                    services.AddSingletonAs(_ => EventStoreClientSettings.Create(configuration))
                        .AsSelf();

                    services.AddSingletonAs<GetEventStore>()
                        .As<IEventStore>();

                    services.AddHealthChecks()
                        .AddCheck<GetEventStoreHealthCheck>("EventStore", tags: new[] { "node" });
                }
            });

            services.AddSingletonAs<OrleansEventNotifier>()
                .As<IEventNotifier>();

            services.AddTransientAs<Rebuilder>()
                .AsSelf();

            services.AddSingletonAs<DefaultStreamNameResolver>()
                .As<IStreamNameResolver>();

            services.AddSingletonAs<DefaultEventDataFormatter>()
                .As<IEventDataFormatter>();

            services.AddSingletonAs(c =>
            {
                var allEventConsumers = c.GetServices<IEventConsumer>();

                return new EventConsumerFactory(n => allEventConsumers.First(x => x.Name == n));
            });
        }
    }
}
