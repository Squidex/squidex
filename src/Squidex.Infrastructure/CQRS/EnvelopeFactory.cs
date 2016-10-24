// ==========================================================================
//  EnvelopeFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Infrastructure.CQRS
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
