// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Infrastructure.EventSourcing
{
    public delegate IEventConsumer EventConsumerFactory(string name);

    public interface IEventConsumer
    {
        int BatchDelay => 500;

        int BatchSize => 1;

        string Name { get; }

        string EventsFilter => ".*";

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
}
