// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using EventStore.Client;
using Squidex.Events.GetEventStore;
using Squidex.Hosting.Configuration;
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
            ["Sql"] = () =>
            {
                if (!string.Equals(config.GetValue<string>("store:type"), "Sql", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ConfigurationException(
                        new ConfigurationError(
                            "Sql event store is only allowed, when 'store:type' is also set to 'Sql'.",
                            "messaging:type"));
                }

                services.AddSquidexEntityFrameworkEventStore(config);
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
