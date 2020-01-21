// ==========================================================================
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
using Squidex.Infrastructure.Tasks;
using Xunit;

#pragma warning disable IDE0067 // Dispose objects before losing scope

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class BackupAssetsTests
    {
        private readonly Rebuilder rebuilder = A.Fake<Rebuilder>();
        private readonly IAssetFileStore assetFileStore = A.Fake<IAssetFileStore>();
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly Guid appId = Guid.NewGuid();
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

            A.CallTo(() => tagService.GetExportableTagsAsync(appId, TagGroups.Assets))
                .Returns(tags);

            await sut.BackupAsync(context);

            A.CallTo(() => context.Writer.WriteJsonAsync(A<string>.Ignored, tags))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_read_tags()
        {
            var tags = new TagsExport();

            var context = CreateRestoreContext();

            A.CallTo(() => context.Reader.ReadJsonAttachmentAsync<TagsExport>(A<string>.Ignored))
                .Returns(tags);

            await sut.RestoreAsync(context);

            A.CallTo(() => tagService.RebuildTagsAsync(appId, TagGroups.Assets, tags))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_backup_created_asset()
        {
            var @event = new AssetCreated { AssetId = Guid.NewGuid() };

            await TestBackupEventAsync(@event, 0);
        }

        [Fact]
        public async Task Should_backup_updated_asset()
        {
            var @event = new AssetUpdated { AssetId = Guid.NewGuid(), FileVersion = 3 };

            await TestBackupEventAsync(@event, @event.FileVersion);
        }

        private async Task TestBackupEventAsync(AssetEvent @event, long version)
        {
            var assetStream = new MemoryStream();
            var assetId = @event.AssetId;

            var context = CreateBackupContext();

            A.CallTo(() => context.Writer.WriteBlobAsync($"{assetId}_{version}.asset", A<Func<Stream, Task>>.Ignored))
                .Invokes((string _, Func<Stream, Task> handler) => handler(assetStream));

            await sut.BackupEventAsync(Envelope.Create(@event), context);

            A.CallTo(() => assetFileStore.DownloadAsync(assetId, version, assetStream, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_restore_created_asset()
        {
            var @event = new AssetCreated { AssetId = Guid.NewGuid() };

            await TestRestoreAsync(@event, 0);
        }

        [Fact]
        public async Task Should_restore_updated_asset()
        {
            var @event = new AssetUpdated { AssetId = Guid.NewGuid(), FileVersion = 3 };

            await TestRestoreAsync(@event, @event.FileVersion);
        }

        private async Task TestRestoreAsync(AssetEvent @event, long version)
        {
            var oldId = Guid.NewGuid();

            var assetStream = new MemoryStream();
            var assetId = @event.AssetId;

            var context = CreateRestoreContext();

            A.CallTo(() => context.Reader.OldGuid(assetId))
                .Returns(oldId);

            A.CallTo(() => context.Reader.ReadBlobAsync($"{oldId}_{version}.asset", A<Func<Stream, Task>>.Ignored))
                .Invokes((string _, Func<Stream, Task> handler) => handler(assetStream));

            await sut.RestoreEventAsync(Envelope.Create(@event), context);

            A.CallTo(() => assetFileStore.UploadAsync(assetId, version, assetStream, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_restore_states_for_all_assets()
        {
            var assetId1 = Guid.NewGuid();
            var assetId2 = Guid.NewGuid();

            var context = CreateRestoreContext();

            await sut.RestoreEventAsync(Envelope.Create(new AssetCreated
            {
                AssetId = assetId1
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new AssetCreated
            {
                AssetId = assetId2
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new AssetDeleted
            {
                AssetId = assetId2
            }), context);

            var rebuildAssets = new HashSet<Guid>();

            var add = new Func<Guid, Task>(id =>
            {
                rebuildAssets.Add(id);

                return TaskHelper.Done;
            });

            A.CallTo(() => rebuilder.InsertManyAsync<AssetDomainObject, AssetState>(A<IdSource>.Ignored, A<CancellationToken>.Ignored))
                .Invokes((IdSource source, CancellationToken _) => source(add));

            await sut.RestoreAsync(context);

            Assert.Equal(new HashSet<Guid>
            {
                assetId1,
                assetId2
            }, rebuildAssets);
        }

        [Fact]
        public async Task Should_restore_states_for_all_asset_folders()
        {
            var assetFolderId1 = Guid.NewGuid();
            var assetFolderId2 = Guid.NewGuid();

            var context = CreateRestoreContext();

            await sut.RestoreEventAsync(Envelope.Create(new AssetFolderCreated
            {
                AssetFolderId = assetFolderId1
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new AssetFolderCreated
            {
                AssetFolderId = assetFolderId2
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new AssetFolderDeleted
            {
                AssetFolderId = assetFolderId2
            }), context);

            var rebuildAssets = new HashSet<Guid>();

            var add = new Func<Guid, Task>(id =>
            {
                rebuildAssets.Add(id);

                return TaskHelper.Done;
            });

            A.CallTo(() => rebuilder.InsertManyAsync<AssetFolderDomainObject, AssetFolderState>(A<IdSource>.Ignored, A<CancellationToken>.Ignored))
                .Invokes((IdSource source, CancellationToken _) => source(add));

            await sut.RestoreAsync(context);

            Assert.Equal(new HashSet<Guid>
            {
                assetFolderId1,
                assetFolderId2
            }, rebuildAssets);
        }

        private BackupContext CreateBackupContext()
        {
            return new BackupContext(appId, CreateUserMapping(), A.Fake<IBackupWriter>());
        }

        private RestoreContext CreateRestoreContext()
        {
            return new RestoreContext(appId, CreateUserMapping(), A.Fake<IBackupReader>());
        }

        private IUserMapping CreateUserMapping()
        {
            var mapping = A.Fake<IUserMapping>();

            A.CallTo(() => mapping.Initiator).Returns(actor);

            return mapping;
        }
    }
}
