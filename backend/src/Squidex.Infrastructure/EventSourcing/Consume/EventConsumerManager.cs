// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.States;
using Squidex.Messaging;

namespace Squidex.Infrastructure.EventSourcing.Consume;

public sealed class EventConsumerManager : IEventConsumerManager
{
    private readonly IPersistenceFactory<EventConsumerState> persistence;
    private readonly IMessageBus messaging;
    private readonly HashSet<string> activeNames;

    public EventConsumerManager(IPersistenceFactory<EventConsumerState> persistence, IEnumerable<IEventConsumer> eventConsumers,
        IMessageBus messaging)
    {
        this.persistence = persistence;
        this.messaging = messaging;
        this.activeNames = eventConsumers.Select(x => x.Name).ToHashSet();
    }

    public async Task<List<EventConsumerInfo>> GetConsumersAsync(
        CancellationToken ct = default)
    {
        var snapshots = await persistence.Snapshots.ReadAllAsync(ct).ToListAsync(ct);

        return snapshots.Where(x => activeNames.Contains(x.Key.ToString())).Select(x => x.Value.ToInfo(x.Key.ToString())).ToList();
    }

    public async Task<EventConsumerInfo> ResetAsync(string consumerName,
        CancellationToken ct = default)
    {
        var state = await GetStateAsync(consumerName, ct);

        await messaging.PublishAsync(new EventConsumerReset(consumerName), ct: ct);

        return state.Value.ToInfo(consumerName);
    }

    public async Task<EventConsumerInfo> StartAsync(string consumerName,
        CancellationToken ct = default)
    {
        var state = await GetStateAsync(consumerName, ct);

        await messaging.PublishAsync(new EventConsumerStart(consumerName), ct: ct);

        return state.Value.ToInfo(consumerName);
    }

    public async Task<EventConsumerInfo> StopAsync(string consumerName,
        CancellationToken ct = default)
    {
        var state = await GetStateAsync(consumerName, ct);

        await messaging.PublishAsync(new EventConsumerStop(consumerName), ct: ct);

        return state.Value.ToInfo(consumerName);
    }

    private async Task<SimpleState<EventConsumerState>> GetStateAsync(string consumerName,
        CancellationToken ct)
    {
        var state = new SimpleState<EventConsumerState>(persistence, GetType(), DomainId.Create(consumerName));

        await state.LoadAsync(ct);

        if (state.Version <= EtagVersion.Empty || !activeNames.Contains(consumerName))
        {
            throw new DomainObjectNotFoundException(consumerName);
        }

        return state;
    }
}
