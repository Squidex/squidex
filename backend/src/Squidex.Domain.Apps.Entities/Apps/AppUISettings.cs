// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppUISettings : IAppUISettings, IDeleter
    {
        private readonly IGrainFactory grainFactory;

        public AppUISettings(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        async Task IDeleter.DeleteContributorAsync(DomainId appId, string contributorId,
            CancellationToken ct)
        {
            await GetGrain(appId, null).ClearAsync();
        }

        async Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            await GetGrain(app.Id, null).ClearAsync();

            foreach (var userId in app.Contributors.Keys)
            {
                await GetGrain(app.Id, userId).ClearAsync();
            }
        }

        public Task<JsonObject> GetAsync(DomainId appId, string? userId)
        {
            return GetGrain(appId, userId).GetAsync();
        }

        public Task RemoveAsync(DomainId appId, string? userId, string path)
        {
            return GetGrain(appId, userId).RemoveAsync(path);
        }

        public Task SetAsync(DomainId appId, string? userId, string path, JsonValue value)
        {
            return GetGrain(appId, userId).SetAsync(path, value);
        }

        public Task SetAsync(DomainId appId, string? userId, JsonObject settings)
        {
            return GetGrain(appId, userId).SetAsync(settings);
        }

        public Task ClearAsync(DomainId appId, string? userId)
        {
            return GetGrain(appId, userId).ClearAsync();
        }

        private IAppUISettingsGrain GetGrain(DomainId appId, string? userId)
        {
            return grainFactory.GetGrain<IAppUISettingsGrain>(GetKey(appId, userId));
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
