// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppUISettingsGrain : GrainOfGuid, IAppUISettingsGrain
    {
        private readonly IStore<Guid> store;
        private IPersistence<State> persistence;
        private State state = new State();

        [CollectionName("UISettings")]
        public sealed class State
        {
            public JObject Settings { get; set; } = new JObject();
        }

        public AppUISettingsGrain(IStore<Guid> store)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        public override Task OnActivateAsync(Guid key)
        {
            persistence = store.WithSnapshots<State, Guid>(GetType(), key, x => state = x);

            return persistence.ReadAsync();
        }

        public Task<J<JObject>> GetAsync()
        {
            return Task.FromResult(state.Settings.AsJ());
        }

        public Task SetAsync(J<JObject> settings)
        {
            state.Settings = settings;

            return persistence.WriteSnapshotAsync(state);
        }

        public Task SetAsync(string path, J<JToken> value)
        {
            var container = GetContainer(path, out var key);

            if (container == null)
            {
                throw new InvalidOperationException("Path does not lead to an object.");
            }

            container[key] = value;

            return persistence.WriteSnapshotAsync(state);
        }

        public Task RemoveAsync(string path)
        {
            var container = GetContainer(path, out var key);

            if (container != null)
            {
                container.Remove(key);
            }

            return persistence.WriteSnapshotAsync(state);
        }

        private JObject GetContainer(string path, out string key)
        {
            Guard.NotNullOrEmpty(path, nameof(path));

            var segments = path.Split('.');

            key = segments[segments.Length - 1];

            var current = state.Settings;

            if (segments.Length > 1)
            {
                foreach (var segment in segments.Take(segments.Length - 1))
                {
                    if (!current.TryGetValue(segment, out var temp))
                    {
                        temp = new JObject();

                        current[segment] = temp;
                    }

                    if (temp is JObject next)
                    {
                        current = next;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return current;
        }
    }
}
