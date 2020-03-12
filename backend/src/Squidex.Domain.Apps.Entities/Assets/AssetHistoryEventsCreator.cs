// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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
                "uploaded asset.");

            AddEventMessage<AssetUpdated>(
                "replaced asset.");

            AddEventMessage<AssetAnnotated>(
                "updated asset.");
        }

        protected override Task<HistoryEvent?> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            var channel = $"assets.{@event.Headers.AggregateId()}";

            var result = ForEvent(@event.Payload, channel);

            return Task.FromResult<HistoryEvent?>(result);
        }
    }
}
