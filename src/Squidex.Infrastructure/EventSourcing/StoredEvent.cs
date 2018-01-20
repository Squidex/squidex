// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class StoredEvent
    {
        private readonly string eventPosition;
        private readonly long eventStreamNumber;
        private readonly EventData data;

        public string EventPosition
        {
            get { return eventPosition; }
        }

        public long EventStreamNumber
        {
            get { return eventStreamNumber; }
        }

        public EventData Data
        {
            get { return data; }
        }

        public StoredEvent(string eventPosition, long eventStreamNumber, EventData data)
        {
            Guard.NotNullOrEmpty(eventPosition, nameof(eventPosition));
            Guard.NotNull(data, nameof(data));

            this.data = data;
            this.eventPosition = eventPosition;
            this.eventStreamNumber = eventStreamNumber;
        }
    }
}
