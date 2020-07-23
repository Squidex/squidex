// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities
{
    public sealed class EventStoreInitializer : IInitializable
    {
        private readonly IEventStore eventStore;

        public int Order => 1000;

        public EventStoreInitializer(IEventStore eventStore)
        {
            Guard.NotNull(eventStore, nameof(eventStore));

            this.eventStore = eventStore;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            return eventStore.CreateIndexAsync(SquidexHeaders.AppId);
        }
    }
}
