// ==========================================================================
//  Envelope.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Infrastructure.EventSourcing
{
    public static class Envelope
    {
        public static Envelope<IEvent> Create<TPayload>(TPayload payload) where TPayload : IEvent
        {
            var eventId = Guid.NewGuid();

            var envelope =
                new Envelope<IEvent>(payload)
                    .SetEventId(eventId)
                    .SetTimestamp(SystemClock.Instance.GetCurrentInstant());

            return envelope;
        }
    }
}
