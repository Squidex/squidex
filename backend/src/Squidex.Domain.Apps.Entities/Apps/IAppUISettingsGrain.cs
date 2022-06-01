// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppUISettingsGrain : IGrainWithStringKey
    {
        Task<J<JsonObject>> GetAsync();

        Task SetAsync(string path, J<JsonValue> value);

        Task SetAsync(J<JsonObject> settings);

        Task RemoveAsync(string path);

        Task ClearAsync();
    }
}
