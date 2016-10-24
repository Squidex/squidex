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
        public static Envelope<IEvent> ForEvent(IEvent @event, IAggregate aggregate)
        {
            var eventId = Guid.NewGuid();

            var envelope =
                new Envelope<IEvent>(@event)
                    .SetAggregateId(aggregate.Id)
                    .SetEventId(eventId)
                    .SetTimestamp(SystemClock.Instance.GetCurrentInstant());

            var appAggregate = aggregate as IAppAggregate;

            if (appAggregate != null)
            {
                envelope = envelope.SetAppId(appAggregate.AppId);
            }

            return envelope;
        }
    }
}
