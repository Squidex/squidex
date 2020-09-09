// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.EventSourcing
{
    public delegate IEventConsumer EventConsumerFactory(string name);

    public interface IEventConsumer
    {
        int BatchSize => 1;

        string Name { get; }

        string EventsFilter { get; }

        bool Handles(StoredEvent @event)
        {
            return true;
        }

        Task ClearAsync()
        {
            return Task.CompletedTask;
        }

        async Task On(IEnumerable<Envelope<IEvent>> @events)
        {
            foreach (var @event in events)
            {
                await On(@event);
            }
        }

        Task On(Envelope<IEvent> @event);
    }
}