// ==========================================================================
//  InMemoryPubSub.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Squidex.Infrastructure
{
    public sealed class InMemoryPubSub : IPubSub
    {
        private readonly Subject<object> subject = new Subject<object>();

        public void Publish<T>(T value, bool notifySelf)
        {
            if (notifySelf)
            {
                subject.OnNext(value);
            }
        }

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            return subject.Where(x => x is T).OfType<T>().Subscribe(handler);
        }
    }
}
