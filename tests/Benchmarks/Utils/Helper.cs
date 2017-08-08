// ==========================================================================
//  Helper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure.CQRS.Events;

namespace Benchmarks.Utils
{
    public static class Helper
    {
        public static EventData CreateEventData()
        {
            return new EventData { EventId = Guid.NewGuid(), Metadata = "EventMetdata", Payload = "EventPayload", Type = "MyEvent" };
        }

        public static void Warmup(this IEventStore eventStore)
        {
            eventStore.AppendEventsAsync(Guid.NewGuid(), "my-stream", new List<EventData> { CreateEventData() }).Wait();
        }
    }
}
