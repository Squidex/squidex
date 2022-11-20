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
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class RecursiveDeleter : IEventConsumer
{
    private readonly ICommandBus commandBus;
    private readonly IAssetRepository assetRepository;
    private readonly IAssetFolderRepository assetFolderRepository;
    private readonly ILogger<RecursiveDeleter> log;
    private readonly HashSet<string> consumingTypes;

    public string Name
    {
        get => GetType().Name;
    }

    public string EventsFilter
    {
        get => "^assetFolder-";
    }

    public RecursiveDeleter(
        ICommandBus commandBus,
        IAssetRepository assetRepository,
        IAssetFolderRepository assetFolderRepository,
        TypeRegistry typeRegistry,
        ILogger<RecursiveDeleter> log)
    {
        this.commandBus = commandBus;
        this.assetRepository = assetRepository;
        this.assetFolderRepository = assetFolderRepository;

        this.log = log;

        // Compute the event types names once for performance reasons and use hashset for extensibility.
        consumingTypes = new HashSet<string>
        {
            typeRegistry.GetName<IEvent, AssetFolderDeleted>()
        };
    }

    public bool Handles(StoredEvent @event)
    {
        return consumingTypes.Contains(@event.Data.Type);
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
