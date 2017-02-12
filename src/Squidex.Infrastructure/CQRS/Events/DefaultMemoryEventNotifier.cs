// ==========================================================================
//  InMemoryEventNotifier.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class DefaultMemoryEventNotifier : IEventNotifier
    {
        private readonly string ChannelName = typeof(DefaultMemoryEventNotifier).Name;

        private readonly IPubSub invalidator;

        public DefaultMemoryEventNotifier(IPubSub invalidator)
        {
            Guard.NotNull(invalidator, nameof(invalidator));

            this.invalidator = invalidator;
        }

        public void NotifyEventsStored()
        {
            invalidator.Publish(ChannelName, string.Empty, true);
        }

        public void Subscribe(Action handler)
        {
            invalidator.Subscribe(ChannelName, x => handler());
        }
    }
}
