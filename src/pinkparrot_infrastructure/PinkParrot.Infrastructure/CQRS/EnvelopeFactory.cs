// ==========================================================================
//  EnvelopeFactory.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;
using PinkParrot.Infrastructure.CQRS.Events;

namespace PinkParrot.Infrastructure.CQRS
{
    public static class EnvelopeFactory
    {
        public static Envelope<IEvent> ForEvent(IEvent @event, Guid aggregateId)
        {
            return new Envelope<IEvent>(@event)
                .SetAggregateId(aggregateId)
                .SetEventId(aggregateId)
                .SetTimestamp(SystemClock.Instance.GetCurrentInstant());
        }
    }
}
