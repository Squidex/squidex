// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class BackupAssets : IBackupHandler
    {
        private const int BatchSize = 100;
        private const string TagsFile = "AssetTags.json";
        private const string TagsAliasFile = "AssetTagsAlias.json";
        private readonly HashSet<DomainId> assetIds = new HashSet<DomainId>();
        private readonly HashSet<DomainId> assetFolderIds = new HashSet<DomainId>();
        private readonly Rebuilder rebuilder;
        private readonly IAssetFileStore assetFileStore;
        private readonly ITagService tagService;

        public string Name { get; } = "Assets";

        public BackupAssets(Rebuilder rebuilder, IAssetFileStore assetFileStore, ITagService tagService)
        {
            this.rebuilder = rebuilder;
            this.assetFileStore = assetFileStore;
            this.tagService = tagService;
        }

        public Task BackupAsync(BackupContext context,
            CancellationToken ct)
        {
            return BackupTagsAsync(context, ct);
        }

        public Task BackupEventAsync(Envelope<IEvent> @event, BackupContext context,
            CancellationToken ct)
        {
            switch (@event.Payload)
            {
                case AssetCreated assetCreated:
                    return WriteAssetAsync(
                        assetCreated.AppId.Id,
                        assetCreated.AssetId,
                        assetCreated.FileVersion,
                        context.Writer,
                        ct);
                case AssetUpdated assetUpdated:
                    return WriteAssetAsync(
                        assetUpdated.AppId.Id,
                        assetUpdated.AssetId,
                        assetUpdated.FileVersion,
                        context.Writer,
                        ct);
            }

            return Task.CompletedTask;
        }

        public async Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context,
            CancellationToken ct)
        {
            switch (@event.Payload)
            {
                case AssetFolderCreated:
                    assetFolderIds.Add(@event.Headers.AggregateId());
                    break;
                case AssetCreated assetCreated:
                    assetIds.Add(@event.Headers.AggregateId());

                    await ReadAssetAsync(
                        assetCreated.AppId.Id,
                        assetCreated.AssetId,
                        assetCreated.FileVersion,
                        context.Reader,
                        ct);
                    break;
                case AssetUpdated assetUpdated:
                    await ReadAssetAsync(
                        assetUpdated.AppId.Id,
                        assetUpdated.AssetId,
                        assetUpdated.FileVersion,
                        context.Reader,
                        ct);
                    break;
            }

            return true;
        }

        public async Task RestoreAsync(RestoreContext context,
            CancellationToken ct)
        {
            await RestoreTagsAsync(context, ct);

            if (assetIds.Count > 0)
            {
                await rebuilder.InsertManyAsync<AssetDomainObject, AssetDomainObject.State>(assetIds, BatchSize, ct);
            }

            if (assetFolderIds.Count > 0)
            {
                await rebuilder.InsertManyAsync<AssetFolderDomainObject, AssetFolderDomainObject.State>(assetFolderIds, BatchSize, ct);
            }
        }

        private async Task RestoreTagsAsync(RestoreContext context,
            CancellationToken ct)
        {
            var tags = (Dictionary<string, Tag>?)null;

            if (await context.Reader.HasFileAsync(TagsFile, ct))
            {
                tags = await context.Reader.ReadJsonAsync<Dictionary<string, Tag>>(TagsFile, ct);
            }

            var alias = (Dictionary<string, string>?)null;

            if (await context.Reader.HasFileAsync(TagsAliasFile, ct))
            {
                alias = await context.Reader.ReadJsonAsync<Dictionary<string, string>>(TagsAliasFile, ct);
            }

            if (alias == null && tags == null)
            {
                return;
            }

            var export = new TagsExport { Tags = tags, Alias = alias };

            await tagService.RebuildTagsAsync(context.AppId, TagGroups.Assets, export);
        }

        private async Task BackupTagsAsync(BackupContext context,
            CancellationToken ct)
        {
            var tags = await tagService.GetExportableTagsAsync(context.AppId, TagGroups.Assets);

            if (tags.Tags != null)
            {
                await context.Writer.WriteJsonAsync(TagsFile, tags.Tags, ct);
            }

            if (tags.Alias?.Count > 0)
            {
                await context.Writer.WriteJsonAsync(TagsAliasFile, tags.Alias, ct);
            }
        }

        private async Task WriteAssetAsync(DomainId appId, DomainId assetId, long fileVersion, IBackupWriter writer,
            CancellationToken ct)
        {
            try
            {
                var fileName = GetName(assetId, fileVersion);

                await using (var stream = await writer.OpenBlobAsync(fileName, ct))
                {
                    await assetFileStore.DownloadAsync(appId, assetId, fileVersion, null, stream, default, ct);
                }
            }
            catch (AssetNotFoundException)
            {
                return;
            }
        }

        private async Task ReadAssetAsync(DomainId appId, DomainId assetId, long fileVersion, IBackupReader reader,
            CancellationToken ct)
        {
            try
            {
                var fileName = GetName(assetId, fileVersion);

                await using (var stream = await reader.OpenBlobAsync(fileName, ct))
                {
                    await assetFileStore.UploadAsync(appId, assetId, fileVersion, null, stream, true, ct);
                }
            }
            catch (FileNotFoundException)
            {
                return;
            }
        }

        private static string GetName(DomainId assetId, long fileVersion)
        {
            return $"{assetId}_{fileVersion}.asset";
        }
    }
}
