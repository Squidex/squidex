// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrate_01.Migrations
{
    public sealed class CreateAssetSlugs : IMigration
    {
        private readonly ISnapshotStore<AssetState, Guid> stateForAssets;

        public CreateAssetSlugs(ISnapshotStore<AssetState, Guid> stateForAssets)
        {
            this.stateForAssets = stateForAssets;
        }

        public Task UpdateAsync()
        {
            return stateForAssets.ReadAllAsync(async (state, version) =>
            {
                state.Slug = state.FileName.ToAssetSlug();

                await stateForAssets.WriteAsync(state.Id, state, version, version);
            });
        }
    }
}
