// ==========================================================================
//  EventStoreExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Actors;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.Events.Actors;

namespace Squidex.Config.Domain
{
    public static class EventStoreExtensions
    {
        public static void UseMyEventStore(this IServiceProvider services)
        {
            services.GetService<EventConsumerCleaner>().CleanAsync().Wait();

            var consumers = services.GetServices<IEventConsumer>();

            foreach (var consumer in consumers)
            {
                var actor = services.GetService<EventConsumerActor>();

                if (actor != null)
                {
                    actor.SubscribeAsync(consumer);

                    services.GetService<RemoteActors>().Connect(consumer.Name, actor);
                }
            }
        }
    }
}
