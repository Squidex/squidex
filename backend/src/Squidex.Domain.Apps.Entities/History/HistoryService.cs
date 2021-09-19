// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public int BatchSize
        {
            get => 1000;
        }

        public int BatchDelay
        {
            get => 1000;
        }

        public string Name
        {
            get => GetType().Name;
        }

        public HistoryService(IHistoryEventRepository repository, IEnumerable<IHistoryEventsCreator> creators, NotifoService notifo)
        {
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

        public Task ClearAsync()
        {
            return repository.ClearAsync();
        }

        public async Task On(IEnumerable<Envelope<IEvent>> events)
        {
            var targets = new List<(Envelope<AppEvent> Event, HistoryEvent? HistoryEvent)>();

            foreach (var @event in events)
            {
                if (@event.Payload is AppEvent)
                {
                    var appEvent = @event.To<AppEvent>();

                    HistoryEvent? historyEvent = null;

                    foreach (var creator in creators)
                    {
                        historyEvent = await creator.CreateEventAsync(@event);

                        if (historyEvent != null)
                        {
                            historyEvent.Actor = appEvent.Payload.Actor;
                            historyEvent.AppId = appEvent.Payload.AppId.Id;
                            historyEvent.Created = @event.Headers.Timestamp();
                            historyEvent.Version = @event.Headers.EventStreamNumber();

                            break;
                        }
                    }

                    targets.Add((appEvent, historyEvent));
                }
            }

            if (targets.Count > 0)
            {
                await notifo.HandleEventsAsync(targets);

                await repository.InsertManyAsync(targets.NotNull(x => x.HistoryEvent));
            }
        }

        public async Task<IReadOnlyList<ParsedHistoryEvent>> QueryByChannelAsync(DomainId appId, string channelPrefix, int count,
            CancellationToken ct = default)
        {
            var items = await repository.QueryByChannelAsync(appId, channelPrefix, count, ct);

            return items.Select(x => new ParsedHistoryEvent(x, texts)).ToList();
        }
    }
}
