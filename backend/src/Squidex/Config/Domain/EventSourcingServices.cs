// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using EventStore.Client;
using Squidex.Events.GetEventStore;
using Squidex.Infrastructure.Commands;
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
                services.AddSquidexMongoEventStore(config);
            },
            ["GetEventStore"] = () =>
            {
                var configuration = config.GetRequiredValue("eventStore:getEventStore:configuration");

                services.AddSingletonAs(_ => EventStoreClientSettings.Create(configuration))
                    .AsSelf();

                services.AddGetEventStore(config);
            },
        });

        services.AddTransientAs<Rebuilder>()
            .AsSelf();

        services.AddSingletonAs<EventConsumerManager>()
            .As<IEventConsumerManager>();

        services.AddSingletonAs<DefaultEventStreamNames>()
            .As<IEventStreamNames>();

        services.AddSingletonAs<DefaultEventFormatter>()
            .As<IEventFormatter>();

        services.AddSingleton<Func<IEventConsumer, EventConsumerProcessor>>(
            sb => c => ActivatorUtilities.CreateInstance<EventConsumerProcessor>(sb, c));
    }
}
