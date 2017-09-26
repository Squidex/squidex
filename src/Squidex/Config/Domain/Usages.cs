﻿// ==========================================================================
//  Usages.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Actors;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.Events.Actors;

namespace Squidex.Config.Domain
{
    public static class Usages
    {
        public static IApplicationBuilder UseMyEventStore(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<EventConsumerCleaner>().CleanAsync().Wait();

            var consumers = app.ApplicationServices.GetServices<IEventConsumer>();

            foreach (var consumer in consumers)
            {
                var actor = app.ApplicationServices.GetService<EventConsumerActor>();

                if (actor != null)
                {
                    actor.SubscribeAsync(consumer);

                    app.ApplicationServices.GetService<RemoteActors>().Connect(consumer.Name, actor);
                }
            }

            return app;
        }

        public static IApplicationBuilder TestExternalSystems(this IApplicationBuilder app)
        {
            var systems = app.ApplicationServices.GetRequiredService<IEnumerable<IExternalSystem>>();

            foreach (var system in systems)
            {
                system.Connect();
            }

            return app;
        }
    }
}
