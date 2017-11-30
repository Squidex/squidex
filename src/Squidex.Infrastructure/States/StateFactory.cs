// ==========================================================================
//  StateFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

#pragma warning disable RECS0096 // Type parameter is never used

namespace Squidex.Infrastructure.States
{
    public sealed class StateFactory : DisposableObjectBase, IExternalSystem, IStateFactory
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);
        private readonly IPubSub pubSub;
        private readonly IStateStore store;
        private readonly IMemoryCache statesCache;
        private readonly IServiceProvider services;
        private readonly object lockObject = new object();
        private IDisposable pubSubscription;

        public sealed class ObjectHolder<T, TState> where T : StatefulObject<TState>
        {
            private readonly Task activationTask;
            private readonly T obj;

            public ObjectHolder(T obj, IStateHolder<TState> stateHolder)
            {
                this.obj = obj;

                activationTask = obj.ActivateAsync(stateHolder);
            }

            public Task<T> ActivateAsync()
            {
                return activationTask.ContinueWith(x => obj);
            }
        }

        public StateFactory(
            IPubSub pubSub,
            IServiceProvider services,
            IStateStore store,
            IMemoryCache statesCache)
        {
            Guard.NotNull(pubSub, nameof(pubSub));
            Guard.NotNull(store, nameof(store));
            Guard.NotNull(services, nameof(services));
            Guard.NotNull(statesCache, nameof(statesCache));

            this.pubSub = pubSub;
            this.store = store;
            this.services = services;
            this.statesCache = statesCache;
        }

        public void Connect()
        {
            pubSubscription = pubSub.Subscribe<InvalidateMessage>(m =>
            {
                lock (lockObject)
                {
                    statesCache.Remove(m.Key);
                }
            });
        }

        public async Task<T> GetDetachedAsync<T, TState>(string key) where T : StatefulObject<TState>
        {
            Guard.NotNull(key, nameof(key));

            var stateHolder = new StateHolder<TState>(key, () => { }, store);
            var state = (T)services.GetService(typeof(T));

            await state.ActivateAsync(stateHolder);

            return state;
        }

        public Task<T> GetAsync<T, TState>(string key) where T : StatefulObject<TState>
        {
            Guard.NotNull(key, nameof(key));

            lock (lockObject)
            {
                if (statesCache.TryGetValue<T>(key, out var state))
                {
                    return Task.FromResult(state);
                }

                state = (T)services.GetService(typeof(T));

                var stateHolder = new StateHolder<TState>(key, () =>
                {
                    pubSub.Publish(new InvalidateMessage { Key = key }, false);
                }, store);

                statesCache.CreateEntry(key)
                    .SetValue(state)
                    .SetAbsoluteExpiration(CacheDuration)
                    .Dispose();

                var stateObj = new ObjectHolder<T, TState>(state, stateHolder);

                return stateObj.ActivateAsync();
            }
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                pubSubscription.Dispose();
            }
        }
    }
}
