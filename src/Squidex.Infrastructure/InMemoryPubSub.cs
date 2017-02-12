// ==========================================================================
//  InMemoryInvalidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace Squidex.Infrastructure
{
    public sealed class InMemoryPubSub : IPubSub
    {
        private readonly ConcurrentDictionary<string, Subject<string>> subjects = new ConcurrentDictionary<string, Subject<string>>();

        public void Publish(string channelName, string token, bool notifySelf)
        {
            if (notifySelf)
            {
                subjects.GetOrAdd(channelName, k => new Subject<string>()).OnNext(token);
            }
        }

        public IDisposable Subscribe(string channelName, Action<string> handler)
        {
            return subjects.GetOrAdd(channelName, k => new Subject<string>()).Subscribe(handler);
        }
    }
}
