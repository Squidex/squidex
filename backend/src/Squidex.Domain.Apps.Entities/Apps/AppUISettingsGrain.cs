// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppUISettingsGrain : GrainOfString, IAppUISettingsGrain
    {
        private readonly IGrainState<State> state;

        [CollectionName("UISettings")]
        public sealed class State
        {
            public JsonObject Settings { get; set; } = JsonValue.Object();
        }

        public AppUISettingsGrain(IGrainState<State> state)
        {
            this.state = state;
        }

        public Task<J<JsonObject>> GetAsync()
        {
            return Task.FromResult(state.Value.Settings.AsJ());
        }

        public Task ClearAsync()
        {
            TryDeactivateOnIdle();

            return state.ClearAsync();
        }

        public Task SetAsync(J<JsonObject> settings)
        {
            state.Value.Settings = settings;

            return state.WriteAsync();
        }

        public Task SetAsync(string path, J<IJsonValue> value)
        {
            var container = GetContainer(path, true, out var key);

            if (container == null)
            {
                throw new InvalidOperationException("Path does not lead to an object.");
            }

            container[key] = value.Value;

            return state.WriteAsync();
        }

        public async Task RemoveAsync(string path)
        {
            var container = GetContainer(path, false, out var key);

            if (container?.ContainsKey(key) == true)
            {
                container.Remove(key);

                await state.WriteAsync();
            }
        }

        private JsonObject? GetContainer(string path, bool add, out string key)
        {
            Guard.NotNullOrEmpty(path, nameof(path));

            var segments = path.Split('.');

            key = segments[^1];

            var current = state.Value.Settings;

            if (segments.Length > 1)
            {
                foreach (var segment in segments.Take(segments.Length - 1))
                {
                    if (!current.TryGetValue(segment, out var temp))
                    {
                        if (add)
                        {
                            temp = JsonValue.Object();

                            current[segment] = temp;
                        }
                        else
                        {
                            return null;
                        }
                    }

                    if (temp is JsonObject next)
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
