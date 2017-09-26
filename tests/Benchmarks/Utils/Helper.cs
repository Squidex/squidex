// ==========================================================================
//  Helper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.CQRS.Events;

namespace Benchmarks.Utils
{
    public static class Helper
    {
        public static EventData CreateEventData()
        {
            return new EventData { EventId = Guid.NewGuid(), Metadata = "EventMetdata", Payload = "EventPayload", Type = "MyEvent" };
        }
    }
}
