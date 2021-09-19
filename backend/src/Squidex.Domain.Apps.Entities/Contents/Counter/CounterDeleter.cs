// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Apps;

namespace Squidex.Domain.Apps.Entities.Contents.Counter
{
    public sealed class CounterDeleter : IDeleter
    {
        private readonly IGrainFactory grainFactory;

        public CounterDeleter(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public Task DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            var grain = grainFactory.GetGrain<ICounterGrain>(app.Id.ToString());

            return grain.ClearAsync();
        }
    }
}
