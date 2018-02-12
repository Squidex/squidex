// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Squidex.Infrastructure.EventSourcing;

#pragma warning disable RECS0096 // Type parameter is never used

namespace Squidex.Infrastructure.States
{
    public sealed class StateFactory : DisposableObjectBase, IInitializable, IStateFactory
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly IPubSub pubSub;
        private readonly IMemoryCache statesCache;
        private readonly IServiceProvider services;
        private readonly IStreamNameResolver streamNameResolver;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly object lockObject = new object();
        private IDisposable pubSubSubscription;

        public sealed class ObjectHolder<T, TKey> where T : IStatefulObject<TKey>
        {
            private readonly Task activationTask;
            private readonly T obj;

            public ObjectHolder(T obj, TKey key, IStore<TKey> store)
            {
                this.obj = obj;

                activationTask = obj.ActivateAsync(key, store);
            }

            public async Task<T> ActivateAsync()
            {
                await activationTask;

                return obj;
            }
        }

        public StateFactory(
            IPubSub pubSub,
            IMemoryCache statesCache,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IServiceProvider services,
            IStreamNameResolver streamNameResolver)
        {
            Guard.NotNull(services, nameof(services));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventDataFormatter, nameof(eventDataFormatter));
            Guard.NotNull(pubSub, nameof(pubSub));
            Guard.NotNull(statesCache, nameof(statesCache));
            Guard.NotNull(streamNameResolver, nameof(streamNameResolver));

            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.pubSub = pubSub;
            this.services = services;
            this.statesCache = statesCache;
            this.streamNameResolver = streamNameResolver;
        }

        public void Initialize()
        {
            pubSubSubscription = pubSub.Subscribe<InvalidateMessage>(m =>
            {
                lock (lockObject)
                {
                    statesCache.Remove(m.Key);
                }
            });
        }

        public Task<T> CreateAsync<T>(string key) where T : IStatefulObject<string>
        {
            return CreateAsync<T, string>(key);
        }

        public Task<T> CreateAsync<T>(Guid key) where T : IStatefulObject<Guid>
        {
            return CreateAsync<T, Guid>(key);
        }

        public async Task<T> CreateAsync<T, TKey>(TKey key) where T : IStatefulObject<TKey>
        {
            Guard.NotNull(key, nameof(key));

            var stateStore = new Store<TKey>(eventStore, eventDataFormatter, services, streamNameResolver);
            var state = (T)services.GetService(typeof(T));

            await state.ActivateAsync(key, stateStore);

            return state;
        }

        public Task<T> GetSingleAsync<T>(string key) where T : IStatefulObject<string>
        {
            return GetSingleAsync<T, string>(key);
        }

        public Task<T> GetSingleAsync<T>(Guid key) where T : IStatefulObject<Guid>
        {
            return GetSingleAsync<T, Guid>(key);
        }

        public Task<T> GetSingleAsync<T, TKey>(TKey key) where T : IStatefulObject<TKey>
        {
            Guard.NotNull(key, nameof(key));

            lock (lockObject)
            {
                if (statesCache.TryGetValue<ObjectHolder<T, TKey>>(key, out var stateObj))
                {
                    return stateObj.ActivateAsync();
                }

                var state = (T)services.GetService(typeof(T));
                var stateStore = new Store<TKey>(eventStore, eventDataFormatter, services, streamNameResolver);

                stateObj = new ObjectHolder<T, TKey>(state, key, stateStore);

                statesCache.CreateEntry(key)
                    .SetValue(stateObj)
                    .SetAbsoluteExpiration(CacheDuration)
                    .Dispose();

                return stateObj.ActivateAsync();
            }
        }

        public void Remove<T, TKey>(TKey key) where T : IStatefulObject<TKey>
        {
            statesCache.Remove(key);
        }

        public void Synchronize<T, TKey>(TKey key) where T : IStatefulObject<TKey>
        {
            pubSub.Publish(new InvalidateMessage { Key = key.ToString() }, false);
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing && pubSubSubscription != null)
            {
                pubSubSubscription.Dispose();
            }
        }
    }
}
