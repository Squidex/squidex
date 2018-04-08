// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using EventStore.ClientAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.States;

namespace Squidex.Config.Domain
{
    public static class EventStoreServices
    {
        public static void AddMyEventStoreServices(this IServiceCollection services, IConfiguration config)
        {
            config.ConfigureByOption("eventStore:type", new Options
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
                        .As<IInitializable>()
                        .As<IEventStore>();
                },
                ["GetEventStore"] = () =>
                {
                    var eventStoreConfiguration = config.GetRequiredValue("eventStore:getEventStore:configuration");
                    var eventStoreProjectionHost = config.GetRequiredValue("eventStore:getEventStore:projectionHost");
                    var eventStorePrefix = config.GetValue<string>("eventStore:getEventStore:prefix");

                    var connection = EventStoreConnection.Create(eventStoreConfiguration);

                    services.AddSingletonAs(c => new GetEventStore(connection, eventStorePrefix, eventStoreProjectionHost))
                        .As<IInitializable>()
                        .As<IEventStore>();
                }
            });

            services.AddSingletonAs<OrleansEventNotifier>()
                .As<IEventNotifier>();

            services.AddSingletonAs<DefaultStreamNameResolver>()
                .As<IStreamNameResolver>();

            services.AddSingletonAs<DefaultEventDataFormatter>()
                .As<IEventDataFormatter>();

            services.AddSingletonAs(c =>
            {
                var allEventConsumers = c.GetServices<IEventConsumer>();

                return new EventConsumerFactory(n => allEventConsumers.FirstOrDefault(x => x.Name == n));
            });
        }
    }
}
