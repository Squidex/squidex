// ==========================================================================
//  StateHolder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.States
{
    public sealed class StateHolder<T> : IStateHolder<T>
    {
        private readonly Action written;
        private readonly IStateStore store;
        private readonly string key;
        private string etag;

        public T State { get; set; }

        public StateHolder(string key, Action written, IStateStore store)
        {
            this.key = key;
            this.store = store;
            this.written = written;
        }

        public async Task ReadAsync()
        {
            (State, etag) = await store.ReadAsync<T>(key);

            if (Equals(State, default(T)))
            {
                State = Activator.CreateInstance<T>();
            }
        }

        public async Task WriteAsync()
        {
            var newEtag = Guid.NewGuid().ToString();

            await store.WriteAsync(key, State, etag, newEtag);

            etag = newEtag;
        }
    }
}
