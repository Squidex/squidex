// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.History;

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

    public HistoryService(IHistoryEventRepository repository, IEnumerable<IHistoryEventsCreator> creators,
        NotifoService notifo)
    {
        this.creators = creators.ToList();
        this.repository = repository;
        this.notifo = notifo;

        foreach (var creator in this.creators)
        {
            foreach (var (key, value) in creator.Texts)
            {
                texts[key] = value;
            }
        }
    }

    public Task ClearAsync()
    {
        return repository.ClearAsync();
    }

    public async Task On(IEnumerable<Envelope<IEvent>> events)
    {
        var targets = new List<(Envelope<IEvent> Event, HistoryEvent? HistoryEvent)>();

        foreach (var @event in events)
        {
            switch (@event.Payload)
            {
                case AppEvent appEvent:
                    {
                        var historyEvent = await CreateEvent(appEvent.AppId.Id, appEvent.Actor, @event);

                        if (historyEvent != null)
                        {
                            targets.Add((@event, historyEvent));
                        }

                        break;
                    }

                case TeamEvent teamEvent:
                    {
                        var historyEvent = await CreateEvent(teamEvent.TeamId, teamEvent.Actor, @event);

                        if (historyEvent != null)
                        {
                            targets.Add((@event, historyEvent));
                        }

                        break;
                    }
            }
        }

        if (targets.Count > 0)
        {
            await notifo.HandleEventsAsync(targets);

            await repository.InsertManyAsync(targets.NotNull(x => x.HistoryEvent));
        }
    }

    private async Task<HistoryEvent?> CreateEvent(DomainId ownerId, RefToken actor, Envelope<IEvent> @event)
    {
        foreach (var creator in creators)
        {
            var historyEvent = await creator.CreateEventAsync(@event);

            if (historyEvent != null)
            {
                historyEvent.Actor = actor;
                historyEvent.OwnerId = ownerId;
                historyEvent.Created = @event.Headers.Timestamp();
                historyEvent.Version = @event.Headers.EventStreamNumber();
                return historyEvent;
            }
        }

        return null;
    }

    public async Task<IReadOnlyList<ParsedHistoryEvent>> QueryByChannelAsync(DomainId ownerId, string channelPrefix, int count,
        CancellationToken ct = default)
    {
        var items = await repository.QueryByChannelAsync(ownerId, channelPrefix, count, ct);

        return items.Select(x => new ParsedHistoryEvent(x, texts)).ToList();
    }
}
