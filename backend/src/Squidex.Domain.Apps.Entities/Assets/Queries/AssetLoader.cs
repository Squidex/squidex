// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure;
using Squidex.Log;

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

        public async Task<IAssetEntity> GetAsync(DomainId appId, DomainId id, long version)
        {
            using (Profiler.TraceMethod<AssetLoader>())
            {
                var key = DomainId.Combine(appId, id);

                var assetGrain = grainFactory.GetGrain<IAssetGrain>(key.ToString());
                var assetState = await assetGrain.GetStateAsync(version);

                var asset = assetState.Value;

                if (asset == null || asset.Version <= EtagVersion.Empty || (version > EtagVersion.Any && asset.Version != version))
                {
                    throw new DomainObjectNotFoundException(id.ToString());
                }

                return asset;
            }
        }
    }
}
