// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class Bootstrap<T> : IStartupTask where T : IBackgroundGrain
    {
        private readonly IGrainFactory grainFactory;

        public Bootstrap(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public Task Execute(CancellationToken cancellationToken)
        {
            var grain = grainFactory.GetGrain<T>("Default");

            return grain.ActivateAsync();
        }
    }
}
