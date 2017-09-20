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
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Config.Domain
{
    public static class Usages
    {
        public static IApplicationBuilder UseMyEventStore(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<EventConsumerCleaner>().CleanAsync().Wait();

            var catchConsumers = app.ApplicationServices.GetServices<IEventConsumer>();

            foreach (var catchConsumer in catchConsumers)
            {
                var receiver = app.ApplicationServices.GetService<EventReceiver>();

                receiver?.Subscribe(catchConsumer);
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
