// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class BackupAssets : BackupHandlerWithStore
    {
        private const string TagsFile = "AssetTags.json";
        private readonly HashSet<Guid> assetIds = new HashSet<Guid>();
        private readonly HashSet<Guid> assetFolderIds = new HashSet<Guid>();
        private readonly IAssetStore assetStore;
        private readonly ITagService tagService;

        public override string Name { get; } = "Assets";

        public BackupAssets(IStore<Guid> store, IAssetStore assetStore, ITagService tagService)
            : base(store)
        {
            Guard.NotNull(assetStore);
            Guard.NotNull(tagService);

            this.assetStore = assetStore;

            this.tagService = tagService;
        }

        public Task BackupAsync(Guid appId, BackupContext context)
        {
            return BackupTagsAsync(appId, context.Writer);
        }

        public Task BackupEventAsync(Envelope<IEvent> @event, BackupContext context)
        {
            switch (@event.Payload)
            {
                case AssetCreated assetCreated:
                    return WriteAssetAsync(assetCreated.AssetId, assetCreated.FileVersion, context.Writer);
                case AssetUpdated assetUpdated:
                    return WriteAssetAsync(assetUpdated.AssetId, assetUpdated.FileVersion, context.Writer);
            }

            return TaskHelper.Done;
        }

        public async Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context)
        {
            switch (@event.Payload)
            {
                case AssetFolderCreated assetFolderCreated:
                    assetFolderIds.Add(assetFolderCreated.AssetFolderId);
                    break;
                case AssetCreated assetCreated:
                    await ReadAssetAsync(assetCreated.AssetId, assetCreated.FileVersion, context.Reader);
                    break;
                case AssetUpdated assetUpdated:
                    await ReadAssetAsync(assetUpdated.AssetId, assetUpdated.FileVersion, context.Reader);
                    break;
            }

            return true;
        }

        public async Task RestoreAsync(RestoreContext context)
        {
            await RestoreTagsAsync(context.AppId, context.Reader);

            await RebuildManyAsync(assetIds, RebuildAsync<AssetState, AssetGrain>);
            await RebuildManyAsync(assetFolderIds, RebuildAsync<AssetFolderState, AssetFolderGrain>);
        }

        private async Task RestoreTagsAsync(Guid appId, IBackupReader reader)
        {
            var tags = await reader.ReadJsonAttachmentAsync<TagsExport>(TagsFile);

            await tagService.RebuildTagsAsync(appId, TagGroups.Assets, tags);
        }

        private async Task BackupTagsAsync(Guid appId, BackupWriter writer)
        {
            var tags = await tagService.GetExportableTagsAsync(appId, TagGroups.Assets);

            await writer.WriteJsonAsync(TagsFile, tags);
        }

        private Task WriteAssetAsync(Guid assetId, long fileVersion, BackupWriter writer)
        {
            return writer.WriteBlobAsync(GetName(assetId, fileVersion), stream =>
            {
                return assetStore.DownloadAsync(assetId.ToString(), fileVersion, null, stream);
            });
        }

        private Task ReadAssetAsync(Guid assetId, long fileVersion, IBackupReader reader)
        {
            assetIds.Add(assetId);

            return reader.ReadBlobAsync(GetName(reader.OldGuid(assetId), fileVersion), async stream =>
            {
                try
                {
                    await assetStore.UploadAsync(assetId.ToString(), fileVersion, null, stream, true);
                }
                catch (AssetAlreadyExistsException)
                {
                    return;
                }
            });
        }

        private static string GetName(Guid assetId, long fileVersion)
        {
            return $"{assetId}_{fileVersion}.asset";
        }
    }
}
