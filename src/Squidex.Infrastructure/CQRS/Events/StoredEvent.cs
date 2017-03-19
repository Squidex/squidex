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
        private readonly long eventStreamNumber;
        private readonly EventData data;

        public long EventNumber
        {
            get { return eventNumber; }
        }

        public long EventStreamNumber
        {
            get { return eventStreamNumber; }
        }

        public EventData Data
        {
            get { return data; }
        }

        public StoredEvent(long eventNumber, long eventStreamNumber, EventData data)
        {
            Guard.NotNull(data, nameof(data));

            this.data = data;
            this.eventNumber = eventNumber;
            this.eventStreamNumber = eventStreamNumber;
        }
    }
}
