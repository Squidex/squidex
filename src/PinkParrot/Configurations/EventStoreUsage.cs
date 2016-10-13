// ==========================================================================
//  EventStoreUsage.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PinkParrot.Infrastructure.CQRS.EventStore;

namespace PinkParrot.Configurations
{
    public static class EventStoreUsage
    {
        public static void UseEventStore(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<EventStoreOptions>>().Value;

            app.ApplicationServices.GetService<EventStoreBus>().Subscribe(options.Prefix);
        }
    }
}
