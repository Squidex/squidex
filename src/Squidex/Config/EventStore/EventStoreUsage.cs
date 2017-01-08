// ==========================================================================
//  EventStoreUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Config.EventStore
{
    public static class EventStoreUsage
    {
        public static IApplicationBuilder UseMyEventStore(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<EventReceiver>().Subscribe();

            return app;
        }
    }
}
