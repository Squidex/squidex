﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Assets;

public class BackupAssetsTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly Rebuilder rebuilder = A.Fake<Rebuilder>();
    private readonly IAssetFileStore assetFileStore = A.Fake<IAssetFileStore>();
    private readonly ITagService tagService = A.Fake<ITagService>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly RefToken actor = RefToken.User("123");
    private readonly BackupAssets sut;

    public BackupAssetsTests()
    {
        ct = cts.Token;

        sut = new BackupAssets(rebuilder, assetFileStore, tagService);
    }

    [Fact]
    public void Should_provide_name()
    {
        Assert.Equal("Assets", sut.Name);
    }

    [Fact]
    public async Task Should_write_tags()
    {
        var tags = new TagsExport
        {
            Tags = new Dictionary<string, Tag>()
        };

        var context = CreateBackupContext();

        A.CallTo(() => tagService.GetExportableTagsAsync(context.AppId, TagGroups.Assets, ct))
            .Returns(tags);

        await sut.BackupAsync(context, ct);

        A.CallTo(() => context.Writer.WriteJsonAsync(A<string>._, tags.Tags, ct))
            .MustHaveHappened();

        A.CallTo(() => context.Writer.WriteJsonAsync(A<string>._, tags.Alias!, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_write_tags_with_alias()
    {
        var tags = new TagsExport
        {
            Alias = new Dictionary<string, string>
            {
                ["tag1"] = "new-name"
            },
            Tags = new Dictionary<string, Tag>()
        };

        var context = CreateBackupContext();

        A.CallTo(() => tagService.GetExportableTagsAsync(context.AppId, TagGroups.Assets, ct))
            .Returns(tags);

        await sut.BackupAsync(context, ct);

        A.CallTo(() => context.Writer.WriteJsonAsync(A<string>._, tags.Tags, ct))
            .MustHaveHappened();

        A.CallTo(() => context.Writer.WriteJsonAsync(A<string>._, tags.Alias, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_read_tags_if_file_exists()
    {
        var tags = new Dictionary<string, Tag>();

        var context = CreateRestoreContext();

        var envelope =
            new Envelope<IEvent>(new AppCreated
            {
                AppId = appId
            });

        A.CallTo(() => context.Reader.HasFileAsync(A<string>._, ct))
            .Returns(true);

        A.CallTo(() => context.Reader.ReadJsonAsync<Dictionary<string, Tag>>(A<string>._, ct))
            .Returns(tags);

        await sut.RestoreEventAsync(envelope, context, ct);

        A.CallTo(() => tagService.RebuildTagsAsync(appId.Id, TagGroups.Assets, A<TagsExport>.That.Matches(x => x.Tags == tags), ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_read_tags_alias_if_file_exists()
    {
        var alias = new Dictionary<string, string>();

        var context = CreateRestoreContext();

        var envelope =
            new Envelope<IEvent>(new AppCreated
            {
                AppId = appId
            });

        A.CallTo(() => context.Reader.HasFileAsync(A<string>._, ct))
            .Returns(false).Once().Then.Returns(true);

        A.CallTo(() => context.Reader.ReadJsonAsync<Dictionary<string, string>>(A<string>._, ct))
            .Returns(alias);

        await sut.RestoreEventAsync(envelope, context, ct);

        A.CallTo(() => tagService.RebuildTagsAsync(appId.Id, TagGroups.Assets, A<TagsExport>.That.Matches(x => x.Alias == alias), ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_read_tags_if_no_file_exists()
    {
        var alias = new Dictionary<string, string>();

        var context = CreateRestoreContext();

        var envelope =
            new Envelope<IEvent>(new AppCreated
            {
                AppId = appId
            });

        A.CallTo(() => context.Reader.HasFileAsync(A<string>._, ct))
            .Returns(false);

        A.CallTo(() => context.Reader.ReadJsonAsync<Dictionary<string, string>>(A<string>._, ct))
            .Returns(alias);

        await sut.RestoreEventAsync(envelope, context, ct);

        A.CallTo(() => context.Reader.ReadJsonAsync<Dictionary<string, string>>(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => context.Reader.ReadJsonAsync<Dictionary<string, Tag>>(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => tagService.RebuildTagsAsync(appId.Id, TagGroups.Assets, A<TagsExport>.That.Matches(x => x.Alias == alias), A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_backup_created_asset()
    {
        var @event = new AssetCreated { AssetId = DomainId.NewGuid() };

        await TestBackupAsync(@event, 0);
    }

    [Fact]
    public async Task Should_backup_created_asset_with_missing_file()
    {
        var @event = new AssetCreated { AssetId = DomainId.NewGuid() };

        await TestBackupFailedAsync(@event, 0);
    }

    [Fact]
    public async Task Should_backup_updated_asset()
    {
        var @event = new AssetUpdated { AssetId = DomainId.NewGuid(), FileVersion = 3 };

        await TestBackupAsync(@event, @event.FileVersion);
    }

    [Fact]
    public async Task Should_backup_updated_asset_with_missing_file()
    {
        var @event = new AssetUpdated { AssetId = DomainId.NewGuid(), FileVersion = 3 };

        await TestBackupFailedAsync(@event, @event.FileVersion);
    }

    private async Task TestBackupAsync(AssetEvent @event, long version)
    {
        var assetStream = new MemoryStream();
        var assetId = @event.AssetId;

        var context = CreateBackupContext();

        A.CallTo(() => context.Writer.OpenBlobAsync($"{assetId}_{version}.asset", ct))
            .Returns(assetStream);

        await sut.BackupEventAsync(AppEvent(@event), context, ct);

        A.CallTo(() => assetFileStore.DownloadAsync(appId.Id, assetId, version, null, assetStream, default, ct))
            .MustHaveHappened();
    }

    private async Task TestBackupFailedAsync(AssetEvent @event, long version)
    {
        var assetStream = new MemoryStream();
        var assetId = @event.AssetId;

        var context = CreateBackupContext();

        A.CallTo(() => context.Writer.OpenBlobAsync($"{assetId}_{version}.asset", ct))
            .Returns(assetStream);

        A.CallTo(() => assetFileStore.DownloadAsync(appId.Id, assetId, version, null, assetStream, default, ct))
            .Throws(new AssetNotFoundException(assetId.ToString()));

        await sut.BackupEventAsync(AppEvent(@event), context, ct);
    }

    [Fact]
    public async Task Should_restore_created_asset()
    {
        var @event = new AssetCreated { AssetId = DomainId.NewGuid() };

        await TestRestoreAsync(@event, 0);
    }

    [Fact]
    public async Task Should_restore_created_asset_with_missing_file()
    {
        var @event = new AssetCreated { AssetId = DomainId.NewGuid() };

        await TestRestoreFailedAsync(@event, 0);
    }

    [Fact]
    public async Task Should_restore_updated_asset()
    {
        var @event = new AssetUpdated { AppId = appId, AssetId = DomainId.NewGuid(), FileVersion = 3 };

        await TestRestoreAsync(@event, @event.FileVersion);
    }

    [Fact]
    public async Task Should_restore_updated_asset_with_missing_file()
    {
        var @event = new AssetUpdated { AppId = appId, AssetId = DomainId.NewGuid(), FileVersion = 3 };

        await TestRestoreFailedAsync(@event, @event.FileVersion);
    }

    private async Task TestRestoreAsync(AssetEvent @event, long version)
    {
        var assetStream = new MemoryStream();
        var assetId = @event.AssetId;

        var context = CreateRestoreContext();

        A.CallTo(() => context.Reader.OpenBlobAsync($"{assetId}_{version}.asset", ct))
            .Returns(assetStream);

        await sut.RestoreEventAsync(AppEvent(@event), context, ct);

        A.CallTo(() => assetFileStore.UploadAsync(appId.Id, assetId, version, null, assetStream, true, ct))
            .MustHaveHappened();
    }

    private async Task TestRestoreFailedAsync(AssetEvent @event, long version)
    {
        var assetStream = new MemoryStream();
        var assetId = @event.AssetId;

        var context = CreateRestoreContext();

        A.CallTo(() => context.Reader.OpenBlobAsync($"{assetId}_{version}.asset", ct))
            .Throws(new FileNotFoundException());

        await sut.RestoreEventAsync(AppEvent(@event), context, ct);

        A.CallTo(() => assetFileStore.UploadAsync(appId.Id, assetId, version, null, assetStream, true, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_restore_states_for_all_assets()
    {
        var assetId1 = DomainId.NewGuid();
        var assetId2 = DomainId.NewGuid();

        var context = CreateRestoreContext();

        await sut.RestoreEventAsync(AppEvent(new AssetCreated
        {
            AssetId = assetId1
        }), context, ct);

        await sut.RestoreEventAsync(AppEvent(new AssetCreated
        {
            AssetId = assetId2
        }), context, ct);

        await sut.RestoreEventAsync(AppEvent(new AssetDeleted
        {
            AssetId = assetId2
        }), context, ct);

        var rebuildAssets = new HashSet<DomainId>();

        A.CallTo(() => rebuilder.InsertManyAsync<AssetDomainObject, AssetDomainObject.State>(A<IEnumerable<DomainId>>._, A<int>._, ct))
            .Invokes(x => rebuildAssets.AddRange(x.GetArgument<IEnumerable<DomainId>>(0)!));

        await sut.RestoreAsync(context, ct);

        Assert.Equal(new HashSet<DomainId>
        {
            DomainId.Combine(appId, assetId1),
            DomainId.Combine(appId, assetId2)
        }, rebuildAssets);
    }

    [Fact]
    public async Task Should_restore_states_for_all_asset_folders()
    {
        var assetFolderId1 = DomainId.NewGuid();
        var assetFolderId2 = DomainId.NewGuid();

        var context = CreateRestoreContext();

        await sut.RestoreEventAsync(AppEvent(new AssetFolderCreated
        {
            AssetFolderId = assetFolderId1
        }), context, ct);

        await sut.RestoreEventAsync(AppEvent(new AssetFolderCreated
        {
            AssetFolderId = assetFolderId2
        }), context, ct);

        await sut.RestoreEventAsync(AppEvent(new AssetFolderDeleted
        {
            AssetFolderId = assetFolderId2
        }), context, ct);

        var rebuildAssetFolders = new HashSet<DomainId>();

        A.CallTo(() => rebuilder.InsertManyAsync<AssetFolderDomainObject, AssetFolderDomainObject.State>(A<IEnumerable<DomainId>>._, A<int>._, ct))
            .Invokes(x => rebuildAssetFolders.AddRange(x.GetArgument<IEnumerable<DomainId>>(0)!));

        await sut.RestoreAsync(context, ct);

        Assert.Equal(new HashSet<DomainId>
        {
            DomainId.Combine(appId, assetFolderId1),
            DomainId.Combine(appId, assetFolderId2)
        }, rebuildAssetFolders);
    }

    private BackupContext CreateBackupContext()
    {
        return new BackupContext(appId.Id, CreateUserMapping(), A.Fake<IBackupWriter>());
    }

    private RestoreContext CreateRestoreContext()
    {
        return new RestoreContext(appId.Id, CreateUserMapping(), A.Fake<IBackupReader>(), DomainId.NewGuid());
    }

    private Envelope<AssetEvent> AppEvent(AssetEvent @event)
    {
        @event.AppId = appId;

        return Envelope.Create(@event).SetAggregateId(DomainId.Combine(appId, @event.AssetId));
    }

    private Envelope<AssetFolderEvent> AppEvent(AssetFolderEvent @event)
    {
        @event.AppId = appId;

        return Envelope.Create(@event).SetAggregateId(DomainId.Combine(appId, @event.AssetFolderId));
    }

    private IUserMapping CreateUserMapping()
    {
        var mapping = A.Fake<IUserMapping>();

        A.CallTo(() => mapping.Initiator).Returns(actor);

        return mapping;
    }
}
