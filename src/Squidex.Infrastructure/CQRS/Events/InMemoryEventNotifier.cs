// ==========================================================================
//  InMemoryEventNotifier.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reactive.Subjects;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class InMemoryEventNotifier : IEventNotifier
    {
        private readonly Subject<object> subject = new Subject<object>();

        public void NotifyEventsStored()
        {
            subject.OnNext(null);
        }

        public void Subscribe(Action handler)
        {
            subject.Subscribe(_ => handler());
        }
    }
}
