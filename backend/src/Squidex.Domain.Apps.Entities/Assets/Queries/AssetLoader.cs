// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public sealed class AssetLoader : IAssetLoader
    {
        private readonly IGrainFactory grainFactory;

        public AssetLoader(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public async Task<IAssetEntity> GetAsync(Guid id, long version)
        {
            using (Profiler.TraceMethod<AssetLoader>())
            {
                var grain = grainFactory.GetGrain<IAssetGrain>(id);

                var content = await grain.GetStateAsync(version);

                if (content.Value == null || content.Value.Version != version)
                {
                    throw new DomainObjectNotFoundException(id.ToString(), typeof(IAssetEntity));
                }

                return content.Value;
            }
        }
    }
}
