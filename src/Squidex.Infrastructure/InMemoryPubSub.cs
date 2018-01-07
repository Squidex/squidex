// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Squidex.Infrastructure
{
    public sealed class InMemoryPubSub : IPubSub
    {
        private readonly Subject<object> subject = new Subject<object>();
        private readonly bool publishAlways;

        public InMemoryPubSub()
        {
        }

        public InMemoryPubSub(bool publishAlways)
        {
            this.publishAlways = publishAlways;
        }

        public void Publish<T>(T value, bool notifySelf)
        {
            if (notifySelf || publishAlways)
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
