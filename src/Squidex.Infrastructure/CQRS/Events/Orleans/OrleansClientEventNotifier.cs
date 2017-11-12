// ==========================================================================
//  OrleansClientEventNotifier.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Orleans;
using Squidex.Infrastructure.CQRS.Events.Orleans.Grains;

namespace Squidex.Infrastructure.CQRS.Events.Orleans
{
    public sealed class OrleansClientEventNotifier : IEventNotifier
    {
        private readonly IEventConsumerRegistryGrain eventConsumerRegistryGrain;

        public OrleansClientEventNotifier(IClusterClient orleans)
        {
            Guard.NotNull(orleans, nameof(orleans));

            eventConsumerRegistryGrain = orleans.GetGrain<IEventConsumerRegistryGrain>("Default");
        }

        public void NotifyEventsStored(string streamName)
        {
            eventConsumerRegistryGrain.ActivateAsync(streamName);
        }
    }
}
