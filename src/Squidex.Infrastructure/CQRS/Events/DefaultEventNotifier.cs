// ==========================================================================
//  DefaultEventNotifier.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class DefaultEventNotifier : IEventNotifier
    {
        private static readonly string ChannelName = typeof(DefaultEventNotifier).Name;

        private readonly IPubSub pubsub;

        public DefaultEventNotifier(IPubSub pubsub)
        {
            Guard.NotNull(pubsub, nameof(pubsub));

            this.pubsub = pubsub;
        }

        public void NotifyEventsStored(string streamName)
        {
            pubsub.Publish(ChannelName, streamName, true);
        }

        public IDisposable Subscribe(Action<string> handler)
        {
            return pubsub.Subscribe(ChannelName, x => handler?.Invoke(x));
        }
    }
}
