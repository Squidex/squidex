// ==========================================================================
//  OrleansEventNotifier.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Orleans;
using Squidex.Infrastructure.CQRS.Events.Orleans.Grains;

namespace Squidex.Infrastructure.CQRS.Events.Orleans
{
    public sealed class OrleansEventNotifier : IEventNotifier
    {
        private readonly IEventConsumerRegistryGrain eventConsumerRegistryGrain;

        public OrleansEventNotifier(IGrainFactory factory)
        {
            Guard.NotNull(factory, nameof(factory));

            eventConsumerRegistryGrain = factory.GetGrain<IEventConsumerRegistryGrain>("Default");
        }

        public void NotifyEventsStored(string streamName)
        {
            eventConsumerRegistryGrain.ActivateAsync(streamName);
        }
    }
}
