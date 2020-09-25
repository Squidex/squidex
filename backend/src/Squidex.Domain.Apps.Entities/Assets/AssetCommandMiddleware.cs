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
            Guard.NotNull(assetEnricher, nameof(assetEnricher));
            Guard.NotNull(assetFileStore, nameof(assetFileStore));
            Guard.NotNull(assetQuery, nameof(assetQuery));
            Guard.NotNull(assetMetadataSources, nameof(assetMetadataSources));
            Guard.NotNull(contextProvider, nameof(contextProvider));

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
                        try
                        {
                            await EnrichWithHashAndUploadAsync(createAsset, tempFile);

                            var ctx = contextProvider.Context.Clone().WithoutAssetEnrichment();

                            if (!createAsset.Duplicate)
                            {
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
                            }

                            await UploadAsync(context, tempFile, createAsset, createAsset.Tags, true, next);
                        }
                        finally
                        {
                            await assetFileStore.DeleteAsync(tempFile);

                            createAsset.File.Dispose();
                        }

                        break;
                    }

                case UpdateAsset updateAsset:
                    {
                        try
                        {
                            await EnrichWithHashAndUploadAsync(updateAsset, tempFile);

                            await UploadAsync(context, tempFile, updateAsset, null, false, next);
                        }
                        finally
                        {
                            await assetFileStore.DeleteAsync(tempFile);

                            updateAsset.File.Dispose();
                        }

                        break;
                    }

                default:
                    await HandleCoreAsync(context, false, next);
                    break;
            }
        }

        private async Task UploadAsync(CommandContext context, string tempFile, UploadAssetCommand command, HashSet<string>? tags, bool created, NextDelegate next)
        {
            await EnrichWithMetadataAsync(command, tags);

            var asset = await HandleCoreAsync(context, created, next);

            if (asset != null)
            {
                await assetFileStore.CopyAsync(tempFile, command.AppId.Id, command.AssetId, asset.FileVersion);
            }
        }

        private async Task<IEnrichedAssetEntity?> HandleCoreAsync(CommandContext context, bool created, NextDelegate next)
        {
            await base.HandleAsync(context, next);

            if (context.PlainResult is IAssetEntity asset && !(context.PlainResult is IEnrichedAssetEntity))
            {
                var enriched = await assetEnricher.EnrichAsync(asset, contextProvider.Context);

                if (created)
                {
                    context.Complete(new AssetCreatedResult(enriched, false));
                }
                else
                {
                    context.Complete(enriched);
                }

                return enriched;
            }

            return null;
        }

        private static bool IsDuplicate(IAssetEntity asset, AssetFile file)
        {
            return asset?.FileName == file.FileName && asset.FileSize == file.FileSize;
        }

        private async Task EnrichWithHashAndUploadAsync(UploadAssetCommand command, string tempFile)
        {
            using (var uploadStream = command.File.OpenRead())
            {
                using (var hashStream = new HasherStream(uploadStream, HashAlgorithmName.SHA256))
                {
                    await assetFileStore.UploadAsync(tempFile, hashStream);

                    command.FileHash = $"{hashStream.GetHashStringAndReset()}{command.File.FileName}{command.File.FileSize}".Sha256Base64();
                }
            }
        }

        private async Task EnrichWithMetadataAsync(UploadAssetCommand command, HashSet<string>? tags)
        {
            foreach (var metadataSource in assetMetadataSources)
            {
                await metadataSource.EnhanceAsync(command, tags);
            }
        }
    }
}
