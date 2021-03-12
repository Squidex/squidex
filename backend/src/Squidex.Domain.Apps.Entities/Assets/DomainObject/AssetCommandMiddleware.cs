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
using Squidex.Domain.Apps.Entities.Assets.Folders;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public sealed class AssetCommandMiddleware : GrainCommandMiddleware<AssetCommand, IAssetGrain>
    {
        private readonly IAssetFileStore assetFileStore;
        private readonly IAssetFolderResolver assetFolderResolver;
        private readonly IAssetEnricher assetEnricher;
        private readonly IAssetQueryService assetQuery;
        private readonly IContextProvider contextProvider;
        private readonly IEnumerable<IAssetMetadataSource> assetMetadataSources;

        public AssetCommandMiddleware(
            IGrainFactory grainFactory,
            IAssetEnricher assetEnricher,
            IAssetFileStore assetFileStore,
            IAssetFolderResolver assetFolderResolver,
            IAssetQueryService assetQuery,
            IContextProvider contextProvider,
            IEnumerable<IAssetMetadataSource> assetMetadataSources)
            : base(grainFactory)
        {
            Guard.NotNull(assetEnricher, nameof(assetEnricher));
            Guard.NotNull(assetFileStore, nameof(assetFileStore));
            Guard.NotNull(assetFolderResolver, nameof(assetFolderResolver));
            Guard.NotNull(assetMetadataSources, nameof(assetMetadataSources));
            Guard.NotNull(assetQuery, nameof(assetQuery));
            Guard.NotNull(contextProvider, nameof(contextProvider));

            this.assetEnricher = assetEnricher;
            this.assetFileStore = assetFileStore;
            this.assetFolderResolver = assetFolderResolver;
            this.assetMetadataSources = assetMetadataSources;
            this.assetQuery = assetQuery;
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

                            await EnrichWithParentAsync(createAsset);
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

                case MoveAsset move:
                    {
                        await EnrichWithParentAsync(move);

                        await base.HandleAsync(context, next);

                        break;
                    }

                case UpsertAsset upsert:
                    {
                        await EnrichWithParentAsync(upsert);

                        await UploadAndHandleAsync(context, next, upsert);

                        break;
                    }

                case UpdateAsset upload:
                    {
                        await UploadAndHandleAsync(context, next, upload);

                        break;
                    }

                default:
                    await base.HandleAsync(context, next);
                    break;
            }
        }

        private async Task EnrichWithParentAsync<T>(T command) where T : AssetCommand, IMoveAssetCommand
        {
            var request = new ResolveRequest(command.User!)
            {
                ParentId = command.ParentId,
                ParentPath = command.ParentPath,
                Create = true
            };

            command.ParentId = await assetFolderResolver.ResolveOrCreateAsync(command.AppId, request);
        }

        private async Task EnrichWithParentAsync(UpsertAsset command)
        {
            if (command.ParentId == null && string.IsNullOrWhiteSpace(command.ParentPath))
            {
                return;
            }

            var request = new ResolveRequest(command.User!)
            {
                ParentId = command.ParentId,
                ParentPath = command.ParentPath,
                Create = true
            };

            command.ParentId = await assetFolderResolver.ResolveOrCreateAsync(command.AppId, request);
        }

        private async Task UploadAndHandleAsync(CommandContext context, NextDelegate next, UploadAssetCommand upload)
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
                        await assetFileStore.CopyAsync(tempFile, asset.AppId.Id, asset.AssetId, asset.FileVersion);
                    }
                    catch (AssetAlreadyExistsException) when (context.Command is not UpsertAsset)
                    {
                        throw;
                    }
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

                    var hash = $"{hashStream.GetHashStringAndReset()}{command.File.FileName}{command.File.FileSize}".Sha256Base64();

                    command.FileHash = hash;
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
