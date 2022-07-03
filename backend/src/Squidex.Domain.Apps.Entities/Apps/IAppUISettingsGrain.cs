// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppUISettingsGrain : IGrainWithStringKey
    {
        Task<JsonObject> GetAsync();

        Task SetAsync(string path, JsonValue value);

        Task SetAsync(JsonObject settings);

        Task RemoveAsync(string path);

        Task ClearAsync();
    }
}
