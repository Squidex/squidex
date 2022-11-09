// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents.Counter;

public sealed class CounterService : ICounterService, IDeleter
{
    private readonly IPersistenceFactory<State> persistenceFactory;

    [CollectionName("Counters")]
    public sealed class State
    {
        public Dictionary<string, long> Counters { get; set; } = new Dictionary<string, long>();

        public bool Increment(string name)
        {
            Counters[name] = Counters.GetValueOrDefault(name) + 1;

            return true;
        }

        public bool Reset(string name, long value)
        {
            Counters[name] = value;

            return true;
        }
    }

    public CounterService(IPersistenceFactory<State> persistenceFactory)
    {
        this.persistenceFactory = persistenceFactory;
    }

    async Task IDeleter.DeleteAppAsync(IAppEntity app,
        CancellationToken ct)
    {
        var state = await GetStateAsync(app.Id, ct);

        await state.ClearAsync(ct);
    }

    public async Task<long> IncrementAsync(DomainId appId, string name,
        CancellationToken ct = default)
    {
        var state = await GetStateAsync(appId, ct);

        await state.UpdateAsync(x => x.Increment(name), ct: ct);

        return state.Value.Counters[name];
    }

    public async Task<long> ResetAsync(DomainId appId, string name, long value,
        CancellationToken ct = default)
    {
        var state = await GetStateAsync(appId, ct);

        await state.UpdateAsync(x => x.Reset(name, value), ct: ct);

        return state.Value.Counters[name];
    }

    private async Task<SimpleState<State>> GetStateAsync(DomainId appId,
        CancellationToken ct)
    {
        var state = new SimpleState<State>(persistenceFactory, GetType(), appId);

        await state.LoadAsync(ct);

        return state;
    }
}
