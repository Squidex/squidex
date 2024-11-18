// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class AssetPermanentDeleter(IAssetFileStore assetFileStore, TypeRegistry typeRegistry) : IEventConsumer
{
    private readonly HashSet<string> consumingTypes =
        [
            typeRegistry.GetName<IEvent, AssetDeleted>()
        ];

    public StreamFilter EventsFilter { get; } = StreamFilter.Prefix("asset-");

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

        if (@event.Payload is AssetDeleted assetDeleted)
        {
            try
            {
                await assetFileStore.DeleteAsync(assetDeleted.AppId.Id, assetDeleted.AssetId);
            }
            catch (AssetNotFoundException)
            {
                return;
            }
        }
    }
}
