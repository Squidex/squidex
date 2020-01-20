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
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class BackupAssets : IBackupHandler
    {
        private const string TagsFile = "AssetTags.json";
        private readonly HashSet<Guid> assetIds = new HashSet<Guid>();
        private readonly HashSet<Guid> assetFolderIds = new HashSet<Guid>();
        private readonly Rebuilder rebuilder;
        private readonly IAssetFileStore assetFileStore;
        private readonly ITagService tagService;

        public string Name { get; } = "Assets";

        public BackupAssets(Rebuilder rebuilder, IAssetFileStore assetFileStore, ITagService tagService)
        {
            Guard.NotNull(rebuilder);
            Guard.NotNull(assetFileStore);
            Guard.NotNull(tagService);

            this.rebuilder = rebuilder;
            this.assetFileStore = assetFileStore;
            this.tagService = tagService;
        }

        public Task BackupAsync(BackupContext context)
        {
            return BackupTagsAsync(context);
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
            await RestoreTagsAsync(context);

            if (assetIds.Count > 0)
            {
                await rebuilder.InsertManyAsync<AssetDomainObject, AssetState>(async target =>
                {
                    foreach (var id in assetIds)
                    {
                        await target(id);
                    }
                });
            }

            if (assetFolderIds.Count > 0)
            {
                await rebuilder.InsertManyAsync<AssetFolderDomainObject, AssetFolderState>(async target =>
                {
                    foreach (var id in assetFolderIds)
                    {
                        await target(id);
                    }
                });
            }
        }

        private async Task RestoreTagsAsync(RestoreContext context)
        {
            var tags = await context.Reader.ReadJsonAttachmentAsync<TagsExport>(TagsFile);

            await tagService.RebuildTagsAsync(context.AppId, TagGroups.Assets, tags);
        }

        private async Task BackupTagsAsync(BackupContext context)
        {
            var tags = await tagService.GetExportableTagsAsync(context.AppId, TagGroups.Assets);

            await context.Writer.WriteJsonAsync(TagsFile, tags);
        }

        private Task WriteAssetAsync(Guid assetId, long fileVersion, IBackupWriter writer)
        {
            return writer.WriteBlobAsync(GetName(assetId, fileVersion), stream =>
            {
                return assetFileStore.DownloadAsync(assetId, fileVersion, stream);
            });
        }

        private Task ReadAssetAsync(Guid assetId, long fileVersion, IBackupReader reader)
        {
            assetIds.Add(assetId);

            return reader.ReadBlobAsync(GetName(reader.OldGuid(assetId), fileVersion), stream =>
            {
                return assetFileStore.UploadAsync(assetId, fileVersion, stream);
            });
        }

        private static string GetName(Guid assetId, long fileVersion)
        {
            return $"{assetId}_{fileVersion}.asset";
        }
    }
}
