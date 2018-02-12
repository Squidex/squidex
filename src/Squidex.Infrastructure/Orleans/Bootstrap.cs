// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class Bootstrap<T> : IRunnable where T : IBackgroundGrain
    {
        private readonly IGrainFactory grainFactory;

        public Bootstrap(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public void Run()
        {
            var grain = grainFactory.GetGrain<T>("Default");

            grain.ActivateAsync().Forget();
        }
    }
}
