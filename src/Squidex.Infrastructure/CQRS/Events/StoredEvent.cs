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
        private readonly string eventPosition;
        private readonly int eventStreamNumber;
        private readonly EventData data;

        public string EventPosition
        {
            get { return eventPosition; }
        }

        public EventData Data
        {
            get { return data; }
        }

        public int EventStreamNumber
        {
            get { return eventStreamNumber; }
        }

        public StoredEvent(string eventPosition, int eventStreamNumber, EventData data)
        {
            Guard.NotNullOrEmpty(eventPosition, nameof(eventPosition));
            Guard.NotNull(data, nameof(data));

            this.data = data;
            this.eventPosition = eventPosition;
            this.eventStreamNumber = eventStreamNumber;
        }
    }
}
