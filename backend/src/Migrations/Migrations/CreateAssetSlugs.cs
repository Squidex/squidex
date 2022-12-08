// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrations.Migrations;

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
        await foreach (var (key, state, version, _) in stateForAssets.ReadAllAsync(ct))
        {
            state.Slug = state.FileName.ToAssetSlug();

            var job = new SnapshotWriteJob<AssetDomainObject.State>(key, state, version);

            await stateForAssets.WriteAsync(job, ct);
        }
    }
}
