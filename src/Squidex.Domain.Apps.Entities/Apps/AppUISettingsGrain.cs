﻿// ==========================================================================
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
    public sealed class AppUISettingsGrain : GrainOfGuid, IAppUISettingsGrain
    {
        private readonly IGrainState<GrainState> state;

        [CollectionName("UISettings")]
        public sealed class GrainState
        {
            public JsonObject Settings { get; set; } = JsonValue.Object();
        }

        public AppUISettingsGrain(IGrainState<GrainState> state)
        {
            Guard.NotNull(state, nameof(state));

            this.state = state;
        }

        public Task<J<JsonObject>> GetAsync()
        {
            return Task.FromResult(state.Value.Settings.AsJ());
        }

        public Task SetAsync(J<JsonObject> settings)
        {
            state.Value.Settings = settings;

            return state.WriteAsync();
        }

        public Task SetAsync(string path, J<IJsonValue> value)
        {
            var container = GetContainer(path, out var key);

            if (container == null)
            {
                throw new InvalidOperationException("Path does not lead to an object.");
            }

            container[key] = value.Value;

            return state.WriteAsync();
        }

        public Task RemoveAsync(string path)
        {
            var container = GetContainer(path, out var key);

            if (container != null)
            {
                container.Remove(key);
            }

            return state.WriteAsync();
        }

        private JsonObject GetContainer(string path, out string key)
        {
            Guard.NotNullOrEmpty(path, nameof(path));

            var segments = path.Split('.');

            key = segments[segments.Length - 1];

            var current = state.Value.Settings;

            if (segments.Length > 1)
            {
                foreach (var segment in segments.Take(segments.Length - 1))
                {
                    if (!current.TryGetValue(segment, out var temp))
                    {
                        temp = JsonValue.Object();

                        current[segment] = temp;
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
