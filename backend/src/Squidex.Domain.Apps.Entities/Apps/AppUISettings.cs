// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppUISettings : IAppUISettings, IDeleter
    {
        private readonly IPersistenceFactory<State> persistanceFactory;

        [CollectionName("UISettings")]
        public sealed class State
        {
            public JsonObject Settings { get; set; } = new JsonObject();

            public void Set(JsonObject settings)
            {
                Settings = settings;
            }

            public void Set(string path, JsonValue value)
            {
                var container = GetContainer(path, true, out var key);

                if (container == null)
                {
                    ThrowHelper.InvalidOperationException("Path does not lead to an object.");
                    return;
                }

                container[key] = value;
            }

            public void Remove(string path)
            {
                var container = GetContainer(path, false, out var key);

                if (container == null)
                {
                    return;
                }

                if (container?.ContainsKey(key) == true)
                {
                    container.Remove(key);
                }
            }

            private JsonObject? GetContainer(string path, bool add, out string key)
            {
                Guard.NotNullOrEmpty(path);

                var segments = path.Split('.');

                key = segments[^1];

                var current = Settings;

                if (segments.Length > 1)
                {
                    foreach (var segment in segments.Take(segments.Length - 1))
                    {
                        if (!current.TryGetValue(segment, out var found))
                        {
                            if (add)
                            {
                                found = new JsonObject();

                                current[segment] = found;
                            }
                            else
                            {
                                return null;
                            }
                        }

                        if (found.Value is JsonObject o)
                        {
                            current = o;
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

        public AppUISettings(IPersistenceFactory<State> persistanceFactory)
        {
            this.persistanceFactory = persistanceFactory;
        }

        async Task IDeleter.DeleteContributorAsync(DomainId appId, string contributorId,
            CancellationToken ct)
        {
            await ClearAsync(appId, null);
        }

        async Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            await ClearAsync(app.Id, null);

            foreach (var userId in app.Contributors.Keys)
            {
                await ClearAsync(app.Id, userId);
            }
        }

        public async Task<JsonObject> GetAsync(DomainId appId, string? userId)
        {
            var state = await GetStateAsync(appId, userId);

            return state.Value.Settings;
        }

        public async Task RemoveAsync(DomainId appId, string? userId, string path)
        {
            var state = await GetStateAsync(appId, userId);

            await state.UpdateAsync(s => s.Remove(path));
        }

        public async Task SetAsync(DomainId appId, string? userId, string path, JsonValue value)
        {
            var state = await GetStateAsync(appId, userId);

            await state.UpdateAsync(s => s.Set(path, value));
        }

        public async Task SetAsync(DomainId appId, string? userId, JsonObject settings)
        {
            var state = await GetStateAsync(appId, userId);

            await state.UpdateAsync(s => s.Set(settings));
        }

        public async Task ClearAsync(DomainId appId, string? userId)
        {
            var state = await GetStateAsync(appId, userId);

            await state.ClearAsync();
        }

        private async Task<SimpleState<State>> GetStateAsync(DomainId appId, string? userId)
        {
            var state = new SimpleState<State>(persistanceFactory, GetType(), GetKey(appId, userId));

            await state.LoadAsync();

            return state;
        }

        private static string GetKey(DomainId appId, string? userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                return $"{appId}_{userId}";
            }
            else
            {
                return $"{appId}";
            }
        }
    }
}
