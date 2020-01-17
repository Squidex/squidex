// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetCommandMiddleware : GrainCommandMiddleware<AssetCommand, IAssetGrain>
    {
        private readonly IAssetFileStore assetFileStore;
        private readonly IAssetEnricher assetEnricher;
        private readonly IAssetQueryService assetQuery;
        private readonly IContextProvider contextProvider;
        private readonly IEnumerable<IAssetMetadataSource> assetMetadataSources;

        public AssetCommandMiddleware(
            IGrainFactory grainFactory,
            IAssetEnricher assetEnricher,
            IAssetFileStore assetFileStore,
            IAssetQueryService assetQuery,
            IContextProvider contextProvider,
            IEnumerable<IAssetMetadataSource> assetMetadataSources)
            : base(grainFactory)
        {
            Guard.NotNull(assetEnricher);
            Guard.NotNull(assetFileStore);
            Guard.NotNull(assetQuery);
            Guard.NotNull(assetMetadataSources);
            Guard.NotNull(contextProvider);

            this.assetFileStore = assetFileStore;
            this.assetEnricher = assetEnricher;
            this.assetQuery = assetQuery;
            this.contextProvider = contextProvider;
            this.assetMetadataSources = assetMetadataSources;
        }

        public override async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            var tempFile = context.ContextId.ToString();

            switch (context.Command)
            {
                case CreateAsset createAsset:
                    {
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

                                    await next(context);
                                    return;
                                }
                            }

                            await EnrichWithMetadataAsync(createAsset, createAsset.Tags);

                            await HandleCoreAsync(context, next);

                            var asset = context.Result<IEnrichedAssetEntity>();

                            context.Complete(new AssetCreatedResult(asset, false));

                            await assetFileStore.CopyAsync(tempFile, createAsset.AssetId, asset.FileVersion);
                        }
                        finally
                        {
                            await assetFileStore.DeleteAsync(tempFile);
                        }

                        break;
                    }

                case UpdateAsset updateAsset:
                    {
                        await EnrichWithMetadataAsync(updateAsset);
                        await EnrichWithHashAndUploadAsync(updateAsset, tempFile);

                        try
                        {
                            await HandleCoreAsync(context, next);

                            var asset = context.Result<IEnrichedAssetEntity>();

                            await assetFileStore.CopyAsync(tempFile, updateAsset.AssetId, asset.FileVersion);
                        }
                        finally
                        {
                            await assetFileStore.DeleteAsync(tempFile);
                        }

                        break;
                    }

                default:
                    await HandleCoreAsync(context, next);
                    break;
            }
        }

        private async Task HandleCoreAsync(CommandContext context, NextDelegate next)
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

        private async Task EnrichWithHashAndUploadAsync(UploadAssetCommand command, string tempFile)
        {
            using (var hashStream = new HasherStream(command.File.OpenRead(), HashAlgorithmName.SHA256))
            {
                await assetFileStore.UploadAsync(tempFile, hashStream);

                command.FileHash = $"{hashStream.GetHashStringAndReset()}{command.File.FileName}{command.File.FileSize}".Sha256Base64();
            }
        }

        private async Task EnrichWithMetadataAsync(UploadAssetCommand command, HashSet<string>? tags = null)
        {
            foreach (var metadataSource in assetMetadataSources)
            {
                await metadataSource.EnhanceAsync(command, tags);
            }
        }
    }
}
