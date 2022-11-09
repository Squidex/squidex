// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using EventStore.Client;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Diagnostics;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.EventSourcing.Consume;
using Squidex.Infrastructure.States;

namespace Squidex.Config.Domain;

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
                        var mongoClient = StoreServices.GetMongoClient(mongoConfiguration);
                        var mongoDatabase = mongoClient.GetDatabase(mongoDatabaseName);

                        return new MongoEventStore(mongoDatabase, c.GetRequiredService<IEventNotifier>());
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

        services.AddTransientAs<Rebuilder>()
            .AsSelf();

        services.AddSingletonAs<EventConsumerManager>()
            .As<IEventConsumerManager>();

        services.AddSingletonAs<DefaultEventStreamNames>()
            .As<IEventStreamNames>();

        services.AddSingletonAs<DefaultEventFormatter>()
            .As<IEventFormatter>();

        services.AddSingletonAs<NoopEventNotifier>()
            .As<IEventNotifier>();

        services.AddSingleton<Func<IEventConsumer, EventConsumerProcessor>>(
            sb => c => ActivatorUtilities.CreateInstance<EventConsumerProcessor>(sb, c));
    }
}
