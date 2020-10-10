// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Threading;
using Squidex.Caching;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class ActivationLimiter : IActivationLimiter
    {
        private readonly ConcurrentDictionary<Type, LastUsedInstances> instances = new ConcurrentDictionary<Type, LastUsedInstances>();

        private sealed class LastUsedInstances
        {
            private readonly LRUCache<IDeactivater, IDeactivater> recentUsedGrains;
            private readonly ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();

            public LastUsedInstances(int limit)
            {
                recentUsedGrains = new LRUCache<IDeactivater, IDeactivater>(limit, (key, _) => key.Deactivate());
            }

            public void Register(IDeactivater instance)
            {
                try
                {
                    lockSlim.EnterWriteLock();

                    recentUsedGrains.Set(instance, instance);
                }
                finally
                {
                    lockSlim.ExitWriteLock();
                }
            }

            public void Unregister(IDeactivater instance)
            {
                try
                {
                    lockSlim.EnterWriteLock();

                    recentUsedGrains.Remove(instance);
                }
                finally
                {
                    lockSlim.ExitWriteLock();
                }
            }
        }

        public void Register(Type grainType, IDeactivater deactivater, int maxActivations)
        {
            var byType = instances.GetOrAdd(grainType, t => new LastUsedInstances(maxActivations));

            byType.Register(deactivater);
        }

        public void Unregister(Type grainType, IDeactivater deactivater)
        {
            instances.TryGetValue(grainType, out var byType);

            byType?.Unregister(deactivater);
        }
    }
}
