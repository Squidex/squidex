// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetHistoryEventsCreator : HistoryEventsCreatorBase
    {
        public AssetHistoryEventsCreator(TypeNameRegistry typeNameRegistry)
            : base(typeNameRegistry)
        {
            AddEventMessage<AssetCreated>(
                "history.assets.uploaded");

            AddEventMessage<AssetUpdated>(
                "history.assets.replaced");

            AddEventMessage<AssetAnnotated>(
                "history.assets.updated");
        }

        protected override Task<HistoryEvent?> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            HistoryEvent? result = null;

            if (@event.Payload is AssetEvent assetEvent)
            {
                var channel = $"assets.{assetEvent.AssetId}";

                result = ForEvent(@event.Payload, channel);
            }

            return Task.FromResult(result);
        }
    }
}
