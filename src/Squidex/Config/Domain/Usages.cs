// ==========================================================================
//  Usages.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Read.Apps.Repositories;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Domain.Apps.Read.Schemas.Services;
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
            var services = app.ApplicationServices;

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

            var appRepository = services.GetService<IAppRepository>();
            var appProvider = services.GetService<IAppProvider>();

            if (appProvider != null)
            {
                appRepository?.SubscribeOnChanged(appProvider.Invalidate);
            }

            var schemaRepository = services.GetService<ISchemaRepository>();
            var schemaProvider = services.GetService<ISchemaProvider>();

            if (schemaProvider != null)
            {
                schemaRepository?.SubscribeOnChanged(schemaProvider.Invalidate);
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
