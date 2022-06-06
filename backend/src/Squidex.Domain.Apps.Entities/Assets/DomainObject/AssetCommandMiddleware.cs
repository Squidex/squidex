// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Cryptography;
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
            this.assetEnricher = assetEnricher;
            this.assetFileStore = assetFileStore;
            this.assetMetadataSources = assetMetadataSources.OrderBy(x => x.Order).ToList();
            this.assetQuery = assetQuery;
            this.contextProvider = contextProvider;
        }

        public override async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            switch (context.Command)
            {
                case CreateAsset create:
                    await UploadWithDuplicateCheckAsync(context, create, create.Duplicate, next);
                    break;

                case UpsertAsset upsert:
                    await UploadWithDuplicateCheckAsync(context, upsert, upsert.Duplicate, next);
                    break;

                case MoveAsset move:
                    await base.HandleAsync(context, next);
                    break;

                case UpdateAsset upload:
                    await UploadAndHandleAsync(context, upload, next);
                    break;

                default:
                    await base.HandleAsync(context, next);
                    break;
            }
        }

        private async Task UploadWithDuplicateCheckAsync(CommandContext context, UploadAssetCommand command, bool duplicate, NextDelegate next)
        {
            var tempFile = context.ContextId.ToString();

            try
            {
                await EnrichWithHashAndUploadAsync(command, tempFile);

                if (!duplicate)
                {
                    var existing =
                        await assetQuery.FindByHashAsync(contextProvider.Context,
                            command.FileHash,
                            command.File.FileName,
                            command.File.FileSize);

                    if (existing != null)
                    {
                        context.Complete(new AssetDuplicate(existing));

                        await next(context);
                        return;
                    }
                }

                await EnrichWithMetadataAsync(command);

                await base.HandleAsync(context, next);
            }
            finally
            {
                await assetFileStore.DeleteAsync(tempFile);

                await command.File.DisposeAsync();
            }
        }

        private async Task UploadAndHandleAsync(CommandContext context, UploadAssetCommand command, NextDelegate next)
        {
            var tempFile = context.ContextId.ToString();

            try
            {
                await EnrichWithHashAndUploadAsync(command, tempFile);
                await EnrichWithMetadataAsync(command);

                await base.HandleAsync(context, next);
            }
            finally
            {
                await assetFileStore.DeleteAsync(tempFile);

                await command.File.DisposeAsync();
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

                    try
                    {
                        await assetFileStore.CopyAsync(tempFile, asset.AppId.Id, asset.AssetId, asset.FileVersion, null);
                    }
                    catch (AssetAlreadyExistsException) when (context.Command is not UpsertAsset)
                    {
                        throw;
                    }
                }

                if (payload is not IEnrichedAssetEntity)
                {
                    payload = await assetEnricher.EnrichAsync(asset, contextProvider.Context, default);
                }
            }

            return payload;
        }

        private async Task EnrichWithHashAndUploadAsync(UploadAssetCommand command, string tempFile)
        {
            await using (var uploadStream = command.File.OpenRead())
            {
                using (var hashStream = new HasherStream(uploadStream, HashAlgorithmName.SHA256))
                {
                    await assetFileStore.UploadAsync(tempFile, hashStream);

                    command.FileHash = ComputeHash(command.File, hashStream);
                }
            }
        }

        private static string ComputeHash(AssetFile file, HasherStream hashStream)
        {
            var steamHash = hashStream.GetHashStringAndReset();

            return $"{steamHash}{file.FileName}{file.FileSize}".ToSha256Base64();
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
