// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing;

public interface IEventConsumer
{
    int BatchDelay => 500;

    int BatchSize => 1;

    string Name { get; }

    string EventsFilter => ".*";

    bool StartLatest => false;

    bool CanClear => true;

    bool Handles(StoredEvent @event)
    {
        return true;
    }

    Task ClearAsync()
    {
        return Task.CompletedTask;
    }

    Task On(Envelope<IEvent> @event)
    {
        return Task.CompletedTask;
    }

    async Task On(IEnumerable<Envelope<IEvent>> events)
    {
        foreach (var @event in events)
        {
            await On(@event);
        }
    }
}
