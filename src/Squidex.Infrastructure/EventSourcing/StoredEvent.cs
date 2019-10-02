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
        public string StreamName { get; }

        public string EventPosition { get; }

        public long EventStreamNumber { get; }

        public EventData Data { get; }

        public StoredEvent(string streamName, string eventPosition, long eventStreamNumber, EventData data)
        {
            Guard.NotNullOrEmpty(streamName);
            Guard.NotNullOrEmpty(eventPosition);
            Guard.NotNull(data);

            Data = data;

            EventPosition = eventPosition;
            EventStreamNumber = eventStreamNumber;

            StreamName = streamName;
        }
    }
}
