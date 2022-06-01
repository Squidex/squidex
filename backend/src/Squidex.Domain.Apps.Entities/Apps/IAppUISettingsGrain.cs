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
        Task<J<JsonValue>> GetAsync();

        Task SetAsync(string path, J<JsonValue> value);

        Task SetAsync(J<JsonValue> settings);

        Task RemoveAsync(string path);

        Task ClearAsync();
    }
}
