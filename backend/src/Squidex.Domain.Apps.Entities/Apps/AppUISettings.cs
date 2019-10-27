// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppUISettings : IAppUISettings
    {
        private readonly IGrainFactory grainFactory;

        public AppUISettings(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory);

            this.grainFactory = grainFactory;
        }

        public async Task<JsonObject> GetAsync(Guid appId, string? userId)
        {
            var result = await GetGrain(appId, userId).GetAsync();

            return result.Value;
        }

        public Task RemoveAsync(Guid appId, string? userId, string path)
        {
            return GetGrain(appId, userId).RemoveAsync(path);
        }

        public Task SetAsync(Guid appId, string? userId, string path, IJsonValue value)
        {
            return GetGrain(appId, userId).SetAsync(path, value.AsJ());
        }

        public Task SetAsync(Guid appId, string? userId, JsonObject settings)
        {
            return GetGrain(appId, userId).SetAsync(settings.AsJ());
        }

        private IAppUISettingsGrain GetGrain(Guid appId, string? userId)
        {
            return grainFactory.GetGrain<IAppUISettingsGrain>(Key(appId, userId));
        }

        private string Key(Guid appId, string? userId)
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
