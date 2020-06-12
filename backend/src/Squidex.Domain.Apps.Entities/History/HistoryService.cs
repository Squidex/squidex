// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.History
{
    public sealed class HistoryService : IHistoryService, IEventConsumer
    {
        private readonly Dictionary<string, string> texts = new Dictionary<string, string>();
        private readonly List<IHistoryEventsCreator> creators;
        private readonly IHistoryEventRepository repository;
        private readonly NotifoService notifo;

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return ".*"; }
        }

        public HistoryService(IHistoryEventRepository repository, IEnumerable<IHistoryEventsCreator> creators, NotifoService notifo)
        {
            Guard.NotNull(repository, nameof(repository));
            Guard.NotNull(creators, nameof(creators));
            Guard.NotNull(notifo, nameof(notifo));

            this.creators = creators.ToList();

            foreach (var creator in this.creators)
            {
                foreach (var (key, value) in creator.Texts)
                {
                    texts[key] = value;
                }
            }

            this.repository = repository;

            this.notifo = notifo;
        }

        public bool Handles(StoredEvent @event)
        {
            return true;
        }

        public Task ClearAsync()
        {
            return repository.ClearAsync();
        }

        public async Task On(Envelope<IEvent> @event)
        {
            await notifo.HandleEventAsync(@event);

            foreach (var creator in creators)
            {
                var historyEvent = await creator.CreateEventAsync(@event);

                if (historyEvent != null)
                {
                    var appEvent = @event.To<AppEvent>();

                    await notifo.HandleHistoryEventAsync(appEvent, historyEvent);

                    historyEvent.Actor = appEvent.Payload.Actor;
                    historyEvent.AppId = appEvent.Payload.AppId.Id;
                    historyEvent.Created = @event.Headers.Timestamp();
                    historyEvent.Version = @event.Headers.EventStreamNumber();

                    await repository.InsertAsync(historyEvent);
                }
            }
        }

        public async Task<IReadOnlyList<ParsedHistoryEvent>> QueryByChannelAsync(Guid appId, string channelPrefix, int count)
        {
            var items = await repository.QueryByChannelAsync(appId, channelPrefix, count);

            return items.Select(x => new ParsedHistoryEvent(x, texts)).ToList();
        }
    }
}
