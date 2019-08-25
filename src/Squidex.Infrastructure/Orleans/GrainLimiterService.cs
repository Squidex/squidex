// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Orleans
{
    [Reentrant]
    public sealed class GrainLimiterService : GrainService, IGrainLimiterService
    {
        private readonly IGrainFactory grainFactory;
        private readonly Dictionary<Type, IGrainTypeRegistration> registrations = new Dictionary<Type, IGrainTypeRegistration>();

        private interface IGrainTypeRegistration
        {
            void Register(Guid id);

            void Register(string id);

            void Unregister(Guid id);

            void Unregister(string id);
        }

        private sealed class GuidGrainTypeRegistration<T> : IGrainTypeRegistration where T : IDeactivatableGrain, IGrainWithGuidKey
        {
            private readonly LRUCache<Guid, Guid> recentUsedGrains;
            private readonly IGrainFactory grainFactory;

            public GuidGrainTypeRegistration(int capacity, IGrainFactory grainFactory)
            {
                Guard.NotNull(grainFactory, nameof(grainFactory));

                this.grainFactory = grainFactory;

                recentUsedGrains = new LRUCache<Guid, Guid>(capacity, (key, _) => OnGrainEvicted(key));
            }

            public void Register(Guid id)
            {
                recentUsedGrains.Set(id, id);
            }

            public void Unregister(Guid id)
            {
                recentUsedGrains.Remove(id);
            }

            public void Register(string id)
            {
            }

            public void Unregister(string id)
            {
            }

            private void OnGrainEvicted(Guid id)
            {
                var grain = grainFactory.GetGrain<T>(id);

                grain.DeactivateAsync().Forget();
            }
        }

        private sealed class StringGrainTypeRegistration<T> : IGrainTypeRegistration where T : IDeactivatableGrain, IGrainWithStringKey
        {
            private readonly LRUCache<string, string> recentUsedGrains;
            private readonly IGrainFactory grainFactory;

            public StringGrainTypeRegistration(int capacity, IGrainFactory grainFactory)
            {
                Guard.NotNull(grainFactory, nameof(grainFactory));

                this.grainFactory = grainFactory;

                recentUsedGrains = new LRUCache<string, string>(capacity, (key, _) => OnGrainEvicted(key));
            }

            public void Register(string id)
            {
                recentUsedGrains.Set(id, id);
            }

            public void Unregister(string id)
            {
                recentUsedGrains.Remove(id);
            }

            public void Register(Guid id)
            {
            }

            public void Unregister(Guid id)
            {
            }

            private void OnGrainEvicted(string id)
            {
                var grain = grainFactory.GetGrain<T>(id);

                grain.DeactivateAsync().Forget();
            }
        }

        public GrainLimiterService(IGrainIdentity grainId, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory)
            : base(grainId, silo, loggerFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public Task RegisterAsync(Type type, Guid id, int limit)
        {
            var registration = TryGetGuidRegistration(type, true, limit);

            registration.Register(id);

            return Task.CompletedTask;
        }

        public Task RegisterAsync(Type type, string id, int limit)
        {
            var registration = TryGetStringRegistration(type, true, limit);

            registration.Register(id);

            return Task.CompletedTask;
        }

        public Task UnregisterAsync(Type type, Guid id)
        {
            var registration = TryGetGuidRegistration(type);

            if (registration != null)
            {
                registration.Unregister(id);
            }

            return Task.CompletedTask;
        }

        public Task UnregisterAsync(Type type, string id)
        {
            var registration = TryGetStringRegistration(type);

            if (registration != null)
            {
                registration.Unregister(id);
            }

            return Task.CompletedTask;
        }

        private IGrainTypeRegistration TryGetGuidRegistration(Type type, bool create = false, int limit = 0)
        {
            if (!registrations.TryGetValue(type, out var result) && create)
            {
                var genericType = typeof(GuidGrainTypeRegistration<>).MakeGenericType(type);

                result = (IGrainTypeRegistration)Activator.CreateInstance(genericType, limit, grainFactory);

                registrations[type] = result;
            }

            return result;
        }

        private IGrainTypeRegistration TryGetStringRegistration(Type type, bool create = false, int limit = 0)
        {
            if (!registrations.TryGetValue(type, out var result) && create)
            {
                var genericType = typeof(StringGrainTypeRegistration<>).MakeGenericType(type);

                result = (IGrainTypeRegistration)Activator.CreateInstance(genericType, limit, grainFactory);

                registrations[type] = result;
            }

            return result;
        }
    }
}
