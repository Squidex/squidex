// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MassTransit;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public sealed class EventConsumerManager : IEventConsumerManager
    {
        private readonly ISnapshotStore<EventConsumerState> snapshotStore;
        private readonly IBus bus;

        public EventConsumerManager(ISnapshotStore<EventConsumerState> snapshotStore, IBus bus)
        {
            this.snapshotStore = snapshotStore;

            this.bus = bus;
        }

        public async Task<List<EventConsumerInfo>> GetConsumersAsync(
            CancellationToken ct = default)
        {
            var snapshots = await snapshotStore.ReadAllAsync(ct).ToListAsync(ct);

            return snapshots.Select(x => x.Value.ToInfo(x.Key.ToString())).ToList();
        }

        public Task ResetAsync(string consumerName)
        {
            return bus.Publish(new ResetEventConsumer(consumerName));
        }

        public Task StartAsync(string consumerName)
        {
            return bus.Publish(new StartEventConsumer(consumerName));
        }

        public Task StopAsync(string consumerName)
        {
            return bus.Publish(new StopEventConsumer(consumerName));
        }
    }
}
