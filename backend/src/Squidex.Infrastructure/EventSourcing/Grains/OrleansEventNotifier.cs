// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Orleans;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public sealed class OrleansEventNotifier : IEventNotifier
    {
        private readonly Lazy<IEventConsumerManagerGrain> eventConsumerManagerGrain;

        public OrleansEventNotifier(IGrainFactory factory)
        {
            eventConsumerManagerGrain = new Lazy<IEventConsumerManagerGrain>(() =>
            {
                return factory.GetGrain<IEventConsumerManagerGrain>(SingleGrain.Id);
            });
        }

        public void NotifyEventsStored(string streamName)
        {
            eventConsumerManagerGrain.Value.ActivateAsync(streamName);
        }
    }
}
