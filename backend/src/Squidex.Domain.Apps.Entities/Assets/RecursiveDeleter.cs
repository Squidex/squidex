// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class RecursiveDeleter : IEventConsumer
    {
        private readonly ICommandBus commandBus;
        private readonly IAssetRepository assetRepository;
        private readonly IAssetFolderRepository assetFolderRepository;
        private readonly ISemanticLog log;
        private readonly string folderDeletedType;

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^assetFolder\\-"; }
        }

        public RecursiveDeleter(
            ICommandBus commandBus,
            IAssetRepository assetRepository,
            IAssetFolderRepository assetFolderRepository,
            TypeNameRegistry typeNameRegistry,
            ISemanticLog log)
        {
            Guard.NotNull(commandBus, nameof(commandBus));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(assetFolderRepository, nameof(assetFolderRepository));
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));
            Guard.NotNull(log, nameof(log));

            this.commandBus = commandBus;
            this.assetRepository = assetRepository;
            this.assetFolderRepository = assetFolderRepository;
            this.log = log;

            folderDeletedType = typeNameRegistry.GetName<AssetFolderDeleted>();
        }

        public bool Handles(StoredEvent @event)
        {
            return @event.Data.Type == folderDeletedType;
        }

        public async Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is AssetFolderDeleted folderDeleted)
            {
                async Task PublishAsync(SquidexCommand command)
                {
                    try
                    {
                        command.Actor = folderDeleted.Actor;

                        await commandBus.PublishAsync(command);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, w => w
                            .WriteProperty("action", "DeleteAssetsRecursive")
                            .WriteProperty("status", "Failed"));
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
    }
}
