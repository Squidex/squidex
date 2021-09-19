// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents.Counter
{
    public sealed class CounterGrain : GrainOfString, ICounterGrain
    {
        private readonly IGrainState<State> state;

        [CollectionName("Counters")]
        public sealed class State
        {
            public Dictionary<string, long> Counters { get; set; } = new Dictionary<string, long>();
        }

        public CounterGrain(IGrainState<State> state)
        {
            this.state = state;
        }

        public Task ClearAsync()
        {
            TryDeactivateOnIdle();

            return state.ClearAsync();
        }

        public Task<long> IncrementAsync(string name)
        {
            state.Value.Counters.TryGetValue(name, out var value);

            return ResetAsync(name, value + 1);
        }

        public async Task<long> ResetAsync(string name, long value)
        {
            state.Value.Counters[name] = value;

            await state.WriteAsync();

            return value;
        }
    }
}
