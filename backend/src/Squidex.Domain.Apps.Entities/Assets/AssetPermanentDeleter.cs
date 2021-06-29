// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Assets;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetPermanentDeleter : IEventConsumer
    {
        private readonly IAssetFileStore assetFileStore;
        private readonly string? deletedType;

        public string Name
        {
            get => GetType().Name;
        }

        public string EventsFilter
        {
            get => "^asset-";
        }

        public AssetPermanentDeleter(IAssetFileStore assetFileStore, TypeNameRegistry typeNameRegistry)
        {
            this.assetFileStore = assetFileStore;

            deletedType = typeNameRegistry?.GetName<AssetDeleted>();
        }

        public bool Handles(StoredEvent @event)
        {
            return @event.Data.Type == deletedType;
        }

        public async Task On(Envelope<IEvent> @event)
        {
            if (@event.Headers.Restored())
            {
                return;
            }

            if (@event.Payload is AssetDeleted assetDeleted)
            {
                for (var version = 0; version < @event.Headers.EventStreamNumber(); version++)
                {
                    try
                    {
                        await assetFileStore.DeleteAsync(assetDeleted.AppId.Id, assetDeleted.AssetId, version, null);
                    }
                    catch (AssetNotFoundException)
                    {
                        continue;
                    }
                }
            }
        }
    }
}
