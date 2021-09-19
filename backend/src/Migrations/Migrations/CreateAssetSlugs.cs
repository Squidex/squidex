// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrations.Migrations
{
    public sealed class CreateAssetSlugs : IMigration
    {
        private readonly ISnapshotStore<AssetDomainObject.State> stateForAssets;

        public CreateAssetSlugs(ISnapshotStore<AssetDomainObject.State> stateForAssets)
        {
            this.stateForAssets = stateForAssets;
        }

        public async Task UpdateAsync(
            CancellationToken ct)
        {
            await foreach (var (state, version) in stateForAssets.ReadAllAsync(ct))
            {
                state.Slug = state.FileName.ToAssetSlug();

                var key = DomainId.Combine(state.AppId.Id, state.Id);

                await stateForAssets.WriteAsync(key, state, version, version, ct);
            }
        }
    }
}
