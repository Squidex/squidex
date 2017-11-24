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
        private readonly SingleThreadedDispatcher dispatcher = new SingleThreadedDispatcher();
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

        public Task<T> GetAsync<T, TState>(string key) where T : StatefulObject<TState>
        {
            Guard.NotNull(key, nameof(key));

            var tcs = new TaskCompletionSource<T>();

            dispatcher.DispatchAsync(async () =>
            {
                try
                {
                    if (statesCache.TryGetValue<T>(key, out var state))
                    {
                        tcs.SetResult(state);
                    }
                    else
                    {
                        state = (T)services.GetService(typeof(T));

                        var stateHolder = new StateHolder<TState>(key, () =>
                        {
                            pubSub.Publish(new InvalidateMessage { Key = key }, false);
                        }, store);

                        await state.ActivateAsync(stateHolder);

                        statesCache.CreateEntry(key)
                            .SetValue(state)
                            .SetAbsoluteExpiration(CacheDuration)
                            .RegisterPostEvictionCallback((k, v, r, s) =>
                            {
                                dispatcher.DispatchAsync(() =>
                                {
                                    state.Dispose();
                                    states.Remove(state);
                                }).Forget();
                            })
                            .Dispose();

                        states.Add(state);

                        tcs.SetResult(state);
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                dispatcher.DispatchAsync(() =>
                {
                    foreach (var state in states)
                    {
                        state.Dispose();
                    }
                });
                dispatcher.StopAndWaitAsync().Wait();
            }
        }
    }
}
