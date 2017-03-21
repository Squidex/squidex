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

        private readonly IPubSub invalidator;

        public DefaultEventNotifier(IPubSub invalidator)
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
