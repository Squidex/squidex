// ==========================================================================
//  StateFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.States
{
    public sealed class StateFactory : DisposableObjectBase, IExternalSystem, IStateFactory
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly IPubSub pubSub;
        private readonly IStateStore store;
        private readonly IMemoryCache statesCache;
        private readonly IServiceProvider services;
        private readonly List<IDisposable> states = new List<IDisposable>();
        private readonly SingleThreadedDispatcher cleanupDispatcher = new SingleThreadedDispatcher();
        private IDisposable pubSubscription;

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
                statesCache.Remove(m.Key);
            });
        }

        public async Task<T> GetAsync<T, TState>(string key) where T : StatefulObject<TState>
        {
            Guard.NotNull(key, nameof(key));

            if (statesCache.TryGetValue<T>(key, out var state))
            {
                return state;
            }

            state = (T)services.GetService(typeof(T));

            var stateHolder = new StateHolder<TState>(key, () =>
            {
                pubSub.Publish(new InvalidateMessage { Key = key }, false);
            }, store);

            await state.ActivateAsync(stateHolder);

            var stateEntry = statesCache.CreateEntry(key);

            stateEntry.Value = state;
            stateEntry.AbsoluteExpirationRelativeToNow = CacheDuration;

            stateEntry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
            {
                EvictionCallback = (k, v, r, s) =>
                {
                    cleanupDispatcher.DispatchAsync(() =>
                    {
                        state.Dispose();
                        states.Remove(state);
                    }).Forget();
                }
            });

            states.Add(state);

            return state;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                cleanupDispatcher.DispatchAsync(() =>
                {
                    foreach (var state in states)
                    {
                        state.Dispose();
                    }
                });
                cleanupDispatcher.StopAndWaitAsync().Wait();
            }
        }
    }
}
