// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Assets.MongoDb;

public class AssetMappingTests : GivenContext
{
    [Fact]
    public void Should_map_asset()
    {
        var source = CreateAsset() as Asset;

        var snapshotJob = new SnapshotWriteJob<Asset>(source.UniqueId, source, source.Version);
        var snapshot = MongoAssetEntity.Create(snapshotJob);

        var mapped = snapshot.ToState();

        mapped.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void Should_map_asset_folder()
    {
        var source = CreateAssetFolder();

        var snapshotJob = new SnapshotWriteJob<AssetFolder>(source.UniqueId, source, source.Version);
        var snapshot = MongoAssetFolderEntity.Create(snapshotJob);

        var mapped = snapshot.ToState();

        mapped.Should().BeEquivalentTo(source);
    }
}
