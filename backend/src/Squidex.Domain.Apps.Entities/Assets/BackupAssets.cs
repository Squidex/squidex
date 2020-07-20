﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class BackupAssets : IBackupHandler
    {
        private const string TagsFile = "AssetTags.json";
        private readonly HashSet<DomainId> assetIds = new HashSet<DomainId>();
        private readonly HashSet<DomainId> assetFolderIds = new HashSet<DomainId>();
        private readonly Rebuilder rebuilder;
        private readonly IAssetFileStore assetFileStore;
        private readonly ITagService tagService;

        public string Name { get; } = "Assets";

        public BackupAssets(Rebuilder rebuilder, IAssetFileStore assetFileStore, ITagService tagService)
        {
            Guard.NotNull(rebuilder, nameof(rebuilder));
            Guard.NotNull(assetFileStore, nameof(assetFileStore));
            Guard.NotNull(tagService, nameof(tagService));

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
                    return WriteAssetAsync(
                        assetCreated.AppId.Id,
                        assetCreated.AssetId,
                        assetCreated.FileVersion,
                        context.Writer);
                case AssetUpdated assetUpdated:
                    return WriteAssetAsync(
                        assetUpdated.AppId.Id,
                        assetUpdated.AssetId,
                        assetUpdated.FileVersion,
                        context.Writer);
            }

            return Task.CompletedTask;
        }

        public async Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context)
        {
            switch (@event.Payload)
            {
                case AssetFolderCreated _:
                    assetFolderIds.Add(@event.Headers.AggregateId());
                    break;
                case AssetCreated assetCreated:
                    assetIds.Add(@event.Headers.AggregateId());

                    await ReadAssetAsync(
                        assetCreated.AppId.Id,
                        assetCreated.AssetId,
                        assetCreated.FileVersion,
                        context.Reader);
                    break;
                case AssetUpdated assetUpdated:
                    await ReadAssetAsync(
                        assetUpdated.AppId.Id,
                        assetUpdated.AssetId,
                        assetUpdated.FileVersion,
                        context.Reader);
                    break;
            }

            return true;
        }

        public async Task RestoreAsync(RestoreContext context)
        {
            await RestoreTagsAsync(context);

            if (assetIds.Count > 0)
            {
                await rebuilder.InsertManyAsync<AssetDomainObject, AssetState>(assetIds);
            }

            if (assetFolderIds.Count > 0)
            {
                await rebuilder.InsertManyAsync<AssetFolderDomainObject, AssetFolderState>(assetFolderIds);
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

        private Task WriteAssetAsync(DomainId appId, DomainId assetId, long fileVersion, IBackupWriter writer)
        {
            return writer.WriteBlobAsync(GetName(assetId, fileVersion), stream =>
            {
                return assetFileStore.DownloadAsync(appId, assetId, fileVersion, stream);
            });
        }

        private Task ReadAssetAsync(DomainId appId, DomainId assetId, long fileVersion, IBackupReader reader)
        {
            return reader.ReadBlobAsync(GetName(assetId, fileVersion), stream =>
            {
                return assetFileStore.UploadAsync(appId, assetId, fileVersion, stream);
            });
        }

        private static string GetName(DomainId assetId, long fileVersion)
        {
            return $"{assetId}_{fileVersion}.asset";
        }
    }
}
