﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

#pragma warning disable IDE0067 // Dispose objects before losing scope

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class BackupAssetsTests
    {
        private readonly Rebuilder rebuilder = A.Fake<Rebuilder>();
        private readonly IAssetFileStore assetFileStore = A.Fake<IAssetFileStore>();
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly RefToken actor = new RefToken(RefTokenType.Subject, "123");
        private readonly BackupAssets sut;

        public BackupAssetsTests()
        {
            sut = new BackupAssets(rebuilder, assetFileStore, tagService);
        }

        [Fact]
        public void Should_provide_name()
        {
            Assert.Equal("Assets", sut.Name);
        }

        [Fact]
        public async Task Should_writer_tags()
        {
            var tags = new TagsExport();

            var context = CreateBackupContext();

            A.CallTo(() => tagService.GetExportableTagsAsync(context.AppId, TagGroups.Assets))
                .Returns(tags);

            await sut.BackupAsync(context);

            A.CallTo(() => context.Writer.WriteJsonAsync(A<string>._, tags))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_read_tags()
        {
            var tags = new TagsExport();

            var context = CreateRestoreContext();

            A.CallTo(() => context.Reader.ReadJsonAttachmentAsync<TagsExport>(A<string>._))
                .Returns(tags);

            await sut.RestoreAsync(context);

            A.CallTo(() => tagService.RebuildTagsAsync(appId.Id, TagGroups.Assets, tags))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_backup_created_asset()
        {
            var @event = new AssetCreated { AssetId = DomainId.NewGuid() };

            await TestBackupEventAsync(@event, 0);
        }

        [Fact]
        public async Task Should_backup_updated_asset()
        {
            var @event = new AssetUpdated { AssetId = DomainId.NewGuid(), FileVersion = 3 };

            await TestBackupEventAsync(@event, @event.FileVersion);
        }

        private async Task TestBackupEventAsync(AssetEvent @event, long version)
        {
            var assetStream = new MemoryStream();
            var assetId = @event.AssetId;

            var context = CreateBackupContext();

            A.CallTo(() => context.Writer.WriteBlobAsync($"{assetId}_{version}.asset", A<Func<Stream, Task>>._))
                .Invokes((string _, Func<Stream, Task> handler) => handler(assetStream));

            await sut.BackupEventAsync(AppEvent(@event), context);

            A.CallTo(() => assetFileStore.DownloadAsync(appId.Id, assetId, version, assetStream, default, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_restore_created_asset()
        {
            var @event = new AssetCreated { AssetId = DomainId.NewGuid() };

            await TestRestoreAsync(@event, 0);
        }

        [Fact]
        public async Task Should_restore_updated_asset()
        {
            var @event = new AssetUpdated { AppId = appId, AssetId = DomainId.NewGuid(), FileVersion = 3 };

            await TestRestoreAsync(@event, @event.FileVersion);
        }

        private async Task TestRestoreAsync(AssetEvent @event, long version)
        {
            var assetStream = new MemoryStream();
            var assetId = @event.AssetId;

            var context = CreateRestoreContext();

            A.CallTo(() => context.Reader.ReadBlobAsync($"{assetId}_{version}.asset", A<Func<Stream, Task>>._))
                .Invokes((string _, Func<Stream, Task> handler) => handler(assetStream));

            await sut.RestoreEventAsync(AppEvent(@event), context);

            A.CallTo(() => assetFileStore.UploadAsync(appId.Id, assetId, version, assetStream, default))
                .MustHaveHappened();
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
            }), context);

            await sut.RestoreEventAsync(AppEvent(new AssetCreated
            {
                AssetId = assetId2
            }), context);

            await sut.RestoreEventAsync(AppEvent(new AssetDeleted
            {
                AssetId = assetId2
            }), context);

            var rebuildAssets = new HashSet<DomainId>();

            A.CallTo(() => rebuilder.InsertManyAsync<AssetDomainObject, AssetState>(A<IEnumerable<DomainId>>._, A<CancellationToken>._))
                .Invokes((IEnumerable<DomainId> source, CancellationToken _) => rebuildAssets.AddRange(source));

            await sut.RestoreAsync(context);

            Assert.Equal(new HashSet<DomainId>
            {
                DomainId.Combine(appId.Id, assetId1),
                DomainId.Combine(appId.Id, assetId2)
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
            }), context);

            await sut.RestoreEventAsync(AppEvent(new AssetFolderCreated
            {
                AssetFolderId = assetFolderId2
            }), context);

            await sut.RestoreEventAsync(AppEvent(new AssetFolderDeleted
            {
                AssetFolderId = assetFolderId2
            }), context);

            var rebuildAssetFolders = new HashSet<DomainId>();

            A.CallTo(() => rebuilder.InsertManyAsync<AssetFolderDomainObject, AssetFolderState>(A<IEnumerable<DomainId>>._, A<CancellationToken>._))
                .Invokes((IEnumerable<DomainId> source, CancellationToken _) => rebuildAssetFolders.AddRange(source));

            await sut.RestoreAsync(context);

            Assert.Equal(new HashSet<DomainId>
            {
                DomainId.Combine(appId.Id, assetFolderId1),
                DomainId.Combine(appId.Id, assetFolderId2)
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

            var envelope = Envelope.Create(@event);

            envelope.SetAggregateId(DomainId.Combine(appId.Id, @event.AssetId));

            return envelope;
        }

        private Envelope<AssetFolderEvent> AppEvent(AssetFolderEvent @event)
        {
            @event.AppId = appId;

            var envelope = Envelope.Create(@event);

            envelope.SetAggregateId(DomainId.Combine(appId.Id, @event.AssetFolderId));

            return envelope;
        }

        private IUserMapping CreateUserMapping()
        {
            var mapping = A.Fake<IUserMapping>();

            A.CallTo(() => mapping.Initiator).Returns(actor);

            return mapping;
        }
    }
}
