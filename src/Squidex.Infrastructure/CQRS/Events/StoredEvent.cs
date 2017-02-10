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
        public long EventNumber { get; }

        public EventData Data { get; }

        public StoredEvent(long eventNumber, EventData data)
        {
            Guard.NotNull(data, nameof(data));

            EventNumber = eventNumber;

            Data = data;
        }
    }
}
