// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Assets.MongoDb;

public class AssetMappingTests
{
    [Fact]
    public void Should_map_asset()
    {
        var user = RefToken.User("1");

        var time = SystemClock.Instance.GetCurrentInstant();

        var source = new AssetDomainObject.State
        {
            Id = DomainId.NewGuid(),
            AppId = NamedId.Of(DomainId.NewGuid(), "my-app"),
            Created = time,
            CreatedBy = user,
            FileHash = "my-hash",
            FileName = "my-image.png",
            FileSize = 1024,
            FileVersion = 13,
            IsDeleted = true,
            IsProtected = true,
            LastModified = time,
            LastModifiedBy = user,
            Metadata = new AssetMetadata().SetPixelHeight(600),
            MimeType = "image/png",
            ParentId = DomainId.NewGuid(),
            Slug = "my-image",
            Tags = new HashSet<string> { "image" },
            TotalSize = 1024 * 2,
            Type = AssetType.Image,
            Version = 42
        };

        var snapshotJob = new SnapshotWriteJob<AssetDomainObject.State>(source.UniqueId, source, source.Version);
        var snapshot = MongoAssetEntity.Create(snapshotJob);

        var mapped = snapshot.ToState();

        mapped.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void Should_map_asset_folder()
    {
        var user = RefToken.User("1");

        var time = SystemClock.Instance.GetCurrentInstant();

        var source = new AssetFolderDomainObject.State
        {
            Id = DomainId.NewGuid(),
            AppId = NamedId.Of(DomainId.NewGuid(), "my-app"),
            Created = time,
            CreatedBy = user,
            FolderName = "my-folder",
            IsDeleted = true,
            LastModified = time,
            LastModifiedBy = user,
            ParentId = DomainId.NewGuid(),
            Version = 42
        };

        var snapshotJob = new SnapshotWriteJob<AssetFolderDomainObject.State>(source.UniqueId, source, source.Version);
        var snapshot = MongoAssetFolderEntity.Create(snapshotJob);

        var mapped = snapshot.ToState();

        mapped.Should().BeEquivalentTo(source);
    }
}
