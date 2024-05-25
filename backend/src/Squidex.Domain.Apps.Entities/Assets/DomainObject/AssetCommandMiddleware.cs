// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Cryptography;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.Queries;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject;

public sealed class AssetCommandMiddleware : CachingDomainObjectMiddleware<AssetCommand, AssetDomainObject, Asset>
{
    private readonly IAssetFileStore assetFileStore;
    private readonly IAssetEnricher assetEnricher;
    private readonly IAssetQueryService assetQuery;
    private readonly IContextProvider contextProvider;
    private readonly IEnumerable<IAssetMetadataSource> assetMetadataSources;

    public AssetCommandMiddleware(
        IDomainObjectFactory domainObjectFactory,
        IDomainObjectCache domainObjectCache,
        IAssetEnricher assetEnricher,
        IAssetFileStore assetFileStore,
        IAssetQueryService assetQuery,
        IContextProvider contextProvider,
        IEnumerable<IAssetMetadataSource> assetMetadataSources)
        : base(domainObjectFactory, domainObjectCache)
    {
        this.assetEnricher = assetEnricher;
        this.assetFileStore = assetFileStore;
        this.assetMetadataSources = assetMetadataSources.OrderBy(x => x.Order).ToList();
        this.assetQuery = assetQuery;
        this.contextProvider = contextProvider;
    }

    public override async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        switch (context.Command)
        {
            case CreateAsset create:
                await UploadWithDuplicateCheckAsync(context, create, create.Duplicate, next, ct);
                break;

            case UpsertAsset upsert:
                await UploadWithDuplicateCheckAsync(context, upsert, upsert.Duplicate, next, ct);
                break;

            case MoveAsset move:
                await base.HandleAsync(context, next, ct);
                break;

            case UpdateAsset upload:
                await UploadAndHandleAsync(context, upload, next, ct);
                break;

            default:
                await base.HandleAsync(context, next, ct);
                break;
        }
    }

    private async Task UploadWithDuplicateCheckAsync(CommandContext context, UploadAssetCommand command, bool duplicate, NextDelegate next,
        CancellationToken ct)
    {
        // Add the file Id to the command, so we can access it later in the script.
        command.FileId = context.ContextId.ToString();

        try
        {
            await EnrichWithMetadataAsync(command, ct);
            await EnrichWithHashAndUploadAsync(command, command.FileId, ct);

            if (!duplicate)
            {
                var existing =
                    await assetQuery.FindByHashAsync(contextProvider.Context,
                        command.FileHash,
                        command.File.FileName,
                        command.File.FileSize,
                        ct);

                if (existing != null)
                {
                    context.Complete(new AssetDuplicate(existing));

                    await next(context, ct);
                    return;
                }
            }

            await base.HandleAsync(context, next, ct);
        }
        finally
        {
            await assetFileStore.DeleteAsync(command.FileId, ct);
        }
    }

    private async Task UploadAndHandleAsync(CommandContext context, UploadAssetCommand command, NextDelegate next,
        CancellationToken ct)
    {
        // Add the file Id to the command, so we can access it later in the script.
        command.FileId = context.ContextId.ToString();

        try
        {
            await EnrichWithMetadataAsync(command, ct);
            await EnrichWithHashAndUploadAsync(command, command.FileId, ct);

            await base.HandleAsync(context, next, ct);
        }
        finally
        {
            await assetFileStore.DeleteAsync(command.FileId, ct);
        }
    }

    protected override async Task<object> EnrichResultAsync(CommandContext context, CommandResult result,
        CancellationToken ct)
    {
        var payload = await base.EnrichResultAsync(context, result, ct);

        if (payload is not Asset asset)
        {
            return payload;
        }

        if (result.IsChanged && context.Command is UploadAssetCommand)
        {
            var tempFile = context.ContextId.ToString();
            try
            {
                await assetFileStore.CopyAsync(tempFile, asset.AppId.Id, asset.Id, asset.FileVersion, null, ct);
            }
            catch (AssetAlreadyExistsException)
            {
            }
        }

        if (payload is not EnrichedAsset)
        {
            payload = await assetEnricher.EnrichAsync(asset, contextProvider.Context, ct);
        }

        return payload;
    }

    private async Task EnrichWithHashAndUploadAsync(UploadAssetCommand command, string tempFile,
        CancellationToken ct)
    {
        await using (var uploadStream = command.File.OpenRead())
        {
            await using (var hashStream = new HasherStream(uploadStream, HashAlgorithmName.SHA256))
            {
                await assetFileStore.UploadAsync(tempFile, hashStream, ct);

                command.FileHash = ComputeHash(command.File, hashStream);
            }
        }
    }

    private static string ComputeHash(IAssetFile file, HasherStream hashStream)
    {
        var steamHash = hashStream.GetHashStringAndReset();

        return $"{steamHash}{file.FileName}{file.FileSize}".ToSha256Base64();
    }

    private async Task EnrichWithMetadataAsync(UploadAssetCommand command,
        CancellationToken ct)
    {
        foreach (var metadataSource in assetMetadataSources)
        {
            await metadataSource.EnhanceAsync(command, ct);
        }
    }
}
