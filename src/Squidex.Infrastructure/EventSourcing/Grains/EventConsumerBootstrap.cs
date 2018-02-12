// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Orleans.Runtime;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public sealed class EventConsumerBootstrap : IRunnable
    {
        private readonly IGrainFactory grainFactory;

        public EventConsumerBootstrap(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public void Run()
        {
            var grain = grainFactory.GetGrain<IEventConsumerManagerGrain>("Default");

            grain.ActivateAsync().Forget();
        }
    }
}
