﻿// ==========================================================================
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
    public sealed class GrainBootstrap<T> : IBackgroundProcess where T : IBackgroundGrain
    {
        private const int NumTries = 10;
        private readonly IGrainFactory grainFactory;

        public GrainBootstrap(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public async Task StartAsync(CancellationToken ct = default)
        {
            for (var i = 1; i <= NumTries; i++)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var grain = grainFactory.GetGrain<T>(SingleGrain.Id);

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

        public override string ToString()
        {
            return typeof(T).ToString();
        }
    }
}
