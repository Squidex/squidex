// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetCommandMiddleware : GrainCommandMiddleware<AssetCommand, IAssetGrain>
    {
        private readonly IAssetStore assetStore;
        private readonly IAssetQueryService assetQuery;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
        private readonly IEnumerable<ITagGenerator<CreateAsset>> tagGenerators;
        private readonly ITagService tagService;

        public AssetCommandMiddleware(
            IGrainFactory grainFactory,
            IAssetQueryService assetQuery,
            IAssetStore assetStore,
            IAssetThumbnailGenerator assetThumbnailGenerator,
            IEnumerable<ITagGenerator<CreateAsset>> tagGenerators,
            ITagService tagService)
            : base(grainFactory)
        {
            Guard.NotNull(assetStore, nameof(assetStore));
            Guard.NotNull(assetQuery, nameof(assetQuery));
            Guard.NotNull(assetThumbnailGenerator, nameof(assetThumbnailGenerator));
            Guard.NotNull(tagGenerators, nameof(tagGenerators));
            Guard.NotNull(tagService, nameof(tagService));

            this.assetStore = assetStore;
            this.assetQuery = assetQuery;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
            this.tagGenerators = tagGenerators;
            this.tagService = tagService;
        }

        public override async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            switch (context.Command)
            {
                case CreateAsset createAsset:
                    {
                        if (createAsset.Tags == null)
                        {
                            createAsset.Tags = new HashSet<string>();
                        }

                        await EnrichWithImageInfosAsync(createAsset);
                        await EnrichWithHashAndUploadAsync(createAsset, context);

                        try
                        {
                            var existings = await assetQuery.QueryByHashAsync(createAsset.AppId.Id, createAsset.FileHash);

                            AssetCreatedResult result = null;

                            foreach (var existing in existings)
                            {
                                if (IsDuplicate(createAsset, existing))
                                {
                                    var denormalizedTags = await tagService.DenormalizeTagsAsync(createAsset.AppId.Id, TagGroups.Assets, existing.Tags);

                                    result = new AssetCreatedResult(existing, true, new HashSet<string>(denormalizedTags.Values));
                                }

                                break;
                            }

                            if (result == null)
                            {
                                foreach (var tagGenerator in tagGenerators)
                                {
                                    tagGenerator.GenerateTags(createAsset, createAsset.Tags);
                                }

                                var asset = (IAssetEntity)await ExecuteCommandAsync(createAsset);

                                result = new AssetCreatedResult(asset, false, createAsset.Tags);

                                await assetStore.CopyAsync(context.ContextId.ToString(), createAsset.AssetId.ToString(), asset.FileVersion, null);
                            }

                            context.Complete(result);
                        }
                        finally
                        {
                            await assetStore.DeleteAsync(context.ContextId.ToString());
                        }

                        break;
                    }

                case UpdateAsset updateAsset:
                    {
                        await EnrichWithImageInfosAsync(updateAsset);
                        await EnrichWithHashAndUploadAsync(updateAsset, context);

                        try
                        {
                            var result = (AssetResult)await ExecuteAndAdjustTagsAsync(updateAsset);

                            context.Complete(result);

                            await assetStore.CopyAsync(context.ContextId.ToString(), updateAsset.AssetId.ToString(), result.Asset.FileVersion, null);
                        }
                        finally
                        {
                            await assetStore.DeleteAsync(context.ContextId.ToString());
                        }

                        break;
                    }

                case AssetCommand command:
                    {
                        var result = await ExecuteAndAdjustTagsAsync(command);

                        context.Complete(result);

                        break;
                    }

                default:
                    await base.HandleAsync(context, next);

                    break;
            }
        }

        private async Task<object> ExecuteAndAdjustTagsAsync(AssetCommand command)
        {
            var result = await ExecuteCommandAsync(command);

            if (result is IAssetEntity asset)
            {
                var denormalizedTags = await tagService.DenormalizeTagsAsync(asset.AppId.Id, TagGroups.Assets, asset.Tags);

                return new AssetResult(asset, new HashSet<string>(denormalizedTags.Values));
            }

            return result;
        }

        private static bool IsDuplicate(CreateAsset createAsset, IAssetEntity asset)
        {
            return asset != null && asset.FileName == createAsset.File.FileName && asset.FileSize == createAsset.File.FileSize;
        }

        private async Task EnrichWithImageInfosAsync(UploadAssetCommand command)
        {
            command.ImageInfo = await assetThumbnailGenerator.GetImageInfoAsync(command.File.OpenRead());
        }

        private async Task EnrichWithHashAndUploadAsync(UploadAssetCommand command, CommandContext context)
        {
            using (var hashStream = new HasherStream(command.File.OpenRead(), HashAlgorithmName.SHA256))
            {
                await assetStore.UploadAsync(context.ContextId.ToString(), hashStream);

                command.FileHash = $"{hashStream.GetHashStringAndReset()}{command.File.FileName}{command.File.FileSize}".Sha256Base64();
            }
        }
    }
}
