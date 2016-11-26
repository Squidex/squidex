// ==========================================================================
//  EventWrapper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using EventStore.ClientAPI;

namespace Squidex.Infrastructure.CQRS.EventStore
{
    internal sealed class EventWrapper : IReceivedEvent
    {
        private readonly ResolvedEvent @event;

        public int EventNumber
        {
            get { return @event.OriginalEventNumber; }
        }

        public string EventType
        {
            get { return @event.Event.EventType; }
        }

        public byte[] Metadata
        {
            get { return @event.Event.Metadata; }
        }

        public byte[] Payload
        {
            get { return @event.Event.Data; }
        }

        public DateTime Created
        {
            get { return @event.Event.Created; }
        }

        public EventWrapper(ResolvedEvent @event)
        {
            this.@event = @event;
        }
    }
}
