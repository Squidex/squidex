// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Orleans.Runtime;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public sealed class EventConsumerBootstrap : ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly IGrainFactory grainFactory;

        public EventConsumerBootstrap(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(SiloLifecycleStage.SiloActive, ct =>
            {
                var grain = grainFactory.GetGrain<IEventConsumerManagerGrain>("Default");

                return grain.ActivateAsync();
            });
        }
    }
}
