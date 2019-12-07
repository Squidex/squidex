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
        private readonly IAssetEnricher assetEnricher;
        private readonly IAssetQueryService assetQuery;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
        private readonly IContextProvider contextProvider;
        private readonly IEnumerable<ITagGenerator<CreateAsset>> tagGenerators;

        public AssetCommandMiddleware(
            IGrainFactory grainFactory,
            IAssetEnricher assetEnricher,
            IAssetQueryService assetQuery,
            IAssetStore assetStore,
            IAssetThumbnailGenerator assetThumbnailGenerator,
            IContextProvider contextProvider,
            IEnumerable<ITagGenerator<CreateAsset>> tagGenerators)
            : base(grainFactory)
        {
            Guard.NotNull(assetEnricher);
            Guard.NotNull(assetStore);
            Guard.NotNull(assetQuery);
            Guard.NotNull(assetThumbnailGenerator);
            Guard.NotNull(contextProvider);
            Guard.NotNull(tagGenerators);

            this.assetStore = assetStore;
            this.assetEnricher = assetEnricher;
            this.assetQuery = assetQuery;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
            this.contextProvider = contextProvider;
            this.tagGenerators = tagGenerators;
        }

        public override async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            var tempFile = context.ContextId.ToString();

            switch (context.Command)
            {
                case CreateAsset createAsset:
                    {
                        await EnrichWithImageInfosAsync(createAsset);
                        await EnrichWithHashAndUploadAsync(createAsset, tempFile);

                        try
                        {
                            var ctx = contextProvider.Context.Clone().WithNoAssetEnrichment();

                            var existings = await assetQuery.QueryByHashAsync(ctx, createAsset.AppId.Id, createAsset.FileHash);

                            foreach (var existing in existings)
                            {
                                if (IsDuplicate(existing, createAsset.File))
                                {
                                    var result = new AssetCreatedResult(existing, true);

                                    context.Complete(result);

                                    await next();
                                    return;
                                }
                            }

                            GenerateTags(createAsset);

                            await HandleCoreAsync(context, next);

                            var asset = context.Result<IEnrichedAssetEntity>();

                            context.Complete(new AssetCreatedResult(asset, false));

                            await assetStore.CopyAsync(tempFile, createAsset.AssetId, asset.FileVersion, null);
                        }
                        finally
                        {
                            await assetStore.DeleteAsync(tempFile);
                        }

                        break;
                    }

                case UpdateAsset updateAsset:
                    {
                        await EnrichWithImageInfosAsync(updateAsset);
                        await EnrichWithHashAndUploadAsync(updateAsset, tempFile);

                        try
                        {
                            await HandleCoreAsync(context, next);

                            var asset = context.Result<IEnrichedAssetEntity>();

                            await assetStore.CopyAsync(tempFile, updateAsset.AssetId, asset.FileVersion, null);
                        }
                        finally
                        {
                            await assetStore.DeleteAsync(tempFile);
                        }

                        break;
                    }

                default:
                    await HandleCoreAsync(context, next);
                    break;
            }
        }

        private async Task HandleCoreAsync(CommandContext context, Func<Task> next)
        {
            await base.HandleAsync(context, next);

            if (context.PlainResult is IAssetEntity asset && !(context.PlainResult is IEnrichedAssetEntity))
            {
                var enriched = await assetEnricher.EnrichAsync(asset, contextProvider.Context);

                context.Complete(enriched);
            }
        }

        private static bool IsDuplicate(IAssetEntity asset, AssetFile file)
        {
            return asset?.FileName == file.FileName && asset.FileSize == file.FileSize;
        }

        private async Task EnrichWithImageInfosAsync(UploadAssetCommand command)
        {
            command.ImageInfo = await assetThumbnailGenerator.GetImageInfoAsync(command.File.OpenRead());
        }

        private async Task EnrichWithHashAndUploadAsync(UploadAssetCommand command, string tempFile)
        {
            using (var hashStream = new HasherStream(command.File.OpenRead(), HashAlgorithmName.SHA256))
            {
                await assetStore.UploadAsync(tempFile, hashStream);

                command.FileHash = $"{hashStream.GetHashStringAndReset()}{command.File.FileName}{command.File.FileSize}".Sha256Base64();
            }
        }

        private void GenerateTags(CreateAsset createAsset)
        {
            if (createAsset.Tags == null)
            {
                createAsset.Tags = new HashSet<string>();
            }

            foreach (var tagGenerator in tagGenerators)
            {
                tagGenerator.GenerateTags(createAsset, createAsset.Tags);
            }
        }
    }
}
