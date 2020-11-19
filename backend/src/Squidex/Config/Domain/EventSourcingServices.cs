// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using EventStore.ClientAPI;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;
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
                ["CosmosDb"] = () =>
                {
                    var cosmosDbConfiguration = config.GetRequiredValue("eventStore:cosmosDB:configuration");
                    var cosmosDbMasterKey = config.GetRequiredValue("eventStore:cosmosDB:masterKey");
                    var cosmosDbDatabase = config.GetRequiredValue("eventStore:cosmosDB:database");

                    services.AddSingletonAs(c => new DocumentClient(new Uri(cosmosDbConfiguration), cosmosDbMasterKey, c.GetRequiredService<JsonSerializerSettings>()))
                        .AsSelf();

                    services.AddSingletonAs(c => new CosmosDbEventStore(
                            c.GetRequiredService<DocumentClient>(),
                            cosmosDbMasterKey,
                            cosmosDbDatabase,
                            c.GetRequiredService<JsonSerializerSettings>()))
                        .As<IEventStore>();

                    services.AddHealthChecks()
                        .AddCheck<CosmosDbHealthCheck>("CosmosDB", tags: new[] { "node" });
                },
                ["GetEventStore"] = () =>
                {
                    var eventStoreConfiguration = config.GetRequiredValue("eventStore:getEventStore:configuration");
                    var eventStoreProjectionHost = config.GetRequiredValue("eventStore:getEventStore:projectionHost");
                    var eventStorePrefix = config.GetValue<string>("eventStore:getEventStore:prefix");

                    services.AddSingletonAs(_ => EventStoreConnection.Create(eventStoreConfiguration))
                        .As<IEventStoreConnection>();

                    services.AddSingletonAs(c => new GetEventStore(
                            c.GetRequiredService<IEventStoreConnection>(),
                            c.GetRequiredService<IJsonSerializer>(),
                            eventStorePrefix,
                            eventStoreProjectionHost))
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
