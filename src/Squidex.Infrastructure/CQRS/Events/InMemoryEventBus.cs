// ==========================================================================
//  InMemoryEventBus.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reactive.Subjects;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class InMemoryEventBus : IEventPublisher, IEventStream
    {
        private readonly Subject<EventData> subject = new Subject<EventData>();

        public void Dispose()
        {
        }

        public void Publish(EventData eventData)
        {
            subject.OnNext(eventData);
        }

        public void Connect(string queueName, Action<EventData> received)
        {
            subject.Subscribe(received);
        }
    }
}
