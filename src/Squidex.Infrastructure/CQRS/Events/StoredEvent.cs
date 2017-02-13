// ==========================================================================
//  StoredEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class StoredEvent
    {
        private readonly long eventNumber;
        private readonly EventData data;

        public long EventNumber
        {
            get { return eventNumber; }
        }

        public EventData Data
        {
            get { return data; }
        }

        public StoredEvent(long eventNumber, EventData data)
        {
            Guard.NotNull(data, nameof(data));

            this.data = data;

            this.eventNumber = eventNumber;
        }
    }
}
