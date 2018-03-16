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
    public sealed class OrleansEventNotifier : IEventNotifier, IInitializable
    {
        private readonly IGrainFactory factory;
        private IEventConsumerManagerGrain eventConsumerManagerGrain;

        public OrleansEventNotifier(IGrainFactory factory)
        {
            Guard.NotNull(factory, nameof(factory));

            this.factory = factory;
        }

        public void Initialize()
        {
            eventConsumerManagerGrain = factory.GetGrain<IEventConsumerManagerGrain>("Default");
        }

        public void NotifyEventsStored(string streamName)
        {
            eventConsumerManagerGrain?.ActivateAsync(streamName);
        }

        public IDisposable Subscribe(Action<string> handler)
        {
            return null;
        }
    }
}
