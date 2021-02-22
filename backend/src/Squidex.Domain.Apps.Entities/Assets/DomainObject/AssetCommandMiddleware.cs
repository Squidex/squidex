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
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
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
            this.assetMetadataSources = assetMetadataSources;
            this.contextProvider = contextProvider;
        }

        public override async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            switch (context.Command)
            {
                case CreateAsset createAsset:
                    {
                        var tempFile = context.ContextId.ToString();

                        try
                        {
                            await EnrichWithHashAndUploadAsync(createAsset, tempFile);

                            if (!createAsset.Duplicate)
                            {
                                var existing =
                                    await assetQuery.FindByHashAsync(contextProvider.Context,
                                        createAsset.FileHash,
                                        createAsset.File.FileName,
                                        createAsset.File.FileSize);

                                if (existing != null)
                                {
                                    context.Complete(new AssetDuplicate(existing));

                                    await next(context);
                                    return;
                                }
                            }

                            await EnrichWithMetadataAsync(createAsset);

                            await base.HandleAsync(context, next);
                        }
                        finally
                        {
                            await assetFileStore.DeleteAsync(tempFile);

                            createAsset.File.Dispose();
                        }

                        break;
                    }

                case UploadAssetCommand upload:
                    {
                        var tempFile = context.ContextId.ToString();

                        try
                        {
                            await EnrichWithHashAndUploadAsync(upload, tempFile);
                            await EnrichWithMetadataAsync(upload);

                            await base.HandleAsync(context, next);
                        }
                        finally
                        {
                            await assetFileStore.DeleteAsync(tempFile);

                            upload.File.Dispose();
                        }

                        break;
                    }

                default:
                    await base.HandleAsync(context, next);
                    break;
            }
        }

        protected override async Task<object> EnrichResultAsync(CommandContext context, CommandResult result)
        {
            var payload = await base.EnrichResultAsync(context, result);

            if (payload is IAssetEntity asset)
            {
                if (result.IsChanged && context.Command is UploadAssetCommand)
                {
                    var tempFile = context.ContextId.ToString();

                    await assetFileStore.CopyAsync(tempFile, asset.AppId.Id, asset.AssetId, asset.FileVersion);
                }

                if (payload is not IEnrichedAssetEntity)
                {
                    payload = await assetEnricher.EnrichAsync(asset, contextProvider.Context);
                }
            }

            return payload;
        }

        private async Task EnrichWithHashAndUploadAsync(UploadAssetCommand command, string tempFile)
        {
            using (var uploadStream = command.File.OpenRead())
            {
                using (var hashStream = new HasherStream(uploadStream, HashAlgorithmName.SHA256))
                {
                    await assetFileStore.UploadAsync(tempFile, hashStream);

                    command.FileHash = hashStream.GetHashStringAndReset();
                }
            }
        }

        private async Task EnrichWithMetadataAsync(UploadAssetCommand command)
        {
            foreach (var metadataSource in assetMetadataSources)
            {
                await metadataSource.EnhanceAsync(command);
            }
        }
    }
}
