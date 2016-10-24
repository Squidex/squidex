// ==========================================================================
//  EventStoreUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.CQRS.EventStore;

namespace Squidex.Configurations.EventStore
{
    public static class EventStoreUsage
    {
        public static IApplicationBuilder UseMyEventStore(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<IOptions<MyEventStoreOptions>>().Value;

            app.ApplicationServices.GetService<EventStoreBus>().Subscribe(options.Prefix);

            return app;
        }
    }
}
