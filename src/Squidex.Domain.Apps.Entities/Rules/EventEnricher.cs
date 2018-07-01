// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using NodaTime;
using Orleans;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public sealed class EventEnricher : IEventEnricher
    {
        private readonly IGrainFactory grainFactory;
        private readonly IClock clock;

        public EventEnricher(IGrainFactory grainFactory, IClock clock)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));
            Guard.NotNull(clock, nameof(clock));

            this.grainFactory = grainFactory;

            this.clock = clock;
        }

        public Task<EnrichedEvent> EnrichAsync(Envelope<AppEvent> @event)
        {
            Guard.NotNull(@event, nameof(@event));

            if (@event.Payload is ContentEvent contentEvent)
            {
                return CreateContentEventAsync(contentEvent, @event);
            }

            if (@event.Payload is AssetEvent assetEvent)
            {
            }

            return Task.FromResult<EnrichedEvent>(null);
        }

        private async Task<EnrichedEvent> CreateContentEventAsync(ContentEvent contentEvent, Envelope<AppEvent> @event)
        {
            var result = new EnrichedContentEvent();

            var content =
                (await grainFactory
                    .GetGrain<IContentGrain>(contentEvent.ContentId)
                    .GetStateAsync(@event.Headers.EventStreamNumber())).Value;

            SimpleMapper.Map(content, result);

            result.Data = content.Data ?? content.DataDraft;

            switch (contentEvent)
            {
                case ContentCreated e:
                    result.Action = EnrichedContentEventAction.Created;
                    break;
                case ContentDeleted e:
                    result.Action = EnrichedContentEventAction.Deleted;
                    break;
                case ContentUpdated e:
                    result.Action = EnrichedContentEventAction.Updated;
                    break;
                case ContentStatusChanged e:
                    if (e.Status == Status.Published)
                    {
                        result.Action = EnrichedContentEventAction.Published;
                    }
                    else
                    {
                        result.Action = EnrichedContentEventAction.Unpublished;
                    }

                    break;
            }

            result.Name = $"{content.SchemaId.Name.ToPascalCase()}{result.Action}";

            SetDefault(result, @event);

            return result;
        }

        private void SetDefault(EnrichedEvent result, Envelope<AppEvent> @event)
        {
            result.Timestamp =
                @event.Headers.Contains(CommonHeaders.Timestamp) ?
                @event.Headers.Timestamp() :
                clock.GetCurrentInstant();

            result.AggregateId =
                @event.Headers.Contains(CommonHeaders.AggregateId) ?
                @event.Headers.AggregateId() :
                Guid.NewGuid();

            if (@event.Payload is SquidexEvent squidexEvent)
            {
                result.Actor = squidexEvent.Actor;
            }

            if (@event.Payload is AppEvent appEvent)
            {
                result.AppId = appEvent.AppId;
            }
        }
    }
}
