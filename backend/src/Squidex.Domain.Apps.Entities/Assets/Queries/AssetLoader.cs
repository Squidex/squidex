﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public sealed class AssetLoader : IAssetLoader
    {
        private readonly IGrainFactory grainFactory;

        public AssetLoader(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task<IAssetEntity?> GetAsync(DomainId appId, DomainId id, long version = EtagVersion.Any)
        {
            using (Telemetry.Activities.StartActivity("AssetLoader/GetAsync"))
            {
                var key = DomainId.Combine(appId, id);

                var assetGrain = grainFactory.GetGrain<IAssetGrain>(key.ToString());
                var assetState = await assetGrain.GetStateAsync(version);

                if (assetState == null || assetState.Version <= EtagVersion.Empty || (version > EtagVersion.Any && assetState.Version != version))
                {
                    return null;
                }

                return assetState;
            }
        }
    }
}
