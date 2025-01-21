// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Events;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class RecursiveDeleter(
    ICommandBus commandBus,
    IAssetRepository assetRepository,
    IAssetFolderRepository assetFolderRepository,
    TypeRegistry typeRegistry,
    ILogger<RecursiveDeleter> log)
    : IEventConsumer
{
    private readonly HashSet<string> consumingTypes =
        [
            typeRegistry.GetName<IEvent, AssetFolderDeleted>(),
        ];

    public StreamFilter EventsFilter { get; } = StreamFilter.Prefix("assetFolder-");

    public ValueTask<bool> HandlesAsync(StoredEvent @event)
    {
        return new ValueTask<bool>(consumingTypes.Contains(@event.Data.Type));
    }

    public async Task On(Envelope<IEvent> @event)
    {
        if (@event.Headers.Restored())
        {
            return;
        }

        if (@event.Payload is not AssetFolderDeleted folderDeleted)
        {
            return;
        }

        async Task PublishAsync(SquidexCommand command)
        {
            try
            {
                command.Actor = folderDeleted.Actor;

                await commandBus.PublishAsync(command, default);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to delete asset recursively.");
            }
        }

        var appId = folderDeleted.AppId;

        var childAssetFolders = await assetFolderRepository.QueryChildIdsAsync(appId.Id, folderDeleted.AssetFolderId);

        foreach (var assetFolderId in childAssetFolders)
        {
            await PublishAsync(new DeleteAssetFolder { AppId = appId, AssetFolderId = assetFolderId });
        }

        var childAssets = await assetRepository.QueryChildIdsAsync(appId.Id, folderDeleted.AssetFolderId);

        foreach (var assetId in childAssets)
        {
            await PublishAsync(new DeleteAsset { AppId = appId, AssetId = assetId });
        }
    }
}
