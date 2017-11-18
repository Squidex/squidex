// ==========================================================================
//  EventStoreServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using EventStore.ClientAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Config.Domain
{
    public static class EventStoreServices
    {
        public static void AddMyEventStoreServices(this IServiceCollection services, IConfiguration config)
        {
            var consumeEvents = config.GetOptionalValue("eventStore:consume", false);

            if (!consumeEvents)
            {
                return;
            }

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
                        .As<IExternalSystem>()
                        .As<IEventStore>();
                },
                ["GetEventStore"] = () =>
                {
                    var eventStoreConfiguration = config.GetRequiredValue("eventStore:getEventStore:configuration");
                    var eventStoreProjectionHost = config.GetRequiredValue("eventStore:getEventStore:projectionHost");
                    var eventStorePrefix = config.GetValue<string>("eventStore:getEventStore:prefix");

                    var connection = EventStoreConnection.Create(eventStoreConfiguration);

                    services.AddSingletonAs(c => new GetEventStore(connection, eventStorePrefix, eventStoreProjectionHost))
                        .As<IExternalSystem>()
                        .As<IEventStore>();
                }
            });
        }
    }
}
