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
        private readonly IAssetQueryService assetQueryService;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
        private readonly IEnumerable<ITagGenerator<CreateAsset>> tagGenerators;

        public AssetCommandMiddleware(
            IGrainFactory grainFactory,
            IAssetQueryService assetQueryService,
            IAssetStore assetStore,
            IAssetThumbnailGenerator assetThumbnailGenerator,
            IEnumerable<ITagGenerator<CreateAsset>> tagGenerators)
            : base(grainFactory)
        {
            Guard.NotNull(assetStore, nameof(assetStore));
            Guard.NotNull(assetQueryService, nameof(assetQueryService));
            Guard.NotNull(assetThumbnailGenerator, nameof(assetThumbnailGenerator));
            Guard.NotNull(tagGenerators, nameof(tagGenerators));

            this.assetStore = assetStore;
            this.assetQueryService = assetQueryService;
            this.assetThumbnailGenerator = assetThumbnailGenerator;

            this.tagGenerators = tagGenerators;
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

                        createAsset.ImageInfo = await assetThumbnailGenerator.GetImageInfoAsync(createAsset.File.OpenRead());

                        createAsset.FileHash = await UploadAsync(context, createAsset.File);

                        try
                        {
                            var existings = await assetQueryService.QueryByHashAsync(createAsset.AppId.Id, createAsset.FileHash);

                            AssetCreatedResult result = null;

                            foreach (var existing in existings)
                            {
                                if (IsDuplicate(createAsset, existing))
                                {
                                    result = new AssetCreatedResult(
                                        existing.Id,
                                        existing.Tags,
                                        existing.Version,
                                        existing.FileVersion,
                                        existing.FileHash,
                                        true);
                                }

                                break;
                            }

                            if (result == null)
                            {
                                foreach (var tagGenerator in tagGenerators)
                                {
                                    tagGenerator.GenerateTags(createAsset, createAsset.Tags);
                                }

                                var commandResult = (AssetSavedResult)await ExecuteCommandAsync(createAsset);

                                result = new AssetCreatedResult(
                                    createAsset.AssetId,
                                    createAsset.Tags,
                                    commandResult.Version,
                                    commandResult.FileVersion,
                                    commandResult.FileHash,
                                    false);

                                await assetStore.CopyAsync(context.ContextId.ToString(), createAsset.AssetId.ToString(), result.FileVersion, null);
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
                        updateAsset.ImageInfo = await assetThumbnailGenerator.GetImageInfoAsync(updateAsset.File.OpenRead());

                        updateAsset.FileHash = await UploadAsync(context, updateAsset.File);
                        try
                        {
                            var result = (AssetSavedResult)await ExecuteCommandAsync(updateAsset);

                            context.Complete(result);

                            await assetStore.CopyAsync(context.ContextId.ToString(), updateAsset.AssetId.ToString(), result.FileVersion, null);
                        }
                        finally
                        {
                            await assetStore.DeleteAsync(context.ContextId.ToString());
                        }

                        break;
                    }

                default:
                    await base.HandleAsync(context, next);
                    break;
            }
        }

        private static bool IsDuplicate(CreateAsset createAsset, IAssetEntity asset)
        {
            return asset != null && asset.FileName == createAsset.File.FileName && asset.FileSize == createAsset.File.FileSize;
        }

        private async Task<string> UploadAsync(CommandContext context, AssetFile file)
        {
            string hash;

            using (var hashStream = new HasherStream(file.OpenRead(), HashAlgorithmName.SHA256))
            {
                await assetStore.UploadAsync(context.ContextId.ToString(), hashStream);

                hash = $"{hashStream.GetHashStringAndReset()}{file.FileName}{file.FileSize}".Sha256Base64();
            }

            return hash;
        }
    }
}
