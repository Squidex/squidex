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
        private const int NumTries = 10;
        private readonly IGrainFactory grainFactory;

        public Bootstrap(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            for (var i = 1; i <= NumTries; i++)
            {
                try
                {
                    var grain = grainFactory.GetGrain<T>("Default");

                    await grain.ActivateAsync();

                    return;
                }
                catch (OrleansException)
                {
                    if (i == NumTries)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
