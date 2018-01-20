// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class DefaultEventNotifier : IEventNotifier
    {
        private static readonly string ChannelName = typeof(DefaultEventNotifier).Name;

        private readonly IPubSub pubsub;

        public sealed class EventNotification
        {
            public string StreamName { get; set; }
        }

        public DefaultEventNotifier(IPubSub pubsub)
        {
            Guard.NotNull(pubsub, nameof(pubsub));

            this.pubsub = pubsub;
        }

        public void NotifyEventsStored(string streamName)
        {
            pubsub.Publish(new EventNotification { StreamName = streamName }, true);
        }

        public IDisposable Subscribe(Action<string> handler)
        {
            return pubsub.Subscribe<EventNotification>(x => handler?.Invoke(x.StreamName));
        }
    }
}
