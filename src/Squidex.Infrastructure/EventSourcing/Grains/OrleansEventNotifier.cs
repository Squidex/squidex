// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Orleans;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public sealed class OrleansEventNotifier : IEventNotifier
    {
        private readonly IEventConsumerManagerGrain eventConsumerManagerGrain;

        public OrleansEventNotifier(IGrainFactory factory)
        {
            Guard.NotNull(factory, nameof(factory));

            eventConsumerManagerGrain = factory.GetGrain<IEventConsumerManagerGrain>("Default");
        }

        public void NotifyEventsStored(string streamName)
        {
            eventConsumerManagerGrain.WakeUpAsync(streamName);
        }

        public IDisposable Subscribe(Action<string> handler)
        {
            return null;
        }
    }
}
