// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Orleans;
using Squidex.Infrastructure.Caching;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class ActivationLimiter : DisposableObjectBase, IActivationLimiter
    {
        private readonly IGrainFactory grainFactory;
        private readonly ConcurrentDictionary<Type, LastUsedIds<Guid>> registrationsGuid = new ConcurrentDictionary<Type, LastUsedIds<Guid>>();
        private readonly ConcurrentDictionary<Type, LastUsedIds<string>> registrationsString = new ConcurrentDictionary<Type, LastUsedIds<string>>();
        private readonly ActionBlock<IDeactivater> deactivaterQueue;

        private interface IDeactivater
        {
            Task DeactivateAsync(IGrainFactory grainFactory);
        }

        private class GuidDeactivater<T> : IDeactivater where T : IGrainWithGuidKey, IDeactivatableGrain
        {
            private readonly Guid key;

            public GuidDeactivater(Guid key)
            {
                this.key = key;
            }

            public Task DeactivateAsync(IGrainFactory grainFactory)
            {
                var grain = grainFactory.GetGrain<T>(key);

                return grain.DeactivateAsync();
            }
        }

        private class StringDeactivater<T> : IDeactivater where T : IGrainWithStringKey, IDeactivatableGrain
        {
            private readonly string key;

            public StringDeactivater(string key)
            {
                this.key = key;
            }

            public Task DeactivateAsync(IGrainFactory grainFactory)
            {
                var grain = grainFactory.GetGrain<T>(key);

                return grain.DeactivateAsync();
            }
        }

        private sealed class LastUsedIds<T>
        {
            private readonly LRUCache<T, T> recentUsedGrains;
            private readonly ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();

            public LastUsedIds(int capacity, Action<T, T> evicted)
            {
                recentUsedGrains = new LRUCache<T, T>(capacity, evicted);
            }

            public void Register(T id)
            {
                try
                {
                    lockSlim.EnterWriteLock();

                    recentUsedGrains.Set(id, id);
                }
                finally
                {
                    lockSlim.ExitWriteLock();
                }
            }

            public void Unregister(T id)
            {
                try
                {
                    lockSlim.EnterWriteLock();

                    recentUsedGrains.Remove(id);
                }
                finally
                {
                    lockSlim.ExitWriteLock();
                }
            }
        }

        public ActivationLimiter(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;

            deactivaterQueue = new ActionBlock<IDeactivater>(DeactivateAsync);
        }

        public void Register<T>(Guid id, int limit) where T : IGrainWithGuidKey, IDeactivatableGrain
        {
            var registration = GetGuidIds<T>(true, limit);

            registration.Register(id);
        }

        public void Register<T>(string id, int limit) where T : IGrainWithStringKey, IDeactivatableGrain
        {
            var registration = GetStringIds<T>(true, limit);

            registration?.Register(id);
        }

        public void Unregister<T>(Guid id) where T : IGrainWithGuidKey, IDeactivatableGrain
        {
            var registration = GetGuidIds<T>();

            registration?.Unregister(id);
        }

        public void Unregister<T>(string id) where T : IGrainWithStringKey, IDeactivatableGrain
        {
            var registration = GetStringIds<T>();

            registration?.Unregister(id);
        }

        private LastUsedIds<Guid> GetGuidIds<T>(bool create = false, int limit = 0) where T : IGrainWithGuidKey, IDeactivatableGrain
        {
            var type = typeof(T);

            if (!registrationsGuid.TryGetValue(type, out var result) && create)
            {
                result = new LastUsedIds<Guid>(limit, (key, _) =>
                {
                    deactivaterQueue.Post(new GuidDeactivater<T>(key));
                });

                if (!registrationsGuid.TryAdd(type, result))
                {
                    result = registrationsGuid[type];
                }
            }

            return result;
        }

        private LastUsedIds<string> GetStringIds<T>(bool create = false, int limit = 0) where T : IGrainWithStringKey, IDeactivatableGrain
        {
            var type = typeof(T);

            if (!registrationsString.TryGetValue(type, out var result) && create)
            {
                result = new LastUsedIds<string>(limit, (key, _) =>
                {
                    deactivaterQueue.Post(new StringDeactivater<T>(key));
                });

                if (!registrationsString.TryAdd(type, result))
                {
                    result = registrationsString[type];
                }
            }

            return result;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                deactivaterQueue.Complete();

                try
                {
                    deactivaterQueue.Completion.Wait(1500);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to wait for graceful shutdown {ex}.");
                }
            }
        }

        private async Task DeactivateAsync(IDeactivater deactivater)
        {
            try
            {
                await deactivater.DeactivateAsync(grainFactory);
            }
            catch
            {
                return;
            }
        }
    }
}
