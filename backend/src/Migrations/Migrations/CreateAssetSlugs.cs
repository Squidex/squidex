// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrations.Migrations
{
    public sealed class CreateAssetSlugs : IMigration
    {
        private readonly ISnapshotStore<AssetState, string> stateForAssets;

        public CreateAssetSlugs(ISnapshotStore<AssetState, string> stateForAssets)
        {
            this.stateForAssets = stateForAssets;
        }

        public Task UpdateAsync()
        {
            return stateForAssets.ReadAllAsync(async (state, version) =>
            {
                state.Slug = state.FileName.ToAssetSlug();

                var key = DomainId.Combine(state.AppId.Id, state.Id).ToString();

                await stateForAssets.WriteAsync(key, state, version, version);
            });
        }
    }
}
