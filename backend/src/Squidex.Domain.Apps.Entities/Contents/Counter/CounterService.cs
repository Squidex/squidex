// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents.Counter
{
    public sealed class CounterService : ICounterService, IDeleter
    {
        private readonly IPersistenceFactory<State> persistenceFactory;

        [CollectionName("Counters")]
        public sealed class State
        {
            public Dictionary<string, long> Counters { get; set; } = new Dictionary<string, long>();

            public void Increment(string name)
            {
                Counters[name] = Counters.GetValueOrDefault(name) + 1;
            }

            public void Reset(string name, long value)
            {
                Counters[name] = value;
            }
        }

        public CounterService(IPersistenceFactory<State> persistenceFactory)
        {
            this.persistenceFactory = persistenceFactory;
        }

        public async Task<long> IncrementAsync(DomainId appId, string name)
        {
            var state = await GetStateAsync(appId);

            await state.UpdateAsync(x => x.Increment(name));

            return state.Value.Counters[name];
        }

        public async Task<long> ResetAsync(DomainId appId, string name, long value)
        {
            var state = await GetStateAsync(appId);

            await state.UpdateAsync(x => x.Reset(name, value));

            return state.Value.Counters[name];
        }

        public async Task ClearAsync(DomainId appId)
        {
            var state = await GetStateAsync(appId);

            await state.UpdateAsync(x => x.Counters.Clear());
        }

        public async Task DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            var state = await GetStateAsync(app.Id);

            await state.ClearAsync(ct);
        }

        private async Task<SimpleState<State>> GetStateAsync(DomainId appId)
        {
            var state = new SimpleState<State>(persistenceFactory, GetType(), appId);

            await state.LoadAsync();

            return state;
        }
    }
}
